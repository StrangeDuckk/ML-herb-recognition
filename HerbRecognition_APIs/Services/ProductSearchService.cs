using System.Text.Json;
using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Services
{
    public interface IProductSearchService
    {
        Task<ProductSearchResponseDTO> SearchAsync(ProductSearchCriteriaDTO criteria);
    }

    /// <summary>
    /// Wyszukiwanie przepisów przez formularz – tak samo jak PlantSearchService.
    /// Porównywane są tylko pola, które użytkownik wypełnił. Wagi:
    ///   typ produktu, każda wybrana roślina (także z kwarantanny) / choroba / właściwość / substancja = 3
    ///   nazwa i słowa z przepisu                                              = 1
    /// Wynik = suma wag zgodnych cech / suma wag wszystkich porównanych cech.
    /// Przeciwwskazania to filtr: przepis z wykluczonym przeciwwskazaniem nie trafia do wyników.
    /// </summary>
    public class ProductSearchService(HerbRecognitionDbContext data, IProductService products) : IProductSearchService
    {
        private const double DictionaryWeight = 3;
        private const double TextWeight = 1;

        public async Task<ProductSearchResponseDTO> SearchAsync(ProductSearchCriteriaDTO criteria)
        {
            Normalize(criteria);

            var minPercent = Math.Clamp(criteria.MinMatchPercent, 0, 100);
            var excluded = criteria.ExcludeContraindicationIds ?? new List<int>();

            // ile cech podał użytkownik – liczymy na pustym przepisie
            var comparedCount = Compute(criteria, Candidate.Empty).Compared;

            var response = new ProductSearchResponseDTO
            {
                CriteriaCount = comparedCount + excluded.Count,
                MinMatchPercent = minPercent
            };

            if (response.CriteriaCount == 0)
                return response;

            // ---------- kandydaci: przepisy w bazie ----------
            var candidates = await data.Products
                .AsNoTracking()
                .Select(p => new Candidate(
                    p.Id,
                    false,
                    p.Name,
                    p.Recipe,
                    p.Producttypeid,
                    p.PlantProducts.Where(pp => pp.Plantsid != null).Select(pp => pp.Plantsid!.Value).ToList(),
                    p.PlantProducts.Where(pp => pp.UserInputId != null).Select(pp => pp.UserInputId!.Value).ToList(),
                    p.Diseases.Select(d => d.Id).ToList(),
                    p.Healthproperties.Select(h => h.Id).ToList(),
                    p.Activesubstances.Select(a => a.Id).ToList(),
                    p.Contraindications.Select(c => c.Id).ToList()))
                .ToListAsync();

            // ---------- kandydaci: zgłoszenia ----------
            if (criteria.IncludePending)
            {
                var inputs = await data.ProductInputs.AsNoTracking().ToListAsync();

                foreach (var input in inputs)
                {
                    var dto = TryDeserialize(input.Inputdata);
                    if (dto is null) continue;

                    candidates.Add(new Candidate(
                        input.Id,
                        true,
                        dto.Name ?? "",
                        dto.Recipe ?? "",
                        dto.ProductTypeId,
                        dto.PlantIds ?? new List<int>(),
                        dto.PlantUserInputIds ?? new List<int>(),
                        dto.DiseaseIds ?? new List<int>(),
                        dto.HealthPropertyIds ?? new List<int>(),
                        dto.ActiveSubstanceIds ?? new List<int>(),
                        dto.ContraindicationIds ?? new List<int>()));
                }
            }

            response.CheckedCount = candidates.Count;

            // ---------- dopasowanie ----------
            var matches = new List<(Candidate Candidate, Score Score, int Percent)>();

            foreach (var candidate in candidates)
            {
                // filtr bezpieczeństwa
                if (candidate.ContraindicationIds.Any(excluded.Contains))
                    continue;

                var score = Compute(criteria, candidate);

                // tylko wykluczenia, bez innych cech – każdy "bezpieczny" przepis pasuje w 100%
                var percent = score.TotalWeight == 0
                    ? 100
                    : (int)Math.Round(score.MatchedWeight / score.TotalWeight * 100);

                if (percent >= minPercent)
                    matches.Add((candidate, score, percent));
            }

            if (matches.Count == 0)
                return response;

            // ---------- dane do kafelków (nazwy roślin, chorób, substancji, miniatury) ----------
            var approvedPreviews = (await products.GetProductsAsync()).ToDictionary(p => p.Id);
            var pendingPreviews = matches.Any(m => m.Candidate.IsPending)
                ? (await products.GetPendingPreviewsAsync()).ToDictionary(p => p.Id)
                : new Dictionary<int, ProductPreviewDTO>();

            foreach (var (candidate, score, percent) in matches)
            {
                var preview = candidate.IsPending
                    ? pendingPreviews.GetValueOrDefault(candidate.Id)
                    : approvedPreviews.GetValueOrDefault(candidate.Id);

                if (preview is null) continue;

                response.Results.Add(new ProductSearchResultDTO
                {
                    Id = preview.Id,
                    Name = preview.Name,
                    ProductTypeName = preview.ProductTypeName,
                    Plants = preview.Plants,
                    Diseases = preview.Diseases,
                    HealthProperties = preview.HealthProperties,
                    ActiveSubstances = preview.ActiveSubstances,
                    MainThumbnailUrl = preview.MainThumbnailUrl,
                    IsPending = candidate.IsPending,
                    MatchPercent = percent,
                    MatchedCount = score.Matched,
                    ComparedCount = score.Compared
                });
            }

            response.Results = response.Results
                .OrderByDescending(r => r.MatchPercent)
                .ThenBy(r => r.IsPending)        // przy remisie najpierw zatwierdzone
                .ThenBy(r => r.Name)
                .ToList();

            return response;
        }

        // =====================================================================
        //  Porównanie jednego przepisu z kryteriami
        // =====================================================================

        private static Score Compute(ProductSearchCriteriaDTO c, Candidate p)
        {
            var s = new Score();

            Name(s, c.Name, p.Name);
            Description(s, c.RecipeText, p.Recipe);

            DictionaryField(s, c.ProductTypeId, p.ProductTypeId);

            // każdy wybrany wpis liczony osobno – im więcej się zgadza, tym wyższy wynik
            EachOf(s, c.PlantIds, p.PlantIds);
            EachOf(s, c.PlantUserInputIds, p.PlantUserInputIds);
            EachOf(s, c.DiseaseIds, p.DiseaseIds);
            EachOf(s, c.HealthPropertyIds, p.HealthPropertyIds);
            EachOf(s, c.ActiveSubstanceIds, p.ActiveSubstanceIds);

            return s;
        }

        private static void DictionaryField(Score s, int? wanted, int actual)
        {
            if (wanted is not int w || w <= 0) return;
            s.Add(DictionaryWeight, actual == w);
        }

        private static void EachOf(Score s, List<int>? wanted, List<int> actual)
        {
            if (wanted is null) return;

            foreach (var id in wanted)
                s.Add(DictionaryWeight, actual.Contains(id));
        }

        private static void Name(Score s, string? wanted, string actual)
        {
            if (string.IsNullOrWhiteSpace(wanted)) return;
            s.Add(TextWeight, FuzzyText.IsSimilar(actual, wanted) || FuzzyText.SharesWords(actual, wanted));
        }

        private static void Description(Score s, string? wanted, string? actual)
        {
            if (string.IsNullOrWhiteSpace(wanted)) return;
            s.Add(TextWeight, FuzzyText.SharesWords(actual, wanted));
        }

        private static void Normalize(ProductSearchCriteriaDTO c)
        {
            c.Name = string.IsNullOrWhiteSpace(c.Name) ? null : c.Name.Trim();
            c.RecipeText = string.IsNullOrWhiteSpace(c.RecipeText) ? null : c.RecipeText.Trim();
            c.PlantIds = Clean(c.PlantIds);
            c.PlantUserInputIds = Clean(c.PlantUserInputIds);
            c.DiseaseIds = Clean(c.DiseaseIds);
            c.HealthPropertyIds = Clean(c.HealthPropertyIds);
            c.ActiveSubstanceIds = Clean(c.ActiveSubstanceIds);
            c.ExcludeContraindicationIds = Clean(c.ExcludeContraindicationIds);
        }

        private static List<int>? Clean(List<int>? ids)
        {
            var list = ids?.Where(x => x > 0).Distinct().Take(50).ToList();
            return list is { Count: > 0 } ? list : null;
        }

        private static CreateProductDTO? TryDeserialize(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<CreateProductDTO>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // =====================================================================

        private sealed record Candidate(
            int Id,
            bool IsPending,
            string Name,
            string Recipe,
            int ProductTypeId,
            List<int> PlantIds,
            List<int> PlantUserInputIds,
            List<int> DiseaseIds,
            List<int> HealthPropertyIds,
            List<int> ActiveSubstanceIds,
            List<int> ContraindicationIds)
        {
            public static readonly Candidate Empty =
                new(0, false, "", "", 0, new(), new(), new(), new(), new(), new());
        }

        private sealed class Score
        {
            public double TotalWeight { get; private set; }
            public double MatchedWeight { get; private set; }
            public int Compared { get; private set; }
            public int Matched { get; private set; }

            public void Add(double weight, bool isMatch)
            {
                TotalWeight += weight;
                Compared++;

                if (isMatch)
                {
                    MatchedWeight += weight;
                    Matched++;
                }
            }
        }
    }
}
