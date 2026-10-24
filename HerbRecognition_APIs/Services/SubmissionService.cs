using System.Text.Json;
using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Services
{
    public interface ISubmissionService
    {
        // ----- zgłoszenia (userinput) -----
        Task<List<UserInputSummaryDTO>> GetSummariesAsync();
        Task<CreatePlantDTO?> GetAsync(int id);
        Task<bool> UpdateAsync(int id, CreatePlantDTO dto);
        Task<bool> DeleteAsync(int id);

        /// <summary>Zgłoszenie -> roślina w bazie. Zwraca Id nowej rośliny.</summary>
        Task<int> ApproveAsync(int id, CreatePlantDTO dto);

        // ----- rośliny w bazie (plant) -----
        Task<List<UserInputSummaryDTO>> GetPlantSummariesAsync();
        Task<CreatePlantDTO?> GetPlantForEditAsync(int plantId);
        Task<bool> UpdatePlantAsync(int plantId, CreatePlantDTO dto);
        Task<bool> DeletePlantAsync(int plantId);

        /// <summary>Bezpośrednie dodanie rośliny przez admina (bez zgłoszenia). Zwraca Id nowej rośliny.</summary>
        Task<int> CreatePlantAsync(CreatePlantDTO dto);

        // ----- podobne rośliny -----
        Task<List<SimilarPlantCandidateDTO>> ResolveSimilarAsync(
            IEnumerable<int>? plantIds, IEnumerable<int>? userInputIds);
    }

    public class SubmissionService(HerbRecognitionDbContext data) : ISubmissionService
    {
        // =====================================================================
        //  JSON zgłoszeń – ten sam format co w PostUserInputPlantAsync
        // =====================================================================

        private static string Serialize(CreatePlantDTO dto) => JsonSerializer.Serialize(dto);

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

        // =====================================================================
        //  ZGŁOSZENIA: lista, odczyt, edycja, usuwanie
        // =====================================================================

        public async Task<List<UserInputSummaryDTO>> GetSummariesAsync()
        {
            var submissions = await data.UserInputs
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .ToListAsync();

            var plantTypes = await data.Planttypes.ToDictionaryAsync(t => t.Id, t => t.Name);
            var result = new List<UserInputSummaryDTO>();

            foreach (var submission in submissions)
            {
                var dto = TryDeserialize(submission.Inputdata);

                if (dto is null)
                {
                    result.Add(new UserInputSummaryDTO
                    {
                        Id = submission.Id,
                        PolishName = "Nieczytelne zgłoszenie",
                        IsCorrupted = true
                    });
                    continue;
                }

                result.Add(new UserInputSummaryDTO
                {
                    Id = submission.Id,
                    Name = dto.Name,
                    PolishName = dto.PolishName,
                    LatinName = dto.LatinName,
                    PlantTypeName = plantTypes.GetValueOrDefault(dto.PlantTypeID),
                    Parts = DescribeParts(dto.PlantTypeID,
                        dto.CreateStalkDTO is not null, dto.CreateSapDTO is not null,
                        dto.CreateRootDTO is not null, dto.CreateHatDTO is not null,
                        dto.CreateLeafDTO is not null, dto.CreateFlowerDTO is not null,
                        dto.CreateFruitDTO is not null),
                    SimilarCount = (dto.SimilarPlantIds?.Count ?? 0) + (dto.SimilarUserInputIds?.Count ?? 0)
                });
            }

            return result;
        }

        public async Task<CreatePlantDTO?> GetAsync(int id)
        {
            var submission = await data.UserInputs
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return submission is null ? null : TryDeserialize(submission.Inputdata);
        }

        public async Task<bool> UpdateAsync(int id, CreatePlantDTO dto)
        {
            var submission = await data.UserInputs.FirstOrDefaultAsync(u => u.Id == id);
            if (submission is null) return false;

            NormalizeSimilar(dto, selfUserInputId: id);
            await ValidateSimilarReferencesAsync(dto);

            submission.Inputdata = Serialize(dto);
            await data.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            var submission = await data.UserInputs.FirstOrDefaultAsync(u => u.Id == id);
            if (submission is null) return false;

            // inne zgłoszenia nie mogą wskazywać na usunięte
            foreach (var other in await FindSubmissionsReferencingSubmissionAsync(id))
            {
                var otherDto = TryDeserialize(other.Inputdata);
                if (otherDto?.SimilarUserInputIds is null) continue;

                otherDto.SimilarUserInputIds.RemoveAll(x => x == id);
                NormalizeSimilar(otherDto, selfUserInputId: other.Id);
                other.Inputdata = Serialize(otherDto);
            }

            data.UserInputs.Remove(submission);
            await data.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        // =====================================================================
        //  ZATWIERDZANIE i bezpośrednie dodawanie
        // =====================================================================

        public Task<int> ApproveAsync(int id, CreatePlantDTO dto) =>
            CreateAndRelinkAsync(id, dto);

        public Task<int> CreatePlantAsync(CreatePlantDTO dto) =>
            CreateAndRelinkAsync(null, dto);

        /// <summary>
        /// W jednej transakcji:
        /// 1. tworzy roślinę i jej części,
        /// 2. tworzy powiązania z zatwierdzonymi roślinami (w obie strony),
        /// 3. zgłoszenia wskazujące na zatwierdzane zgłoszenie dostają Id nowej rośliny,
        /// 4. zgłoszenia wskazane przez zatwierdzane dostają Id nowej rośliny,
        /// 5. usuwa zatwierdzane zgłoszenie z userinput.
        /// </summary>
        private async Task<int> CreateAndRelinkAsync(int? submissionId, CreatePlantDTO dto)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            try
            {
                UserInput? submission = null;

                if (submissionId is int id)
                {
                    submission = await data.UserInputs.FirstOrDefaultAsync(u => u.Id == id)
                        ?? throw new KeyNotFoundException($"Zgłoszenie #{id} nie istnieje.");
                }

                NormalizeSimilar(dto, selfUserInputId: submissionId);
                await ValidateSimilarReferencesAsync(dto);
                await ValidateDictionariesAsync(dto);

                // 1. roślina + części
                var plant = new Plant();
                ApplyPlantValues(plant, dto);
                data.Plants.Add(plant);
                await data.SaveChangesAsync();

                // 2. powiązania z zatwierdzonymi
                await SyncApprovedLinksAsync(plant.Id, dto.SimilarPlantIds);

                // 3. zgłoszenia, które wskazywały na zatwierdzane zgłoszenie
                if (submissionId is int approvedId)
                {
                    foreach (var other in await FindSubmissionsReferencingSubmissionAsync(approvedId))
                    {
                        var otherDto = TryDeserialize(other.Inputdata);
                        if (otherDto is null) continue;

                        otherDto.SimilarUserInputIds?.RemoveAll(x => x == approvedId);
                        AddPlantLink(otherDto, plant.Id);
                        NormalizeSimilar(otherDto, selfUserInputId: other.Id);
                        other.Inputdata = Serialize(otherDto);
                    }
                }

                // 4. zgłoszenia wskazane przez zatwierdzane
                await SyncPendingLinksAsync(plant.Id, dto.SimilarUserInputIds);

                // 5. zgłoszenie znika z kwarantanny
                if (submission is not null)
                    data.UserInputs.Remove(submission);

                await data.SaveChangesAsync();
                await transaction.CommitAsync();

                return plant.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =====================================================================
        //  ROŚLINY W BAZIE: lista, odczyt do edycji, edycja, usuwanie
        // =====================================================================

        public async Task<List<UserInputSummaryDTO>> GetPlantSummariesAsync()
        {
            var rows = await data.Plants
                .AsNoTracking()
                .OrderBy(p => p.Polishname)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Polishname,
                    p.Latinname,
                    p.Planttypeid,
                    TypeName = p.Planttype.Name,
                    HasStalk = p.Stalkid != null,
                    HasSap = p.Sapid != null,
                    HasRoot = p.Rootid != null,
                    HasHat = p.Hatid != null,
                    HasLeaf = p.Leafid != null,
                    HasFlower = p.Flowerid != null,
                    HasFruit = p.Fruitid != null,
                    SimilarCount = p.SimilarPlants.Count()
                })
                .ToListAsync();

            return rows.Select(r => new UserInputSummaryDTO
            {
                Id = r.Id,
                Name = r.Name,
                PolishName = r.Polishname,
                LatinName = r.Latinname,
                PlantTypeName = r.TypeName,
                Parts = DescribeParts(r.Planttypeid,
                    r.HasStalk, r.HasSap, r.HasRoot, r.HasHat, r.HasLeaf, r.HasFlower, r.HasFruit),
                SimilarCount = r.SimilarCount
            }).ToList();
        }

        /// <summary>Roślina z bazy w postaci CreatePlantDTO – do wczytania w formularz.</summary>
        public async Task<CreatePlantDTO?> GetPlantForEditAsync(int plantId)
        {
            var p = await PlantsWithParts()
                .AsNoTracking()
                .Include(x => x.SimilarPlants)
                .Include(x => x.SimilarTo)
                .FirstOrDefaultAsync(x => x.Id == plantId);

            if (p is null) return null;

            // zgłoszenia, które wskazują tę roślinę jako podobną
            var pendingIds = (await FindSubmissionsReferencingPlantAsync(plantId))
                .Select(u => u.Id)
                .ToList();

            var similarIds = p.SimilarPlants.Select(s => s.SimilarPlantId)
                .Union(p.SimilarTo.Select(s => s.PlantId))
                .Where(x => x != plantId)
                .ToList();

            return new CreatePlantDTO
            {
                Name = p.Name,
                PolishName = p.Polishname,
                LatinName = p.Latinname,
                Subriquet = p.Subriquet,
                MoreInfo = p.moreInfo,
                PlantTypeID = p.Planttypeid,
                OccuranceId = p.Occuranceid,
                PoisonabilityId = p.Poisonabilityid,
                MonthOfHarvestId = p.MonthOfHarvestId,

                SimilarPlantIds = similarIds.Count > 0 ? similarIds : null,
                SimilarUserInputIds = pendingIds.Count > 0 ? pendingIds : null,

                CreateStalkDTO = p.Stalk is null ? null : new CreateStalkDTO
                {
                    StalkShapeId = p.Stalk.Shapeid,
                    StalkColorId = p.Stalk.Colorid,
                    StalkSurfaceId = p.Stalk.Surfaceid,
                    StalkIsHollow = p.Stalk.isHollow,
                    StalkMoreInfo = p.Stalk.moreInfo,
                    StalkMonthOfHarvestId = p.Stalk.MonthOfHarvestId
                },
                CreateSapDTO = p.Sap is null ? null : new CreateSapDTO
                {
                    SapColorId = p.Sap.Colorid,
                    SapLeavesStain = p.Sap.Leavesstains,
                    SapSticky = p.Sap.Sticky,
                    SapMoreInfo = p.Sap.moreInfo,
                    SapMonthOfHarvestId = p.Sap.MonthOfHarvestId
                },
                CreateRootDTO = p.Root is null ? null : new CreateRootDTO
                {
                    RootColorId = p.Root.Colorid,
                    RootSurfaceId = p.Root.Surfaceid,
                    RootThicknessId = p.Root.Thicknessid,
                    RootIsHollow = p.Root.isHollow,
                    RootMoreInfo = p.Root.moreInfo,
                    RootMonthOfHarvestId = p.Root.MonthOfHarvestId
                },
                CreateHatDTO = p.Hat is null ? null : new CreateHatDTO
                {
                    HatColorId = p.Hat.Colorid,
                    HatShapeId = p.Hat.Shapeid,
                    HatThicknessId = p.Hat.Thicknessid,
                    HatSurfaceId = p.Hat.Surfaceid,
                    HatHasSpots = p.Hat.Hasspots,
                    HatHasGills = p.Hat.Hasgills,
                    HatHasRing = p.Hat.HasRing,
                    HatMoreInfo = p.Hat.moreInfo,
                    HatMonthOfHarvestId = p.Hat.MonthOfHarvestId
                },
                CreateLeafDTO = p.Leaf is null ? null : new CreateLeafDTO
                {
                    LeafStripes = p.Leaf.Stripes,
                    LeafSpots = p.Leaf.Spots,
                    LeafHoles = p.Leaf.Holes,
                    LeafShapeId = p.Leaf.Leafshapeid,
                    LeafColorId = p.Leaf.Leafcolorid,
                    LeafSurfaceId = p.Leaf.Surfaceid,
                    LeafThicknessId = p.Leaf.Thicknessid,
                    LeafFlavourId = p.Leaf.Flavourid,
                    LeafLength = p.Leaf.Leaflength,
                    LeafMoreInfo = p.Leaf.moreInfo,
                    LeafMonthOfHarvestId = p.Leaf.MonthOfHarvestId
                },
                CreateFlowerDTO = p.Flower is null ? null : new CreateFlowerDTO
                {
                    FlowerSizeInCm = p.Flower.Sizeincm,
                    FlowerColorId = p.Flower.Colorid,
                    FlowerShapeId = p.Flower.Shapeid,
                    FlowerFlavourId = p.Flower.Flavourid,
                    FlowerScentPower = p.Flower.Scentpower,
                    FlowerMoreInfo = p.Flower.moreInfo,
                    FlowerMonthOfHarvestId = p.Flower.MonthOfHarvestId
                },
                CreateFruitDTO = p.Fruit is null ? null : new CreateFruitDTO
                {
                    FruitFlavourId = p.Fruit.Flavourid,
                    FruitShapeId = p.Fruit.Shapeid,
                    FruitColorId = p.Fruit.Colorid,
                    FruitSurfaceId = p.Fruit.Surfaceid,
                    FruitThicknessId = p.Fruit.Thicknessid,
                    FruitMoreInfo = p.Fruit.moreInfo,
                    FruitMonthOfHarvestId = p.Fruit.MonthOfHarvestId
                }
            };
        }

        public async Task<bool> UpdatePlantAsync(int plantId, CreatePlantDTO dto)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            try
            {
                var plant = await PlantsWithParts().FirstOrDefaultAsync(p => p.Id == plantId);
                if (plant is null) return false;

                // roślina nie może być podobna sama do siebie
                dto.SimilarPlantIds?.RemoveAll(x => x == plantId);
                NormalizeSimilar(dto, selfUserInputId: null);
                await ValidateSimilarReferencesAsync(dto);
                await ValidateDictionariesAsync(dto);

                ApplyPlantValues(plant, dto);   // pola + części (dodanie / zmiana / usunięcie)
                await data.SaveChangesAsync();

                await SyncApprovedLinksAsync(plantId, dto.SimilarPlantIds);
                await SyncPendingLinksAsync(plantId, dto.SimilarUserInputIds);

                await data.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeletePlantAsync(int plantId)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            try
            {
                var plant = await PlantsWithParts().FirstOrDefaultAsync(p => p.Id == plantId);
                if (plant is null) return false;

                // powiązania z innymi roślinami (obie strony)
                await SyncApprovedLinksAsync(plantId, null);

                // zgłoszenia wskazujące tę roślinę
                await SyncPendingLinksAsync(plantId, null);

                // części należą tylko do tej rośliny
                if (plant.Stalk is not null) data.Stalks.Remove(plant.Stalk);
                if (plant.Sap is not null) data.Saps.Remove(plant.Sap);
                if (plant.Root is not null) data.Roots.Remove(plant.Root);
                if (plant.Hat is not null) data.Hats.Remove(plant.Hat);
                if (plant.Leaf is not null) data.Leaves.Remove(plant.Leaf);
                if (plant.Flower is not null) data.Flowers.Remove(plant.Flower);
                if (plant.Fruit is not null) data.Fruits.Remove(plant.Fruit);

                data.Plants.Remove(plant);

                await data.SaveChangesAsync();   // DbUpdateException, jeśli roślina ma zdjęcia lub produkty
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =====================================================================
        //  Wspólne: wartości rośliny i części
        // =====================================================================

        private IQueryable<Plant> PlantsWithParts() => data.Plants
            .Include(p => p.Stalk)
            .Include(p => p.Sap)
            .Include(p => p.Root)
            .Include(p => p.Hat)
            .Include(p => p.Leaf)
            .Include(p => p.Flower)
            .Include(p => p.Fruit);

        /// <summary>
        /// Przepisuje dane z DTO na roślinę. Dla każdej części:
        /// jest w DTO -> tworzy lub aktualizuje, nie ma w DTO -> usuwa istniejącą.
        /// </summary>
        private void ApplyPlantValues(Plant plant, CreatePlantDTO dto)
        {
            plant.Name = dto.Name.Trim();
            plant.Polishname = dto.PolishName.Trim();
            plant.Latinname = dto.LatinName;
            plant.Subriquet = dto.Subriquet;
            plant.moreInfo = dto.MoreInfo;
            plant.Planttypeid = dto.PlantTypeID;
            plant.Occuranceid = dto.OccuranceId;
            plant.Poisonabilityid = dto.PoisonabilityId;
            plant.MonthOfHarvestId = dto.MonthOfHarvestId;

            // ---------- łodyga / pień / trzon ----------
            if (dto.CreateStalkDTO is { } stalk)
            {
                plant.Stalk ??= new Stalk();
                plant.Stalk.Shapeid = stalk.StalkShapeId;
                plant.Stalk.Colorid = stalk.StalkColorId;
                plant.Stalk.Surfaceid = stalk.StalkSurfaceId;
                plant.Stalk.isHollow = stalk.StalkIsHollow;
                plant.Stalk.moreInfo = stalk.StalkMoreInfo;
                plant.Stalk.MonthOfHarvestId = stalk.StalkMonthOfHarvestId;
            }
            else if (plant.Stalk is not null)
            {
                data.Stalks.Remove(plant.Stalk);
                plant.Stalk = null;
                plant.Stalkid = null;
            }

            // ---------- sok ----------
            if (dto.CreateSapDTO is { } sap)
            {
                plant.Sap ??= new Sap();
                plant.Sap.Colorid = sap.SapColorId;
                plant.Sap.Leavesstains = sap.SapLeavesStain;
                plant.Sap.Sticky = sap.SapSticky;
                plant.Sap.moreInfo = sap.SapMoreInfo;
                plant.Sap.MonthOfHarvestId = sap.SapMonthOfHarvestId;
            }
            else if (plant.Sap is not null)
            {
                data.Saps.Remove(plant.Sap);
                plant.Sap = null;
                plant.Sapid = null;
            }

            // ---------- korzeń ----------
            if (dto.CreateRootDTO is { } root)
            {
                plant.Root ??= new Root();
                plant.Root.Colorid = root.RootColorId;
                plant.Root.Surfaceid = root.RootSurfaceId;
                plant.Root.Thicknessid = root.RootThicknessId;
                plant.Root.isHollow = root.RootIsHollow;
                plant.Root.moreInfo = root.RootMoreInfo;
                plant.Root.MonthOfHarvestId = root.RootMonthOfHarvestId;
            }
            else if (plant.Root is not null)
            {
                data.Roots.Remove(plant.Root);
                plant.Root = null;
                plant.Rootid = null;
            }

            // ---------- kapelusz ----------
            if (dto.CreateHatDTO is { } hat)
            {
                plant.Hat ??= new Hat();
                plant.Hat.Colorid = hat.HatColorId;
                plant.Hat.Shapeid = hat.HatShapeId;
                plant.Hat.Thicknessid = hat.HatThicknessId;
                plant.Hat.Surfaceid = hat.HatSurfaceId;
                plant.Hat.Hasspots = hat.HatHasSpots;
                plant.Hat.Hasgills = hat.HatHasGills;
                plant.Hat.HasRing = hat.HatHasRing;
                plant.Hat.moreInfo = hat.HatMoreInfo;
                plant.Hat.MonthOfHarvestId = hat.HatMonthOfHarvestId;
            }
            else if (plant.Hat is not null)
            {
                data.Hats.Remove(plant.Hat);
                plant.Hat = null;
                plant.Hatid = null;
            }

            // ---------- liście ----------
            if (dto.CreateLeafDTO is { } leaf)
            {
                plant.Leaf ??= new Leaf();
                plant.Leaf.Stripes = leaf.LeafStripes;
                plant.Leaf.Spots = leaf.LeafSpots;
                plant.Leaf.Holes = leaf.LeafHoles;
                plant.Leaf.Leafshapeid = leaf.LeafShapeId;
                plant.Leaf.Leafcolorid = leaf.LeafColorId;
                plant.Leaf.Surfaceid = leaf.LeafSurfaceId;
                plant.Leaf.Thicknessid = leaf.LeafThicknessId;
                plant.Leaf.Flavourid = leaf.LeafFlavourId;
                plant.Leaf.Leaflength = leaf.LeafLength;
                plant.Leaf.moreInfo = leaf.LeafMoreInfo;
                plant.Leaf.MonthOfHarvestId = leaf.LeafMonthOfHarvestId;
            }
            else if (plant.Leaf is not null)
            {
                data.Leaves.Remove(plant.Leaf);
                plant.Leaf = null;
                plant.Leafid = null;
            }

            // ---------- kwiaty ----------
            if (dto.CreateFlowerDTO is { } flower)
            {
                plant.Flower ??= new Flower();
                plant.Flower.Sizeincm = flower.FlowerSizeInCm;
                plant.Flower.Colorid = flower.FlowerColorId;
                plant.Flower.Shapeid = flower.FlowerShapeId;
                plant.Flower.Flavourid = flower.FlowerFlavourId;
                plant.Flower.Scentpower = flower.FlowerScentPower;
                plant.Flower.moreInfo = flower.FlowerMoreInfo;
                plant.Flower.MonthOfHarvestId = flower.FlowerMonthOfHarvestId;
            }
            else if (plant.Flower is not null)
            {
                data.Flowers.Remove(plant.Flower);
                plant.Flower = null;
                plant.Flowerid = null;
            }

            // ---------- owoce ----------
            if (dto.CreateFruitDTO is { } fruit)
            {
                plant.Fruit ??= new Fruit();
                plant.Fruit.Flavourid = fruit.FruitFlavourId;
                plant.Fruit.Shapeid = fruit.FruitShapeId;
                plant.Fruit.Colorid = fruit.FruitColorId;
                plant.Fruit.Surfaceid = fruit.FruitSurfaceId;
                plant.Fruit.Thicknessid = fruit.FruitThicknessId;
                plant.Fruit.moreInfo = fruit.FruitMoreInfo;
                plant.Fruit.MonthOfHarvestId = fruit.FruitMonthOfHarvestId;
            }
            else if (plant.Fruit is not null)
            {
                data.Fruits.Remove(plant.Fruit);
                plant.Fruit = null;
                plant.Fruitid = null;
            }
        }

        /// <summary>Sprawdza, czy wszystkie Id słownikowe z DTO istnieją w bazie.</summary>
        private async Task ValidateDictionariesAsync(CreatePlantDTO dto)
        {
            await EnsureExistsAsync(data.Planttypes, dto.PlantTypeID, "typ rośliny");
            await EnsureExistsAsync(data.Occurances, dto.OccuranceId, "występowanie");
            await EnsureExistsAsync(data.Poisonabilities, dto.PoisonabilityId, "trujące właściwości");
            await EnsureExistsAsync(data.MonthOfHarvests, dto.MonthOfHarvestId, "miesiąc zbioru");

            if (dto.CreateStalkDTO is { } stalk)
            {
                await EnsureExistsAsync(data.Shapes, stalk.StalkShapeId, "kształt (łodyga)");
                await EnsureExistsAsync(data.Colors, stalk.StalkColorId, "kolor (łodyga)");
                await EnsureExistsAsync(data.Surfaces, stalk.StalkSurfaceId, "powierzchnia (łodyga)");
                await EnsureExistsAsync(data.MonthOfHarvests, stalk.StalkMonthOfHarvestId, "miesiąc zbioru (łodyga)");
            }

            if (dto.CreateSapDTO is { } sap)
            {
                await EnsureExistsAsync(data.Colors, sap.SapColorId, "kolor (sok)");
                await EnsureExistsAsync(data.MonthOfHarvests, sap.SapMonthOfHarvestId, "miesiąc zbioru (sok)");
            }

            if (dto.CreateRootDTO is { } root)
            {
                await EnsureExistsAsync(data.Colors, root.RootColorId, "kolor (korzeń)");
                await EnsureExistsAsync(data.Surfaces, root.RootSurfaceId, "powierzchnia (korzeń)");
                await EnsureExistsAsync(data.Thicknesses, root.RootThicknessId, "grubość (korzeń)");
                await EnsureExistsAsync(data.MonthOfHarvests, root.RootMonthOfHarvestId, "miesiąc zbioru (korzeń)");
            }

            if (dto.CreateHatDTO is { } hat)
            {
                await EnsureExistsAsync(data.Colors, hat.HatColorId, "kolor (kapelusz)");
                await EnsureExistsAsync(data.Shapes, hat.HatShapeId, "kształt (kapelusz)");
                await EnsureExistsAsync(data.Thicknesses, hat.HatThicknessId, "grubość (kapelusz)");
                await EnsureExistsAsync(data.Surfaces, hat.HatSurfaceId, "powierzchnia (kapelusz)");
                await EnsureExistsAsync(data.MonthOfHarvests, hat.HatMonthOfHarvestId, "miesiąc zbioru (kapelusz)");
            }

            if (dto.CreateLeafDTO is { } leaf)
            {
                await EnsureExistsAsync(data.Shapes, leaf.LeafShapeId, "kształt (liść)");
                await EnsureExistsAsync(data.Colors, leaf.LeafColorId, "kolor (liść)");
                await EnsureExistsAsync(data.Surfaces, leaf.LeafSurfaceId, "powierzchnia (liść)");
                await EnsureExistsAsync(data.Thicknesses, leaf.LeafThicknessId, "grubość (liść)");
                await EnsureExistsAsync(data.Flavours, leaf.LeafFlavourId, "smak (liść)");
                await EnsureExistsAsync(data.MonthOfHarvests, leaf.LeafMonthOfHarvestId, "miesiąc zbioru (liść)");
            }

            if (dto.CreateFlowerDTO is { } flower)
            {
                await EnsureExistsAsync(data.Colors, flower.FlowerColorId, "kolor (kwiat)");
                await EnsureExistsAsync(data.Shapes, flower.FlowerShapeId, "kształt (kwiat)");
                await EnsureExistsAsync(data.Flavours, flower.FlowerFlavourId, "smak (kwiat)");
                await EnsureExistsAsync(data.MonthOfHarvests, flower.FlowerMonthOfHarvestId, "miesiąc zbioru (kwiat)");
            }

            if (dto.CreateFruitDTO is { } fruit)
            {
                await EnsureExistsAsync(data.Flavours, fruit.FruitFlavourId, "smak (owoc)");
                await EnsureExistsAsync(data.Shapes, fruit.FruitShapeId, "kształt (owoc)");
                await EnsureExistsAsync(data.Colors, fruit.FruitColorId, "kolor (owoc)");
                await EnsureExistsAsync(data.Surfaces, fruit.FruitSurfaceId, "powierzchnia (owoc)");
                await EnsureExistsAsync(data.Thicknesses, fruit.FruitThicknessId, "grubość (owoc)");
                await EnsureExistsAsync(data.MonthOfHarvests, fruit.FruitMonthOfHarvestId, "miesiąc zbioru (owoc)");
            }
        }

        // =====================================================================
        //  Wspólne: powiązania podobnych roślin
        // =====================================================================

        /// <summary>
        /// Ustawia powiązania rośliny z zatwierdzonymi roślinami dokładnie na podaną listę
        /// (w obie strony). null = usuń wszystkie.
        /// </summary>
        private async Task SyncApprovedLinksAsync(int plantId, IEnumerable<int>? similarIds)
        {
            var wanted = similarIds?.Where(x => x != plantId).ToHashSet() ?? new HashSet<int>();

            var existing = await data.PlantSimilarplants
                .Where(l => l.PlantId == plantId || l.SimilarPlantId == plantId)
                .ToListAsync();

            foreach (var link in existing)
            {
                var otherId = link.PlantId == plantId ? link.SimilarPlantId : link.PlantId;
                if (!wanted.Contains(otherId))
                    data.PlantSimilarplants.Remove(link);
            }

            foreach (var otherId in wanted)
            {
                if (!existing.Any(l => l.PlantId == plantId && l.SimilarPlantId == otherId))
                    data.PlantSimilarplants.Add(new PlantSimilarPlant { PlantId = plantId, SimilarPlantId = otherId });

                if (!existing.Any(l => l.PlantId == otherId && l.SimilarPlantId == plantId))
                    data.PlantSimilarplants.Add(new PlantSimilarPlant { PlantId = otherId, SimilarPlantId = plantId });
            }

            await data.SaveChangesAsync();
        }

        /// <summary>
        /// Ustawia, które zgłoszenia wskazują roślinę jako podobną (ich SimilarPlantIds).
        /// null = żadne zgłoszenie nie wskazuje tej rośliny.
        /// </summary>
        private async Task SyncPendingLinksAsync(int plantId, IEnumerable<int>? userInputIds)
        {
            var wanted = userInputIds?.ToHashSet() ?? new HashSet<int>();

            // zgłoszenia, które teraz wskazują roślinę, a nie powinny
            var referencing = await FindSubmissionsReferencingPlantAsync(plantId);

            foreach (var submission in referencing.Where(s => !wanted.Contains(s.Id)))
            {
                var dto = TryDeserialize(submission.Inputdata);
                if (dto?.SimilarPlantIds is null) continue;

                dto.SimilarPlantIds.RemoveAll(x => x == plantId);
                NormalizeSimilar(dto, selfUserInputId: submission.Id);
                submission.Inputdata = Serialize(dto);
            }

            // zgłoszenia, które powinny wskazywać roślinę, a jeszcze nie wskazują
            var toAdd = wanted.Except(referencing.Select(s => s.Id)).ToList();

            if (toAdd.Count > 0)
            {
                var targets = await data.UserInputs
                    .Where(u => toAdd.Contains(u.Id))
                    .ToListAsync();

                foreach (var target in targets)
                {
                    var dto = TryDeserialize(target.Inputdata);
                    if (dto is null) continue;

                    AddPlantLink(dto, plantId);
                    NormalizeSimilar(dto, selfUserInputId: target.Id);
                    target.Inputdata = Serialize(dto);
                }
            }

            await data.SaveChangesAsync();
        }

        // =====================================================================
        //  Podobne rośliny: wczytanie nazw do formularza
        // =====================================================================

        public async Task<List<SimilarPlantCandidateDTO>> ResolveSimilarAsync(
            IEnumerable<int>? plantIds, IEnumerable<int>? userInputIds)
        {
            var result = new List<SimilarPlantCandidateDTO>();

            var approvedIds = plantIds?.Distinct().ToList() ?? new List<int>();
            if (approvedIds.Count > 0)
            {
                result.AddRange(await data.Plants
                    .Where(p => approvedIds.Contains(p.Id))
                    .Select(p => new SimilarPlantCandidateDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        PolishName = p.Polishname,
                        LatinName = p.Latinname,
                        PlantTypeName = p.Planttype.Name,
                        IsApproved = true
                    })
                    .ToListAsync());
            }

            var pendingIds = userInputIds?.Distinct().ToList() ?? new List<int>();
            if (pendingIds.Count > 0)
            {
                var submissions = await data.UserInputs
                    .AsNoTracking()
                    .Where(u => pendingIds.Contains(u.Id))
                    .ToListAsync();

                var plantTypes = await data.Planttypes.ToDictionaryAsync(t => t.Id, t => t.Name);

                foreach (var submission in submissions)
                {
                    var dto = TryDeserialize(submission.Inputdata);
                    if (dto is null) continue;

                    result.Add(new SimilarPlantCandidateDTO
                    {
                        Id = submission.Id,
                        Name = dto.Name,
                        PolishName = dto.PolishName,
                        LatinName = dto.LatinName,
                        PlantTypeName = plantTypes.GetValueOrDefault(dto.PlantTypeID),
                        IsApproved = false
                    });
                }
            }

            return result;
        }

        // =====================================================================
        //  Pomocnicze
        // =====================================================================

        /// <summary>Zgłoszenia z danym Id w SimilarUserInputIds (śledzone – można je zmieniać).</summary>
        private Task<List<UserInput>> FindSubmissionsReferencingSubmissionAsync(int userInputId) =>
            data.UserInputs
                .FromSqlInterpolated($@"
                    SELECT * FROM userinput
                    WHERE inputdata->'SimilarUserInputIds' @> jsonb_build_array({userInputId})")
                .ToListAsync();

        /// <summary>Zgłoszenia z danym Id rośliny w SimilarPlantIds (śledzone – można je zmieniać).</summary>
        private Task<List<UserInput>> FindSubmissionsReferencingPlantAsync(int plantId) =>
            data.UserInputs
                .FromSqlInterpolated($@"
                    SELECT * FROM userinput
                    WHERE inputdata->'SimilarPlantIds' @> jsonb_build_array({plantId})")
                .ToListAsync();

        private static void AddPlantLink(CreatePlantDTO dto, int plantId)
        {
            dto.SimilarPlantIds ??= new List<int>();

            if (!dto.SimilarPlantIds.Contains(plantId))
                dto.SimilarPlantIds.Add(plantId);
        }

        /// <summary>Usuwa duplikaty, odwołanie do samego siebie i puste listy (-> null).</summary>
        private static void NormalizeSimilar(CreatePlantDTO dto, int? selfUserInputId)
        {
            dto.SimilarPlantIds = CleanIds(dto.SimilarPlantIds);
            dto.SimilarUserInputIds = CleanIds(dto.SimilarUserInputIds?.Where(x => x != selfUserInputId));
        }

        private static List<int>? CleanIds(IEnumerable<int>? ids)
        {
            var list = ids?.Distinct().ToList();
            return list is { Count: > 0 } ? list : null;
        }

        private async Task ValidateSimilarReferencesAsync(CreatePlantDTO dto)
        {
            if (dto.SimilarPlantIds is { Count: > 0 } plantIds &&
                await data.Plants.CountAsync(p => plantIds.Contains(p.Id)) != plantIds.Count)
            {
                throw new KeyNotFoundException("Część wskazanych podobnych roślin nie istnieje w bazie.");
            }

            if (dto.SimilarUserInputIds is { Count: > 0 } userInputIds &&
                await data.UserInputs.CountAsync(u => userInputIds.Contains(u.Id)) != userInputIds.Count)
            {
                throw new KeyNotFoundException("Część wskazanych podobnych zgłoszeń już nie istnieje.");
            }
        }

        private static async Task EnsureExistsAsync<T>(DbSet<T> set, int? id, string what) where T : class
        {
            if (id is null) return;

            if (await set.FindAsync(id.Value) is null)
                throw new KeyNotFoundException($"Nie istnieje {what} o Id {id}.");
        }

        private static List<string> DescribeParts(int plantTypeId,
            bool stalk, bool sap, bool root, bool hat, bool leaf, bool flower, bool fruit)
        {
            var parts = new List<string>();

            if (stalk) parts.Add(plantTypeId switch { 1 => "Pień", 3 => "Trzon", _ => "Łodyga" });
            if (sap) parts.Add("Sok");
            if (root) parts.Add("Korzeń");
            if (hat) parts.Add("Kapelusz");
            if (leaf) parts.Add("Liście");
            if (flower) parts.Add("Kwiaty");
            if (fruit) parts.Add("Owoce");

            return parts;
        }
    }
}


//using HerbRecognition_APIs.DTOs;
//using HerbRecognition_APIs.Models;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;

//namespace HerbRecognition_APIs.Services
//{
//    public interface ISubmissionService
//    {
//        Task<List<UserInputSummaryDTO>> GetSummariesAsync();
//        Task<CreatePlantDTO?> GetAsync(int id);
//        Task<bool> UpdateAsync(int id, CreatePlantDTO dto);
//        Task<bool> DeleteAsync(int id);
//        Task<int> ApproveAsync(int id, CreatePlantDTO dto);// Zgłoszenie -> roślina w bazie. Zwraca Id nowej rośliny
//        Task<int> CreatePlantAsync(CreatePlantDTO dto);// Bezpośrednie dodanie rośliny przez admina (bez zgłoszenia). Zwraca Id nowej rośliny
//        Task<List<SimilarPlantCandidateDTO>> ResolveSimilarAsync(IEnumerable<int>? plantIds, IEnumerable<int>? userInputIds);
//    }

//    public class SubmissionService(HerbRecognitionDbContext data) : ISubmissionService
//    {
//        private static string Serialize(CreatePlantDTO dto) => JsonSerializer.Serialize(dto);//  JSON zgłoszeń – ten sam format co w PostUserInputPlantAsync
//        private static CreatePlantDTO? TryDeserialize(string json)
//        {
//            try
//            {
//                return JsonSerializer.Deserialize<CreatePlantDTO>(json);
//            }
//            catch (JsonException)
//            {
//                return null;
//            }
//        }

//        // =====================================================================
//        //  Lista i odczyt zgłoszeń
//        // =====================================================================

//        public async Task<List<UserInputSummaryDTO>> GetSummariesAsync()
//        {
//            var submissions = await data.UserInputs
//                .AsNoTracking()
//                .OrderBy(u => u.Id)
//                .ToListAsync();

//            var plantTypes = await data.Planttypes.ToDictionaryAsync(t => t.Id, t => t.Name);
//            var result = new List<UserInputSummaryDTO>();

//            foreach (var submission in submissions)
//            {
//                var dto = TryDeserialize(submission.Inputdata);

//                if (dto is null)
//                {
//                    result.Add(new UserInputSummaryDTO
//                    {
//                        Id = submission.Id,
//                        PolishName = "Nieczytelne zgłoszenie",
//                        IsCorrupted = true
//                    });
//                    continue;
//                }

//                result.Add(new UserInputSummaryDTO
//                {
//                    Id = submission.Id,
//                    Name = dto.Name,
//                    PolishName = dto.PolishName,
//                    LatinName = dto.LatinName,
//                    PlantTypeName = plantTypes.GetValueOrDefault(dto.PlantTypeID),
//                    Parts = DescribeParts(dto),
//                    SimilarCount = (dto.SimilarPlantIds?.Count ?? 0) + (dto.SimilarUserInputIds?.Count ?? 0)
//                });
//            }

//            return result;
//        }
//        public async Task<CreatePlantDTO?> GetAsync(int id)
//        {
//            var submission = await data.UserInputs
//                .AsNoTracking()
//                .FirstOrDefaultAsync(u => u.Id == id);

//            return submission is null ? null : TryDeserialize(submission.Inputdata);
//        }

//        // =====================================================================
//        //  Edycja i usuwanie zgłoszeń
//        // =====================================================================
//        public async Task<bool> UpdateAsync(int id, CreatePlantDTO dto)
//        {
//            var submission = await data.UserInputs.FirstOrDefaultAsync(u => u.Id == id);
//            if (submission is null) return false;

//            NormalizeSimilar(dto, selfUserInputId: id);
//            await ValidateSimilarReferencesAsync(dto);

//            submission.Inputdata = Serialize(dto);
//            await data.SaveChangesAsync();
//            return true;
//        }
//        public async Task<bool> DeleteAsync(int id)
//        {
//            await using var transaction = await data.Database.BeginTransactionAsync();

//            var submission = await data.UserInputs.FirstOrDefaultAsync(u => u.Id == id);
//            if (submission is null) return false;

//            // inne zgłoszenia nie mogą wskazywać na usunięte
//            foreach (var other in await FindSubmissionsReferencingAsync(id))
//            {
//                var otherDto = TryDeserialize(other.Inputdata);
//                if (otherDto?.SimilarUserInputIds is null) continue;

//                otherDto.SimilarUserInputIds.RemoveAll(x => x == id);
//                NormalizeSimilar(otherDto, selfUserInputId: other.Id);
//                other.Inputdata = Serialize(otherDto);
//            }

//            data.UserInputs.Remove(submission);
//            await data.SaveChangesAsync();
//            await transaction.CommitAsync();
//            return true;
//        }

//        // =====================================================================
//        //  Zatwierdzanie i bezpośrednie dodawanie
//        // =====================================================================

//        public Task<int> ApproveAsync(int id, CreatePlantDTO dto) => CreateAndRelinkAsync(id, dto);
//        public Task<int> CreatePlantAsync(CreatePlantDTO dto) => CreateAndRelinkAsync(null, dto);
//        private async Task<int> CreateAndRelinkAsync(int? submissionId, CreatePlantDTO dto) //dla przypisania ponownie podobnych roslin z jeszcze nie zatwierdzonych
//        {
//            await using var transaction = await data.Database.BeginTransactionAsync();

//            try
//            {
//                UserInput? submission = null;

//                if (submissionId is int id)
//                {
//                    submission = await data.UserInputs.FirstOrDefaultAsync(u => u.Id == id)
//                        ?? throw new KeyNotFoundException($"Zgłoszenie #{id} nie istnieje.");
//                }

//                NormalizeSimilar(dto, selfUserInputId: submissionId);
//                await ValidateSimilarReferencesAsync(dto);

//                var plant = await AddPlantAsync(dto);

//                // zgłoszenia, które wskazywały na zatwierdzane zgłoszenie
//                if (submissionId is int approvedId)
//                {
//                    foreach (var other in await FindSubmissionsReferencingAsync(approvedId))
//                    {
//                        var otherDto = TryDeserialize(other.Inputdata);
//                        if (otherDto is null) continue;

//                        otherDto.SimilarUserInputIds?.RemoveAll(x => x == approvedId);
//                        AddPlantLink(otherDto, plant.Id);
//                        NormalizeSimilar(otherDto, selfUserInputId: other.Id);
//                        other.Inputdata = Serialize(otherDto);
//                    }
//                }

//                // zgłoszenia wskazane przez zatwierdzane
//                if (dto.SimilarUserInputIds is { Count: > 0 } pendingIds)
//                {
//                    var targets = await data.UserInputs
//                        .Where(u => pendingIds.Contains(u.Id))
//                        .ToListAsync();

//                    foreach (var target in targets)
//                    {
//                        var targetDto = TryDeserialize(target.Inputdata);
//                        if (targetDto is null) continue;

//                        AddPlantLink(targetDto, plant.Id);
//                        NormalizeSimilar(targetDto, selfUserInputId: target.Id);
//                        target.Inputdata = Serialize(targetDto);
//                    }
//                }

//                // zgłoszenie znika z kwarantanny
//                if (submission is not null)
//                    data.UserInputs.Remove(submission);

//                await data.SaveChangesAsync();
//                await transaction.CommitAsync();

//                return plant.Id;
//            }
//            catch
//            {
//                await transaction.RollbackAsync();
//                throw;
//            }
//        }

//        //Tworzy roślinę z częściami
//        private async Task<Plant> AddPlantAsync(CreatePlantDTO dto)
//        {
//            // ---------- słowniki rośliny ----------
//            await EnsureExistsAsync(data.Planttypes, dto.PlantTypeID, "typ rośliny");
//            await EnsureExistsAsync(data.Occurances, dto.OccuranceId, "występowanie");
//            await EnsureExistsAsync(data.Poisonabilities, dto.PoisonabilityId, "trujące właściwości");
//            await EnsureExistsAsync(data.MonthOfHarvests, dto.MonthOfHarvestId, "miesiąc zbioru");

//            var plant = new Plant
//            {
//                Name = dto.Name.Trim(),
//                Polishname = dto.PolishName.Trim(),
//                Latinname = dto.LatinName,
//                Subriquet = dto.Subriquet,
//                moreInfo = dto.MoreInfo,

//                Planttypeid = dto.PlantTypeID,
//                Occuranceid = dto.OccuranceId,
//                Poisonabilityid = dto.PoisonabilityId,
//                MonthOfHarvestId = dto.MonthOfHarvestId
//            };

//            // ---------- łodyga / pień / trzon ----------
//            if (dto.CreateStalkDTO is { } stalk)
//            {
//                await EnsureExistsAsync(data.Shapes, stalk.StalkShapeId, "kształt (łodyga)");
//                await EnsureExistsAsync(data.Colors, stalk.StalkColorId, "kolor (łodyga)");
//                await EnsureExistsAsync(data.Surfaces, stalk.StalkSurfaceId, "powierzchnia (łodyga)");
//                await EnsureExistsAsync(data.MonthOfHarvests, stalk.StalkMonthOfHarvestId, "miesiąc zbioru (łodyga)");

//                plant.Stalk = new Stalk
//                {
//                    Shapeid = stalk.StalkShapeId,
//                    Colorid = stalk.StalkColorId,
//                    Surfaceid = stalk.StalkSurfaceId,
//                    isHollow = stalk.StalkIsHollow,
//                    moreInfo = stalk.StalkMoreInfo,
//                    MonthOfHarvestId = stalk.StalkMonthOfHarvestId
//                };
//            }

//            // ---------- sok ----------
//            if (dto.CreateSapDTO is { } sap)
//            {
//                await EnsureExistsAsync(data.Colors, sap.SapColorId, "kolor (sok)");
//                await EnsureExistsAsync(data.MonthOfHarvests, sap.SapMonthOfHarvestId, "miesiąc zbioru (sok)");

//                plant.Sap = new Sap
//                {
//                    Colorid = sap.SapColorId,
//                    Leavesstains = sap.SapLeavesStain,
//                    Sticky = sap.SapSticky,
//                    moreInfo = sap.SapMoreInfo,
//                    MonthOfHarvestId = sap.SapMonthOfHarvestId
//                };
//            }

//            // ---------- korzeń ----------
//            if (dto.CreateRootDTO is { } root)
//            {
//                await EnsureExistsAsync(data.Colors, root.RootColorId, "kolor (korzeń)");
//                await EnsureExistsAsync(data.Surfaces, root.RootSurfaceId, "powierzchnia (korzeń)");
//                await EnsureExistsAsync(data.Thicknesses, root.RootThicknessId, "grubość (korzeń)");
//                await EnsureExistsAsync(data.MonthOfHarvests, root.RootMonthOfHarvestId, "miesiąc zbioru (korzeń)");

//                plant.Root = new Root
//                {
//                    Colorid = root.RootColorId,
//                    Surfaceid = root.RootSurfaceId,
//                    Thicknessid = root.RootThicknessId,
//                    isHollow = root.RootIsHollow,
//                    moreInfo = root.RootMoreInfo,
//                    MonthOfHarvestId = root.RootMonthOfHarvestId
//                };
//            }

//            // ---------- kapelusz ----------
//            if (dto.CreateHatDTO is { } hat)
//            {
//                await EnsureExistsAsync(data.Colors, hat.HatColorId, "kolor (kapelusz)");
//                await EnsureExistsAsync(data.Shapes, hat.HatShapeId, "kształt (kapelusz)");
//                await EnsureExistsAsync(data.Thicknesses, hat.HatThicknessId, "grubość (kapelusz)");
//                await EnsureExistsAsync(data.Surfaces, hat.HatSurfaceId, "powierzchnia (kapelusz)");
//                await EnsureExistsAsync(data.MonthOfHarvests, hat.HatMonthOfHarvestId, "miesiąc zbioru (kapelusz)");

//                plant.Hat = new Hat
//                {
//                    Colorid = hat.HatColorId,
//                    Shapeid = hat.HatShapeId,
//                    Thicknessid = hat.HatThicknessId,
//                    Surfaceid = hat.HatSurfaceId,
//                    Hasspots = hat.HatHasSpots,
//                    Hasgills = hat.HatHasGills,
//                    HasRing = hat.HatHasRing,
//                    moreInfo = hat.HatMoreInfo,
//                    MonthOfHarvestId = hat.HatMonthOfHarvestId
//                };
//            }

//            // ---------- liście ----------
//            if (dto.CreateLeafDTO is { } leaf)
//            {
//                await EnsureExistsAsync(data.Shapes, leaf.LeafShapeId, "kształt (liść)");
//                await EnsureExistsAsync(data.Colors, leaf.LeafColorId, "kolor (liść)");
//                await EnsureExistsAsync(data.Surfaces, leaf.LeafSurfaceId, "powierzchnia (liść)");
//                await EnsureExistsAsync(data.Thicknesses, leaf.LeafThicknessId, "grubość (liść)");
//                await EnsureExistsAsync(data.Flavours, leaf.LeafFlavourId, "smak (liść)");
//                await EnsureExistsAsync(data.MonthOfHarvests, leaf.LeafMonthOfHarvestId, "miesiąc zbioru (liść)");

//                plant.Leaf = new Leaf
//                {
//                    Stripes = leaf.LeafStripes,
//                    Spots = leaf.LeafSpots,
//                    Holes = leaf.LeafHoles,
//                    Leafshapeid = leaf.LeafShapeId,
//                    Leafcolorid = leaf.LeafColorId,
//                    Surfaceid = leaf.LeafSurfaceId,
//                    Thicknessid = leaf.LeafThicknessId,
//                    Flavourid = leaf.LeafFlavourId,
//                    Leaflength = leaf.LeafLength,
//                    moreInfo = leaf.LeafMoreInfo,
//                    MonthOfHarvestId = leaf.LeafMonthOfHarvestId
//                };
//            }

//            // ---------- kwiaty ----------
//            if (dto.CreateFlowerDTO is { } flower)
//            {
//                await EnsureExistsAsync(data.Colors, flower.FlowerColorId, "kolor (kwiat)");
//                await EnsureExistsAsync(data.Shapes, flower.FlowerShapeId, "kształt (kwiat)");
//                await EnsureExistsAsync(data.Flavours, flower.FlowerFlavourId, "smak (kwiat)");
//                await EnsureExistsAsync(data.MonthOfHarvests, flower.FlowerMonthOfHarvestId, "miesiąc zbioru (kwiat)");

//                plant.Flower = new Flower
//                {
//                    Sizeincm = flower.FlowerSizeInCm,
//                    Colorid = flower.FlowerColorId,
//                    Shapeid = flower.FlowerShapeId,
//                    Flavourid = flower.FlowerFlavourId,
//                    Scentpower = flower.FlowerScentPower,
//                    moreInfo = flower.FlowerMoreInfo,
//                    MonthOfHarvestId = flower.FlowerMonthOfHarvestId
//                };
//            }

//            // ---------- owoce ----------
//            if (dto.CreateFruitDTO is { } fruit)
//            {
//                await EnsureExistsAsync(data.Flavours, fruit.FruitFlavourId, "smak (owoc)");
//                await EnsureExistsAsync(data.Shapes, fruit.FruitShapeId, "kształt (owoc)");
//                await EnsureExistsAsync(data.Colors, fruit.FruitColorId, "kolor (owoc)");
//                await EnsureExistsAsync(data.Surfaces, fruit.FruitSurfaceId, "powierzchnia (owoc)");
//                await EnsureExistsAsync(data.Thicknesses, fruit.FruitThicknessId, "grubość (owoc)");
//                await EnsureExistsAsync(data.MonthOfHarvests, fruit.FruitMonthOfHarvestId, "miesiąc zbioru (owoc)");

//                plant.Fruit = new Fruit
//                {
//                    Flavourid = fruit.FruitFlavourId,
//                    Shapeid = fruit.FruitShapeId,
//                    Colorid = fruit.FruitColorId,
//                    Surfaceid = fruit.FruitSurfaceId,
//                    Thicknessid = fruit.FruitThicknessId,
//                    moreInfo = fruit.FruitMoreInfo,
//                    MonthOfHarvestId = fruit.FruitMonthOfHarvestId
//                };
//            }

//            // najpierw części, potem roślinę z Id czesci
//            data.Plants.Add(plant);
//            await data.SaveChangesAsync();

//            // ---------- podobne rośliny (zatwierdzone), w obie strony ----------
//            if (dto.SimilarPlantIds is { Count: > 0 } similarIds)
//            {
//                foreach (var similarId in similarIds)
//                {
//                    data.PlantSimilarplants.Add(new PlantSimilarPlant
//                    {
//                        PlantId = plant.Id,
//                        SimilarPlantId = similarId
//                    });

//                    data.PlantSimilarplants.Add(new PlantSimilarPlant
//                    {
//                        PlantId = similarId,
//                        SimilarPlantId = plant.Id
//                    });
//                }

//                await data.SaveChangesAsync();
//            }

//            return plant;
//        }

//        // =====================================================================
//        //  Podobne rośliny: wczytanie nazw do formularza
//        // =====================================================================

//        public async Task<List<SimilarPlantCandidateDTO>> ResolveSimilarAsync(
//            IEnumerable<int>? plantIds, IEnumerable<int>? userInputIds)
//        {
//            var result = new List<SimilarPlantCandidateDTO>();

//            var approvedIds = plantIds?.Distinct().ToList() ?? new List<int>();
//            if (approvedIds.Count > 0)
//            {
//                result.AddRange(await data.Plants
//                    .Where(p => approvedIds.Contains(p.Id))
//                    .Select(p => new SimilarPlantCandidateDTO
//                    {
//                        Id = p.Id,
//                        Name = p.Name,
//                        PolishName = p.Polishname,
//                        LatinName = p.Latinname,
//                        PlantTypeName = p.Planttype.Name,
//                        IsApproved = true
//                    })
//                    .ToListAsync());
//            }

//            var pendingIds = userInputIds?.Distinct().ToList() ?? new List<int>();
//            if (pendingIds.Count > 0)
//            {
//                var submissions = await data.UserInputs
//                    .AsNoTracking()
//                    .Where(u => pendingIds.Contains(u.Id))
//                    .ToListAsync();

//                var plantTypes = await data.Planttypes.ToDictionaryAsync(t => t.Id, t => t.Name);

//                foreach (var submission in submissions)
//                {
//                    var dto = TryDeserialize(submission.Inputdata);
//                    if (dto is null) continue;

//                    result.Add(new SimilarPlantCandidateDTO
//                    {
//                        Id = submission.Id,
//                        Name = dto.Name,
//                        PolishName = dto.PolishName,
//                        LatinName = dto.LatinName,
//                        PlantTypeName = plantTypes.GetValueOrDefault(dto.PlantTypeID),
//                        IsApproved = false
//                    });
//                }
//            }

//            return result;
//        }

//        // =====================================================================
//        //  Pomocnicze
//        // =====================================================================       
//        private Task<List<UserInput>> FindSubmissionsReferencingAsync(int userInputId) =>
//            data.UserInputs
//                .FromSqlInterpolated($@"
//                    SELECT * FROM userinput
//                    WHERE inputdata->'SimilarUserInputIds' @> jsonb_build_array({userInputId})")
//                .ToListAsync();//Zgłoszenia, które mają dane Id w SimilarUserInputIds
//        private static void AddPlantLink(CreatePlantDTO dto, int plantId)
//        {
//            dto.SimilarPlantIds ??= new List<int>();

//            if (!dto.SimilarPlantIds.Contains(plantId))
//                dto.SimilarPlantIds.Add(plantId);
//        }
//        private static void NormalizeSimilar(CreatePlantDTO dto, int? selfUserInputId)//Usuwa duplikaty, odwołanie do samego siebie i puste listy (-> null)
//        {
//            dto.SimilarPlantIds = CleanIds(dto.SimilarPlantIds);
//            dto.SimilarUserInputIds = CleanIds(dto.SimilarUserInputIds?.Where(x => x != selfUserInputId));
//        }
//        private static List<int>? CleanIds(IEnumerable<int>? ids)
//        {
//            var list = ids?.Distinct().ToList();
//            return list is { Count: > 0 } ? list : null;
//        }
//        private async Task ValidateSimilarReferencesAsync(CreatePlantDTO dto)
//        {
//            if (dto.SimilarPlantIds is { Count: > 0 } plantIds &&
//                await data.Plants.CountAsync(p => plantIds.Contains(p.Id)) != plantIds.Count)
//            {
//                throw new KeyNotFoundException("Część wskazanych podobnych roślin nie istnieje w bazie.");
//            }

//            if (dto.SimilarUserInputIds is { Count: > 0 } userInputIds &&
//                await data.UserInputs.CountAsync(u => userInputIds.Contains(u.Id)) != userInputIds.Count)
//            {
//                throw new KeyNotFoundException("Część wskazanych podobnych zgłoszeń już nie istnieje.");
//            }
//        }
//        private static async Task EnsureExistsAsync<T>(DbSet<T> set, int? id, string what) where T : class
//        {
//            if (id is null) return;

//            if (await set.FindAsync(id.Value) is null)
//                throw new KeyNotFoundException($"Nie istnieje {what} o Id {id}.");
//        }
//        private static List<string> DescribeParts(CreatePlantDTO dto)
//        {
//            var parts = new List<string>();

//            if (dto.CreateStalkDTO is not null)
//                parts.Add(dto.PlantTypeID switch { 1 => "Pień", 3 => "Trzon", _ => "Łodyga" });
//            if (dto.CreateSapDTO is not null) parts.Add("Sok");
//            if (dto.CreateRootDTO is not null) parts.Add("Korzeń");
//            if (dto.CreateHatDTO is not null) parts.Add("Kapelusz");
//            if (dto.CreateLeafDTO is not null) parts.Add("Liście");
//            if (dto.CreateFlowerDTO is not null) parts.Add("Kwiaty");
//            if (dto.CreateFruitDTO is not null) parts.Add("Owoce");

//            return parts;
//        }
//    }
//}
