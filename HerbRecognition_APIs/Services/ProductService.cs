using System.Text.Json;
using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using HerbRecognition_APIs.Pictures;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Services
{
    /// <summary>Błąd danych produktu, który można pokazać użytkownikowi (400).</summary>
    public class ProductValidationException(string message) : Exception(message);

    public interface IProductService
    {
        // ----- publiczne -----
        Task<List<ProductPreviewDTO>> GetProductsAsync(int? plantId = null);
        Task<GetProductDTO?> GetProductAsync(int id);
        Task<List<ProductPreviewDTO>> GetPendingPreviewsAsync(int? plantId = null);
        Task<int> SubmitAsync(CreateProductDTO dto);

        // ----- admin: zgłoszenia (productinput) -----
        Task<List<ProductPreviewDTO>> GetSubmissionSummariesAsync();
        Task<CreateProductDTO?> GetSubmissionAsync(int id);
        Task<bool> UpdateSubmissionAsync(int id, CreateProductDTO dto);
        Task<bool> DeleteSubmissionAsync(int id);
        Task<int> ApproveSubmissionAsync(int id, CreateProductDTO dto);

        // ----- admin: produkty w bazie -----
        Task<List<ProductPreviewDTO>> GetProductSummariesAsync();
        Task<CreateProductDTO?> GetProductForEditAsync(int id);
        Task<bool> UpdateProductAsync(int id, CreateProductDTO dto);
        Task<bool> DeleteProductAsync(int id);
        Task<int> CreateProductAsync(CreateProductDTO dto);

        // ----- rośliny z kwarantanny wskazane w produktach -----
        Task OnPlantSubmissionApprovedAsync(int userInputId, int plantId);
        Task OnPlantSubmissionDeletedAsync(int userInputId);
    }

    public class ProductService(HerbRecognitionDbContext data, IPictureFileStore pictureFiles) : IProductService
    {
        private const int MaxPlants = 20;
        private const int MaxPerDictionary = 20;

        // =====================================================================
        //  PUBLICZNE
        // =====================================================================

        public Task<List<ProductPreviewDTO>> GetProductsAsync(int? plantId = null) =>
            QueryProductPreviewsAsync(plantId, includeAdminInfo: false);

        public async Task<GetProductDTO?> GetProductAsync(int id)
        {
            var row = await data.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Recipe,
                    TypeName = p.Producttype.Name,
                    Properties = p.Healthproperties.OrderBy(h => h.Property)
                        .Select(h => new NamedRefDTO { Id = h.Id, Name = h.Property }).ToList(),
                    Substances = p.Activesubstances.OrderBy(a => a.Activesubstance1)
                        .Select(a => new NamedRefDTO { Id = a.Id, Name = a.Activesubstance1 }).ToList(),
                    Diseases = p.Diseases.OrderBy(d => d.Name)
                        .Select(d => new DiseaseRefDTO { Id = d.Id, Name = d.Name, Symptoms = d.Symptoms }).ToList(),
                    Contraindications = p.Contraindications.OrderBy(c => c.Name)
                        .Select(c => new NamedRefDTO { Id = c.Id, Name = c.Name }).ToList(),
                    Plants = p.PlantProducts
                        .Select(pp => new PlantLinkRow(pp.Plantsid, pp.UserInputId,
                            pp.Plants != null ? pp.Plants.Polishname : null))
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (row is null) return null;

            var pendingNames = await PendingPlantNamesAsync(row.Plants.Select(x => x.UserInputId));

            return new GetProductDTO
            {
                Id = row.Id,
                Name = row.Name,
                Recipe = row.Recipe,
                ProductTypeName = row.TypeName,
                HealthProperties = row.Properties,
                ActiveSubstances = row.Substances,
                Diseases = row.Diseases,
                Contraindications = row.Contraindications,
                Plants = ToPlantRefs(row.Plants, pendingNames)
            };
        }

        /// <summary>Zgłoszenia produktów do biblioteki (bez zdjęć – nie były jeszcze sprawdzone).
        /// plantId: tylko zgłoszenia wskazujące tę zatwierdzoną roślinę.</summary>
        public async Task<List<ProductPreviewDTO>> GetPendingPreviewsAsync(int? plantId = null)
        {
            var previews = await QuerySubmissionPreviewsAsync(includeAdminInfo: false);

            return previews
                .Where(p => !p.IsCorrupted)
                .Where(p => plantId is null || p.Plants.Any(x => x.IsApproved && x.Id == plantId))
                .ToList();
        }

        public async Task<int> SubmitAsync(CreateProductDTO dto)
        {
            await NormalizeAsync(dto);
            await ValidateAsync(dto, requireResolved: false);

            var input = new ProductInput { Inputdata = Serialize(dto) };
            data.ProductInputs.Add(input);
            await data.SaveChangesAsync();

            return input.Id;
        }

        // =====================================================================
        //  ADMIN: ZGŁOSZENIA
        // =====================================================================

        public Task<List<ProductPreviewDTO>> GetSubmissionSummariesAsync() =>
            QuerySubmissionPreviewsAsync(includeAdminInfo: true);

        public async Task<CreateProductDTO?> GetSubmissionAsync(int id)
        {
            var input = await data.ProductInputs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return input is null ? null : TryDeserialize(input.Inputdata);
        }

        public async Task<bool> UpdateSubmissionAsync(int id, CreateProductDTO dto)
        {
            var input = await data.ProductInputs.FirstOrDefaultAsync(x => x.Id == id);
            if (input is null) return false;

            await NormalizeAsync(dto);
            await ValidateAsync(dto, requireResolved: false);

            input.Inputdata = Serialize(dto);
            await data.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            var input = await data.ProductInputs.FirstOrDefaultAsync(x => x.Id == id);
            if (input is null) return false;

            var pictureLinks = await data.Pictures
                .Where(p => p.ProductInputId == id)
                .Select(p => p.Picturelink)
                .ToListAsync();

            data.ProductInputs.Remove(input);   // wiersze zdjęć usuwa kaskada w bazie
            await data.SaveChangesAsync();
            await transaction.CommitAsync();

            foreach (var link in pictureLinks)
                pictureFiles.Delete(link);

            return true;
        }

        /// <summary>Zgłoszenie -> produkt w bazie. Wszystkie propozycje nowych wpisów muszą być rozstrzygnięte.</summary>
        public async Task<int> ApproveSubmissionAsync(int id, CreateProductDTO dto)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            var input = await data.ProductInputs.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Zgłoszenie produktu #{id} nie istnieje.");

            var productId = await CreateFromDtoAsync(dto);

            // zdjęcia zgłoszenia przechodzą na nowy produkt
            await data.Pictures
                .Where(p => p.ProductInputId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.ProductId, (int?)productId)
                    .SetProperty(p => p.ProductInputId, (int?)null));

            data.ProductInputs.Remove(input);
            await data.SaveChangesAsync();
            await transaction.CommitAsync();

            return productId;
        }

        // =====================================================================
        //  ADMIN: PRODUKTY W BAZIE
        // =====================================================================

        public Task<List<ProductPreviewDTO>> GetProductSummariesAsync() =>
            QueryProductPreviewsAsync(plantId: null, includeAdminInfo: true);

        public async Task<CreateProductDTO?> GetProductForEditAsync(int id)
        {
            return await data.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new CreateProductDTO
                {
                    Name = p.Name,
                    Recipe = p.Recipe,
                    ProductTypeId = p.Producttypeid,
                    PlantIds = p.PlantProducts.Where(pp => pp.Plantsid != null)
                        .Select(pp => pp.Plantsid!.Value).ToList(),
                    PlantUserInputIds = p.PlantProducts.Where(pp => pp.UserInputId != null)
                        .Select(pp => pp.UserInputId!.Value).ToList(),
                    HealthPropertyIds = p.Healthproperties.Select(h => h.Id).ToList(),
                    ActiveSubstanceIds = p.Activesubstances.Select(a => a.Id).ToList(),
                    DiseaseIds = p.Diseases.Select(d => d.Id).ToList(),
                    ContraindicationIds = p.Contraindications.Select(c => c.Id).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateProductAsync(int id, CreateProductDTO dto)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            var product = await data.Products
                .Include(p => p.Healthproperties)
                .Include(p => p.Activesubstances)
                .Include(p => p.Diseases)
                .Include(p => p.Contraindications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return false;

            await NormalizeAsync(dto);
            await ValidateAsync(dto, requireResolved: true);

            product.Name = dto.Name;
            product.Recipe = dto.Recipe;
            product.Producttypeid = dto.ProductTypeId;
            await ApplyDictionariesAsync(product, dto);

            // rośliny: najprościej i bez ryzyka duplikatów – od nowa
            await data.PlantProducts.Where(pp => pp.Productsid == id).ExecuteDeleteAsync();
            foreach (var link in BuildPlantLinks(dto))
            {
                link.Productsid = id;
                data.PlantProducts.Add(link);
            }

            await data.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            var product = await data.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return false;

            var pictureLinks = await data.Pictures
                .Where(p => p.ProductId == id)
                .Select(p => p.Picturelink)
                .ToListAsync();

            // tabele łączące, plant_product i wiersze zdjęć usuwa kaskada w bazie
            data.Products.Remove(product);
            await data.SaveChangesAsync();
            await transaction.CommitAsync();

            foreach (var link in pictureLinks)
                pictureFiles.Delete(link);

            return true;
        }

        public async Task<int> CreateProductAsync(CreateProductDTO dto)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();
            var productId = await CreateFromDtoAsync(dto);
            await transaction.CommitAsync();
            return productId;
        }

        // =====================================================================
        //  ROŚLINY Z KWARANTANNY W PRODUKTACH
        //  Wywoływane z SubmissionService w jego transakcji (ten sam DbContext).
        // =====================================================================

        /// <summary>Zgłoszenie rośliny zatwierdzone -> powiązania wskazują nową roślinę.</summary>
        public async Task OnPlantSubmissionApprovedAsync(int userInputId, int plantId)
        {
            // produkty w bazie
            await data.PlantProducts
                .Where(pp => pp.UserInputId == userInputId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(pp => pp.Plantsid, (int?)plantId)
                    .SetProperty(pp => pp.UserInputId, (int?)null));

            // zgłoszenia produktów
            foreach (var input in await FindSubmissionsReferencingPlantSubmissionAsync(userInputId))
            {
                var dto = TryDeserialize(input.Inputdata);
                if (dto is null) continue;

                dto.PlantUserInputIds?.RemoveAll(x => x == userInputId);
                dto.PlantIds ??= new List<int>();
                if (!dto.PlantIds.Contains(plantId))
                    dto.PlantIds.Add(plantId);

                input.Inputdata = Serialize(dto);
            }

            await data.SaveChangesAsync();
        }

        /// <summary>Zgłoszenie rośliny usunięte -> znika ze zgłoszeń produktów.
        /// (Wiersze plant_product produktów w bazie usuwa kaskada.)</summary>
        public async Task OnPlantSubmissionDeletedAsync(int userInputId)
        {
            foreach (var input in await FindSubmissionsReferencingPlantSubmissionAsync(userInputId))
            {
                var dto = TryDeserialize(input.Inputdata);
                if (dto?.PlantUserInputIds is null) continue;

                dto.PlantUserInputIds.RemoveAll(x => x == userInputId);
                input.Inputdata = Serialize(dto);
            }

            await data.SaveChangesAsync();
        }

        // =====================================================================
        //  Tworzenie produktu
        // =====================================================================

        /// <summary>Tworzy produkt z DTO. Wywołujący odpowiada za transakcję.</summary>
        private async Task<int> CreateFromDtoAsync(CreateProductDTO dto)
        {
            await NormalizeAsync(dto);
            await ValidateAsync(dto, requireResolved: true);

            var product = new Product
            {
                Name = dto.Name,
                Recipe = dto.Recipe,
                Producttypeid = dto.ProductTypeId
            };

            await ApplyDictionariesAsync(product, dto);

            foreach (var link in BuildPlantLinks(dto))
                product.PlantProducts.Add(link);

            data.Products.Add(product);
            await data.SaveChangesAsync();

            return product.Id;
        }

        /// <summary>Ustawia cztery kolekcje słownikowe dokładnie na Id z DTO.</summary>
        private async Task ApplyDictionariesAsync(Product product, CreateProductDTO dto)
        {
            var hp = dto.HealthPropertyIds ?? new List<int>();
            var asIds = dto.ActiveSubstanceIds ?? new List<int>();
            var dIds = dto.DiseaseIds ?? new List<int>();
            var cIds = dto.ContraindicationIds ?? new List<int>();

            product.Healthproperties.Clear();
            foreach (var x in await data.Healthproperties.Where(h => hp.Contains(h.Id)).ToListAsync())
                product.Healthproperties.Add(x);

            product.Activesubstances.Clear();
            foreach (var x in await data.Activesubstances.Where(a => asIds.Contains(a.Id)).ToListAsync())
                product.Activesubstances.Add(x);

            product.Diseases.Clear();
            foreach (var x in await data.Diseases.Where(d => dIds.Contains(d.Id)).ToListAsync())
                product.Diseases.Add(x);

            product.Contraindications.Clear();
            foreach (var x in await data.Contraindications.Where(c => cIds.Contains(c.Id)).ToListAsync())
                product.Contraindications.Add(x);
        }

        private static IEnumerable<PlantProduct> BuildPlantLinks(CreateProductDTO dto)
        {
            foreach (var plantId in dto.PlantIds ?? new List<int>())
                yield return new PlantProduct { Plantsid = plantId };

            foreach (var userInputId in dto.PlantUserInputIds ?? new List<int>())
                yield return new PlantProduct { UserInputId = userInputId };
        }

        // =====================================================================
        //  Czyszczenie i walidacja DTO
        // =====================================================================

        /// <summary>Przycina teksty, usuwa duplikaty i puste wpisy, a propozycje,
        /// które już są w słowniku, zamienia na Id.</summary>
        private async Task NormalizeAsync(CreateProductDTO dto)
        {
            dto.Name = (dto.Name ?? "").Trim();
            dto.Recipe = (dto.Recipe ?? "").Trim();

            dto.PlantIds = CleanIds(dto.PlantIds);
            dto.PlantUserInputIds = CleanIds(dto.PlantUserInputIds);
            dto.HealthPropertyIds = CleanIds(dto.HealthPropertyIds);
            dto.ActiveSubstanceIds = CleanIds(dto.ActiveSubstanceIds);
            dto.DiseaseIds = CleanIds(dto.DiseaseIds);
            dto.ContraindicationIds = CleanIds(dto.ContraindicationIds);

            dto.NewHealthProperties = CleanTexts(dto.NewHealthProperties, 300, "Właściwość zdrowotna");
            dto.NewActiveSubstances = CleanTexts(dto.NewActiveSubstances, 300, "Substancja czynna");
            dto.NewContraindications = CleanTexts(dto.NewContraindications, 1000, "Przeciwwskazanie");
            dto.NewDiseases = CleanDiseases(dto.NewDiseases);

            if (dto.NewHealthProperties is not null)
            {
                var all = await data.Healthproperties.Select(h => new { h.Id, h.Property }).ToListAsync();
                (dto.NewHealthProperties, dto.HealthPropertyIds) =
                    MatchExisting(dto.NewHealthProperties, dto.HealthPropertyIds, all.Select(x => (x.Id, x.Property)));
            }

            if (dto.NewActiveSubstances is not null)
            {
                var all = await data.Activesubstances.Select(a => new { a.Id, a.Activesubstance1 }).ToListAsync();
                (dto.NewActiveSubstances, dto.ActiveSubstanceIds) =
                    MatchExisting(dto.NewActiveSubstances, dto.ActiveSubstanceIds, all.Select(x => (x.Id, x.Activesubstance1)));
            }

            if (dto.NewContraindications is not null)
            {
                var all = await data.Contraindications.Select(c => new { c.Id, c.Name }).ToListAsync();
                (dto.NewContraindications, dto.ContraindicationIds) =
                    MatchExisting(dto.NewContraindications, dto.ContraindicationIds, all.Select(x => (x.Id, x.Name)));
            }

            if (dto.NewDiseases is not null)
            {
                var all = await data.Diseases.Select(d => new { d.Id, d.Name }).ToListAsync();
                var lookup = ToLookup(all.Select(x => (x.Id, x.Name)));
                var ids = dto.DiseaseIds ?? new List<int>();
                var remaining = new List<NewDiseaseDTO>();

                foreach (var disease in dto.NewDiseases)
                {
                    if (lookup.TryGetValue(Key(disease.Name), out var existingId))
                    {
                        if (!ids.Contains(existingId)) ids.Add(existingId);
                    }
                    else
                    {
                        remaining.Add(disease);
                    }
                }

                dto.DiseaseIds = ids.Count > 0 ? ids : null;
                dto.NewDiseases = remaining.Count > 0 ? remaining : null;
            }
        }

        private async Task ValidateAsync(CreateProductDTO dto, bool requireResolved)
        {
            if (dto.Name.Length is < 2 or > 300)
                throw new ProductValidationException("Nazwa produktu musi mieć od 2 do 300 znaków.");

            if (dto.Recipe.Length < 10)
                throw new ProductValidationException("Opisz przepis – co najmniej 10 znaków.");

            if (dto.Recipe.Length > 10000)
                throw new ProductValidationException("Przepis może mieć najwyżej 10 000 znaków.");

            var plantCount = Count(dto.PlantIds) + Count(dto.PlantUserInputIds);
            if (plantCount == 0)
                throw new ProductValidationException("Wskaż przynajmniej jedną roślinę.");
            if (plantCount > MaxPlants)
                throw new ProductValidationException($"Produkt może mieć najwyżej {MaxPlants} roślin.");

            // przepis musi mówić, na co działa
            if (Count(dto.DiseaseIds) + Count(dto.NewDiseases) == 0)
                throw new ProductValidationException("Wskaż przynajmniej jedną chorobę, na którą działa produkt.");

            CheckLimit(Count(dto.HealthPropertyIds) + Count(dto.NewHealthProperties), "właściwości zdrowotnych");
            CheckLimit(Count(dto.ActiveSubstanceIds) + Count(dto.NewActiveSubstances), "substancji czynnych");
            CheckLimit(Count(dto.DiseaseIds) + Count(dto.NewDiseases), "chorób");
            CheckLimit(Count(dto.ContraindicationIds) + Count(dto.NewContraindications), "przeciwwskazań");

            if (!await data.Producttypes.AnyAsync(t => t.Id == dto.ProductTypeId))
                throw new KeyNotFoundException("Wybierz typ produktu.");

            await EnsureAllExistAsync(data.Plants.Select(p => p.Id), dto.PlantIds,
                "Część wskazanych roślin nie istnieje.");
            await EnsureAllExistAsync(data.UserInputs.Select(u => u.Id), dto.PlantUserInputIds,
                "Część wskazanych zgłoszeń roślin już nie istnieje.");
            await EnsureAllExistAsync(data.Healthproperties.Select(h => h.Id), dto.HealthPropertyIds,
                "Część wskazanych właściwości zdrowotnych nie istnieje.");
            await EnsureAllExistAsync(data.Activesubstances.Select(a => a.Id), dto.ActiveSubstanceIds,
                "Część wskazanych substancji czynnych nie istnieje.");
            await EnsureAllExistAsync(data.Diseases.Select(d => d.Id), dto.DiseaseIds,
                "Część wskazanych chorób nie istnieje.");
            await EnsureAllExistAsync(data.Contraindications.Select(c => c.Id), dto.ContraindicationIds,
                "Część wskazanych przeciwwskazań nie istnieje.");

            if (requireResolved)
            {
                var unresolved = new List<string>();
                unresolved.AddRange((dto.NewHealthProperties ?? new()).Select(x => $"właściwość „{x}”"));
                unresolved.AddRange((dto.NewActiveSubstances ?? new()).Select(x => $"substancja „{x}”"));
                unresolved.AddRange((dto.NewDiseases ?? new()).Select(x => $"choroba „{x.Name}”"));
                unresolved.AddRange((dto.NewContraindications ?? new()).Select(x => $"przeciwwskazanie „{x}”"));

                if (unresolved.Count > 0)
                    throw new ProductValidationException(
                        "Zanim zapiszesz produkt w bazie, przypisz nowe wpisy do słowników: " +
                        string.Join(", ", unresolved) + ".");
            }
        }

        private static void CheckLimit(int count, string what)
        {
            if (count > MaxPerDictionary)
                throw new ProductValidationException($"Produkt może mieć najwyżej {MaxPerDictionary} {what}.");
        }

        private static async Task EnsureAllExistAsync(IQueryable<int> existingIds, List<int>? wanted, string message)
        {
            if (wanted is not { Count: > 0 }) return;

            var found = await existingIds.CountAsync(id => wanted.Contains(id));
            if (found != wanted.Count)
                throw new KeyNotFoundException(message);
        }

        private static List<int>? CleanIds(IEnumerable<int>? ids)
        {
            var list = ids?.Where(x => x > 0).Distinct().ToList();
            return list is { Count: > 0 } ? list : null;
        }

        private static List<string>? CleanTexts(IEnumerable<string?>? texts, int maxLength, string what)
        {
            var list = new List<string>();

            foreach (var raw in texts ?? Enumerable.Empty<string?>())
            {
                var text = raw?.Trim();
                if (string.IsNullOrEmpty(text)) continue;

                if (text.Length > maxLength)
                    throw new ProductValidationException($"{what} może mieć najwyżej {maxLength} znaków.");

                if (!list.Any(x => Key(x) == Key(text)))
                    list.Add(text);
            }

            return list.Count > 0 ? list : null;
        }

        private static List<NewDiseaseDTO>? CleanDiseases(IEnumerable<NewDiseaseDTO?>? diseases)
        {
            var list = new List<NewDiseaseDTO>();

            foreach (var raw in diseases ?? Enumerable.Empty<NewDiseaseDTO?>())
            {
                var name = raw?.Name?.Trim();
                if (string.IsNullOrEmpty(name)) continue;

                if (name.Length > 300)
                    throw new ProductValidationException("Nazwa choroby może mieć najwyżej 300 znaków.");

                var symptoms = string.IsNullOrWhiteSpace(raw!.Symptoms) ? null : raw.Symptoms.Trim();
                if (symptoms is { Length: > 4000 })
                    throw new ProductValidationException("Opis objawów może mieć najwyżej 4000 znaków.");

                if (!list.Any(x => Key(x.Name) == Key(name)))
                    list.Add(new NewDiseaseDTO { Name = name, Symptoms = symptoms });
            }

            return list.Count > 0 ? list : null;
        }

        private static (List<string>? Remaining, List<int>? Ids) MatchExisting(
            List<string> names, List<int>? ids, IEnumerable<(int Id, string Name)> dictionary)
        {
            var lookup = ToLookup(dictionary);
            var resolved = ids?.ToList() ?? new List<int>();
            var remaining = new List<string>();

            foreach (var name in names)
            {
                if (lookup.TryGetValue(Key(name), out var id))
                {
                    if (!resolved.Contains(id)) resolved.Add(id);
                }
                else
                {
                    remaining.Add(name);
                }
            }

            return (remaining.Count > 0 ? remaining : null, resolved.Count > 0 ? resolved : null);
        }

        private static Dictionary<string, int> ToLookup(IEnumerable<(int Id, string Name)> dictionary) =>
            dictionary
                .GroupBy(x => Key(x.Name))
                .ToDictionary(g => g.Key, g => g.First().Id);

        private static string Key(string text) => text.Trim().ToLowerInvariant();

        private static int Count<T>(List<T>? list) => list?.Count ?? 0;

        // =====================================================================
        //  Listy (biblioteka, panel admina)
        // =====================================================================

        private sealed record PlantLinkRow(int? PlantId, int? UserInputId, string? PlantName);

        private async Task<List<ProductPreviewDTO>> QueryProductPreviewsAsync(int? plantId, bool includeAdminInfo)
        {
            var query = data.Products.AsNoTracking();

            if (plantId is int pid)
                query = query.Where(p => p.PlantProducts.Any(pp => pp.Plantsid == pid));

            var rows = await query
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    TypeName = p.Producttype.Name,
                    Diseases = p.Diseases.OrderBy(d => d.Name).Select(d => d.Name).ToList(),
                    Properties = p.Healthproperties.OrderBy(h => h.Property).Select(h => h.Property).ToList(),
                    Substances = p.Activesubstances.OrderBy(a => a.Activesubstance1).Select(a => a.Activesubstance1).ToList(),
                    Plants = p.PlantProducts
                        .Select(pp => new PlantLinkRow(pp.Plantsid, pp.UserInputId,
                            pp.Plants != null ? pp.Plants.Polishname : null))
                        .ToList()
                })
                .ToListAsync();

            var pendingNames = await PendingPlantNamesAsync(rows.SelectMany(r => r.Plants).Select(x => x.UserInputId));
            var pictures = await PictureInfoAsync(forSubmissions: false);

            return rows.Select(r =>
            {
                pictures.TryGetValue(r.Id, out var info);

                return new ProductPreviewDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    ProductTypeName = r.TypeName,
                    Diseases = r.Diseases,
                    HealthProperties = r.Properties,
                    ActiveSubstances = r.Substances,
                    Plants = ToPlantRefs(r.Plants, pendingNames),
                    MainThumbnailUrl = info.Thumbnail,
                    PictureCount = includeAdminInfo ? info.Count : 0
                };
            }).ToList();
        }

        private async Task<List<ProductPreviewDTO>> QuerySubmissionPreviewsAsync(bool includeAdminInfo)
        {
            var inputs = await data.ProductInputs.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
            var parsed = inputs.Select(i => (i.Id, Dto: TryDeserialize(i.Inputdata))).ToList();
            var valid = parsed.Where(x => x.Dto is not null).Select(x => x.Dto!).ToList();

            // nazwy pobierane raz dla wszystkich zgłoszeń
            var typeIds = valid.Select(d => d.ProductTypeId).Distinct().ToList();
            var plantIds = valid.SelectMany(d => d.PlantIds ?? new()).Distinct().ToList();
            var diseaseIds = valid.SelectMany(d => d.DiseaseIds ?? new()).Distinct().ToList();
            var propertyIds = valid.SelectMany(d => d.HealthPropertyIds ?? new()).Distinct().ToList();
            var substanceIds = valid.SelectMany(d => d.ActiveSubstanceIds ?? new()).Distinct().ToList();

            var types = await data.Producttypes.Where(t => typeIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Name);
            var plants = await data.Plants.Where(p => plantIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Polishname);
            var diseases = await data.Diseases.Where(d => diseaseIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name);
            var properties = await data.Healthproperties.Where(h => propertyIds.Contains(h.Id)).ToDictionaryAsync(h => h.Id, h => h.Property);
            var substances = await data.Activesubstances.Where(a => substanceIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, a => a.Activesubstance1);
            var pendingNames = await PendingPlantNamesAsync(valid.SelectMany(d => d.PlantUserInputIds ?? new()).Select(x => (int?)x));

            var pictures = includeAdminInfo
                ? await PictureInfoAsync(forSubmissions: true)
                : new Dictionary<int, (int Count, string? Thumbnail)>();

            return parsed.Select(x =>
            {
                if (x.Dto is null)
                {
                    return new ProductPreviewDTO
                    {
                        Id = x.Id,
                        Name = "Nieczytelne zgłoszenie",
                        IsPending = true,
                        IsCorrupted = true
                    };
                }

                var d = x.Dto;
                pictures.TryGetValue(x.Id, out var info);

                var plantRefs = (d.PlantIds ?? new())
                    .Select(id => new PlantRefDTO { Id = id, Name = plants.GetValueOrDefault(id) ?? $"Roślina #{id}", IsApproved = true })
                    .Concat((d.PlantUserInputIds ?? new())
                        .Select(id => new PlantRefDTO { Id = id, Name = pendingNames.GetValueOrDefault(id) ?? $"Zgłoszenie #{id}", IsApproved = false }))
                    .ToList();

                return new ProductPreviewDTO
                {
                    Id = x.Id,
                    Name = d.Name,
                    ProductTypeName = types.GetValueOrDefault(d.ProductTypeId),
                    Plants = plantRefs,
                    Diseases = (d.DiseaseIds ?? new()).Select(id => diseases.GetValueOrDefault(id) ?? $"#{id}")
                        .Concat((d.NewDiseases ?? new()).Select(n => n.Name))
                        .ToList(),
                    HealthProperties = (d.HealthPropertyIds ?? new()).Select(id => properties.GetValueOrDefault(id) ?? $"#{id}")
                        .Concat(d.NewHealthProperties ?? new())
                        .ToList(),
                    ActiveSubstances = (d.ActiveSubstanceIds ?? new()).Select(id => substances.GetValueOrDefault(id) ?? $"#{id}")
                        .Concat(d.NewActiveSubstances ?? new())
                        .ToList(),
                    MainThumbnailUrl = includeAdminInfo ? info.Thumbnail : null,   // publicznie bez zdjęć
                    PictureCount = info.Count,
                    NewEntriesCount = Count(d.NewHealthProperties) + Count(d.NewActiveSubstances)
                                    + Count(d.NewDiseases) + Count(d.NewContraindications),
                    IsPending = true
                };
            }).ToList();
        }

        private static List<PlantRefDTO> ToPlantRefs(IEnumerable<PlantLinkRow> links, Dictionary<int, string> pendingNames) =>
            links
                .Select(l => l.PlantId is int plantId
                    ? new PlantRefDTO { Id = plantId, Name = l.PlantName ?? $"Roślina #{plantId}", IsApproved = true }
                    : new PlantRefDTO
                    {
                        Id = l.UserInputId!.Value,
                        Name = pendingNames.GetValueOrDefault(l.UserInputId.Value) ?? $"Zgłoszenie #{l.UserInputId}",
                        IsApproved = false
                    })
                .OrderBy(p => !p.IsApproved)
                .ThenBy(p => p.Name)
                .ToList();

        /// <summary>Nazwy polskie roślin z kwarantanny (z JSON-a zgłoszenia).</summary>
        private async Task<Dictionary<int, string>> PendingPlantNamesAsync(IEnumerable<int?> userInputIds)
        {
            var ids = userInputIds.Where(x => x is not null).Select(x => x!.Value).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, string>();

            var submissions = await data.UserInputs.AsNoTracking().Where(u => ids.Contains(u.Id)).ToListAsync();
            var result = new Dictionary<int, string>();

            foreach (var submission in submissions)
            {
                try
                {
                    var plant = JsonSerializer.Deserialize<CreatePlantDTO>(submission.Inputdata);
                    if (!string.IsNullOrWhiteSpace(plant?.PolishName))
                        result[submission.Id] = plant.PolishName;
                }
                catch (JsonException)
                {
                    // nieczytelne zgłoszenie – zostanie pokazane jako "Zgłoszenie #id"
                }
            }

            return result;
        }

        /// <summary>Liczba zdjęć i miniatura zdjęcia głównego – produkty albo zgłoszenia produktów.</summary>
        private async Task<Dictionary<int, (int Count, string? Thumbnail)>> PictureInfoAsync(bool forSubmissions)
        {
            var query = data.Pictures.AsNoTracking();

            var rows = forSubmissions
                ? await query.Where(p => p.ProductInputId != null)
                    .Select(p => new { OwnerId = p.ProductInputId!.Value, p.Id, p.Picturelink, p.CzyGlowneZdjecie })
                    .ToListAsync()
                : await query.Where(p => p.ProductId != null)
                    .Select(p => new { OwnerId = p.ProductId!.Value, p.Id, p.Picturelink, p.CzyGlowneZdjecie })
                    .ToListAsync();

            return rows
                .GroupBy(r => r.OwnerId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var main = g.OrderByDescending(r => r.CzyGlowneZdjecie).ThenBy(r => r.Id).First();
                        return (g.Count(), (string?)pictureFiles.ThumbnailLink(main.Picturelink));
                    });
        }

        // =====================================================================
        //  JSON zgłoszeń
        // =====================================================================

        private static string Serialize(CreateProductDTO dto) => JsonSerializer.Serialize(dto);

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

        private Task<List<ProductInput>> FindSubmissionsReferencingPlantSubmissionAsync(int userInputId) =>
            data.ProductInputs
                .FromSqlInterpolated($@"
                    SELECT * FROM productinput
                    WHERE inputdata->'PlantUserInputIds' @> jsonb_build_array({userInputId})")
                .ToListAsync();
    }
}
