using System.Text.Json;
using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Services
{
    public interface IPlantSearchService
    {
        Task<PlantSearchResponseDTO> SearchAsync(PlantSearchCriteriaDTO criteria);
    }

    /// <summary>
    /// Wyszukiwanie przez formularz. Dla każdej rośliny (i opcjonalnie zgłoszenia)
    /// liczy procent zgodności z cechami podanymi przez użytkownika.
    ///
    /// Porównywane są tylko pola, które użytkownik wypełnił. Wagi:
    ///   słowniki (kolor, kształt, typ, ...) = 3
    ///   Tak / Nie                            = 2
    ///   tekst i liczby                       = 1
    /// Wynik = suma wag zgodnych cech / suma wag wszystkich porównanych cech.
    /// </summary>
    public class PlantSearchService(HerbRecognitionDbContext data) : IPlantSearchService
    {
        private const double DictionaryWeight = 3;
        private const double FlagWeight = 2;
        private const double TextWeight = 1;
        private const double NumberWeight = 1;

        public async Task<PlantSearchResponseDTO> SearchAsync(PlantSearchCriteriaDTO criteria)
        {
            var minPercent = Math.Clamp(criteria.MinMatchPercent, 0, 100);

            // ile cech podał użytkownik – liczymy na pustej roślinie
            var criteriaCount = Compute(criteria, new CreatePlantDTO()).Compared;

            var response = new PlantSearchResponseDTO
            {
                CriteriaCount = criteriaCount,
                MinMatchPercent = minPercent
            };

            if (criteriaCount == 0)
                return response;

            // ---------- kandydaci: rośliny z bazy ----------
            var candidates = new List<Candidate>();

            var plants = await data.Plants
                .AsNoTracking()
                .Include(p => p.Stalk)
                .Include(p => p.Sap)
                .Include(p => p.Root)
                .Include(p => p.Hat)
                .Include(p => p.Leaf)
                .Include(p => p.Flower)
                .Include(p => p.Fruit)
                .ToListAsync();

            candidates.AddRange(plants.Select(p => new Candidate(p.Id, false, PlantMapper.ToCreateDto(p))));

            // ---------- kandydaci: zgłoszenia ----------
            if (criteria.IncludePending)
            {
                var submissions = await data.UserInputs.AsNoTracking().ToListAsync();

                foreach (var submission in submissions)
                {
                    var dto = TryDeserialize(submission.Inputdata);
                    if (dto is not null)
                        candidates.Add(new Candidate(submission.Id, true, dto));
                }
            }

            response.CheckedCount = candidates.Count;

            // ---------- słowniki do wyświetlenia wyników ----------
            var plantTypes = await data.Planttypes.ToDictionaryAsync(t => t.Id, t => t.Name);
            var months = await data.MonthOfHarvests.ToDictionaryAsync(m => m.Id, m => m.MonthName);

            // ---------- liczenie dopasowania ----------
            foreach (var candidate in candidates)
            {
                var score = Compute(criteria, candidate.Data);

                var percent = (int)Math.Round(score.MatchedWeight / score.TotalWeight * 100);
                if (percent < minPercent) continue;

                response.Results.Add(new PlantSearchResultDTO
                {
                    Id = candidate.Id,
                    IsPending = candidate.IsPending,
                    Name = candidate.Data.Name,
                    PolishName = candidate.Data.PolishName,
                    LatinName = candidate.Data.LatinName,
                    Subriquet = candidate.Data.Subriquet,
                    PlantTypeName = plantTypes.GetValueOrDefault(candidate.Data.PlantTypeID),
                    MonthOfHarvestName = candidate.Data.MonthOfHarvestId is int m ? months.GetValueOrDefault(m) : null,
                    MatchPercent = percent,
                    MatchedCount = score.Matched,
                    ComparedCount = score.Compared
                });
            }

            response.Results = response.Results
                .OrderByDescending(r => r.MatchPercent)
                .ThenBy(r => r.IsPending)               // przy remisie najpierw zatwierdzone
                .ThenBy(r => r.PolishName)
                .ToList();

            return response;
        }

        // =====================================================================
        //  Porównanie jednej rośliny z kryteriami
        // =====================================================================

        private static Score Compute(PlantSearchCriteriaDTO c, CreatePlantDTO p)
        {
            var s = new Score();

            // ----- nazwy: wystarczy, że pasuje do którejkolwiek nazwy rośliny -----
            var names = new[] { p.Name, p.PolishName, p.LatinName, p.Subriquet };
            AnyName(s, c.Name, names);
            AnyName(s, c.PolishName, names);
            AnyName(s, c.LatinName, names);
            AnyName(s, c.Subriquet, names);
            Description(s, c.MoreInfo, p.MoreInfo);

            // ----- dane podstawowe -----
            DictionaryField(s, c.PlantTypeId, p.PlantTypeID);
            DictionaryField(s, c.OccuranceId, p.OccuranceId);
            DictionaryField(s, c.MonthOfHarvestId, p.MonthOfHarvestId);

            if (c.PoisonabilityId is int poison)
            {
                // 0 = użytkownik twierdzi, że roślina nie jest trująca
                s.Add(DictionaryWeight, poison == 0
                    ? p.PoisonabilityId is null
                    : p.PoisonabilityId == poison);
            }

            // ----- części: jeśli roślina nie ma danej części, podane cechy się nie zgadzają -----
            if (c.Stalk is { } stalk)
            {
                var x = p.CreateStalkDTO;
                DictionaryField(s, stalk.ShapeId, x?.StalkShapeId);
                DictionaryField(s, stalk.ColorId, x?.StalkColorId);
                DictionaryField(s, stalk.SurfaceId, x?.StalkSurfaceId);
                DictionaryField(s, stalk.MonthOfHarvestId, x?.StalkMonthOfHarvestId);
                Flag(s, stalk.IsHollow, x?.StalkIsHollow);
                Description(s, stalk.MoreInfo, x?.StalkMoreInfo);
            }

            if (c.Sap is { } sap)
            {
                var x = p.CreateSapDTO;
                DictionaryField(s, sap.ColorId, x?.SapColorId);
                DictionaryField(s, sap.MonthOfHarvestId, x?.SapMonthOfHarvestId);
                Flag(s, sap.LeavesStain, x?.SapLeavesStain);
                Flag(s, sap.Sticky, x?.SapSticky);
                Description(s, sap.MoreInfo, x?.SapMoreInfo);
            }

            if (c.Root is { } root)
            {
                var x = p.CreateRootDTO;
                DictionaryField(s, root.ColorId, x?.RootColorId);
                DictionaryField(s, root.SurfaceId, x?.RootSurfaceId);
                DictionaryField(s, root.ThicknessId, x?.RootThicknessId);
                DictionaryField(s, root.MonthOfHarvestId, x?.RootMonthOfHarvestId);
                Flag(s, root.IsHollow, x?.RootIsHollow);
                Description(s, root.MoreInfo, x?.RootMoreInfo);
            }

            if (c.Hat is { } hat)
            {
                var x = p.CreateHatDTO;
                DictionaryField(s, hat.ColorId, x?.HatColorId);
                DictionaryField(s, hat.ShapeId, x?.HatShapeId);
                DictionaryField(s, hat.ThicknessId, x?.HatThicknessId);
                DictionaryField(s, hat.SurfaceId, x?.HatSurfaceId);
                DictionaryField(s, hat.MonthOfHarvestId, x?.HatMonthOfHarvestId);
                Flag(s, hat.HasSpots, x?.HatHasSpots);
                Flag(s, hat.HasGills, x?.HatHasGills);
                Flag(s, hat.HasRing, x?.HatHasRing);
                Description(s, hat.MoreInfo, x?.HatMoreInfo);
            }

            if (c.Leaf is { } leaf)
            {
                var x = p.CreateLeafDTO;
                DictionaryField(s, leaf.ShapeId, x?.LeafShapeId);
                DictionaryField(s, leaf.ColorId, x?.LeafColorId);
                DictionaryField(s, leaf.SurfaceId, x?.LeafSurfaceId);
                DictionaryField(s, leaf.ThicknessId, x?.LeafThicknessId);
                DictionaryField(s, leaf.FlavourId, x?.LeafFlavourId);
                DictionaryField(s, leaf.MonthOfHarvestId, x?.LeafMonthOfHarvestId);
                Flag(s, leaf.HasStripes, x?.LeafStripes);
                Flag(s, leaf.HasSpots, x?.LeafSpots);
                Flag(s, leaf.HasHoles, x?.LeafHoles);
                Size(s, leaf.Size, x?.LeafLength);
                Description(s, leaf.MoreInfo, x?.LeafMoreInfo);
            }

            if (c.Flower is { } flower)
            {
                var x = p.CreateFlowerDTO;
                DictionaryField(s, flower.ColorId, x?.FlowerColorId);
                DictionaryField(s, flower.ShapeId, x?.FlowerShapeId);
                DictionaryField(s, flower.FlavourId, x?.FlowerFlavourId);
                DictionaryField(s, flower.MonthOfHarvestId, x?.FlowerMonthOfHarvestId);
                Size(s, flower.Size, x?.FlowerSizeInCm);
                Scent(s, flower.ScentPower, x?.FlowerScentPower);
                Description(s, flower.MoreInfo, x?.FlowerMoreInfo);
            }

            if (c.Fruit is { } fruit)
            {
                var x = p.CreateFruitDTO;
                DictionaryField(s, fruit.FlavourId, x?.FruitFlavourId);
                DictionaryField(s, fruit.ShapeId, x?.FruitShapeId);
                DictionaryField(s, fruit.ColorId, x?.FruitColorId);
                DictionaryField(s, fruit.SurfaceId, x?.FruitSurfaceId);
                DictionaryField(s, fruit.ThicknessId, x?.FruitThicknessId);
                DictionaryField(s, fruit.MonthOfHarvestId, x?.FruitMonthOfHarvestId);
                Description(s, fruit.MoreInfo, x?.FruitMoreInfo);
            }

            return s;
        }

        // ----- pojedyncze porównania (pole niewypełnione = pomijamy) -----

        private static void DictionaryField(Score s, int? wanted, int? actual)
        {
            if (wanted is not int w || w <= 0) return;
            s.Add(DictionaryWeight, actual == w);
        }

        private static void Flag(Score s, bool? wanted, bool? actual)
        {
            if (wanted is null) return;
            s.Add(FlagWeight, actual == wanted);
        }

        private static void AnyName(Score s, string? wanted, string?[] names)
        {
            if (string.IsNullOrWhiteSpace(wanted)) return;
            s.Add(TextWeight, names.Any(n => FuzzyText.IsSimilar(n, wanted)));
        }

        private static void Description(Score s, string? wanted, string? actual)
        {
            if (string.IsNullOrWhiteSpace(wanted)) return;
            s.Add(TextWeight, FuzzyText.SharesWords(actual, wanted));
        }

        /// <summary>Rozmiar w cm: zgodny, jeśli różnica ≤ 20% (minimum 0,5 cm).</summary>
        private static void Size(Score s, decimal? wanted, decimal? actual)
        {
            if (wanted is not decimal w || w <= 0) return;

            var tolerance = Math.Max(w * 0.2m, 0.5m);
            s.Add(NumberWeight, actual is decimal a && Math.Abs(a - w) <= tolerance);
        }

        /// <summary>Siła zapachu 0–5: zgodna, jeśli różnica ≤ 1.</summary>
        private static void Scent(Score s, int? wanted, int? actual)
        {
            if (wanted is not int w) return;
            s.Add(NumberWeight, actual is int a && Math.Abs(a - w) <= 1);
        }

        // =====================================================================

        private static CreatePlantDTO? TryDeserialize(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<CreatePlantDTO>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private sealed record Candidate(int Id, bool IsPending, CreatePlantDTO Data);

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
