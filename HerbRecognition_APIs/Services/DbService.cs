using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using System.Drawing;
using System.Linq.Expressions;
using System.Text.Json;
using System.Xml;

namespace HerbRecognition_APIs.Services
{
    public interface IDbService
    {
        //Task<Plant> CreatePlantAsync(CreatePlantDTO dto);
        Task<IEnumerable<GetPlantDTO>> GetAllPlantsAsync();
        Task<GetPlantDTO?> GetPlantByIdAsync(int id);
        Task<IEnumerable<GetPlantDTO?>> GetPlantByNameAsync(string name);
        Task<IEnumerable<GetPlantDTO>> GetPlantsAsync();
        Task<CreatePlantDTO> GetUserInputPlantAsync(int id);
        Task<int> PostUserInputPlantAsync(CreatePlantDTO dto);
        Task<List<SimilarPlantCandidateDTO>> SearchSimilarCandidatesAsync(string query, int limit = 10);
        Task<List<SimilarPlantCandidateDTO>> GetPendingSimilarToPlantAsync(int plantId);
        Task<List<GetPlantTypeDTO>> GetAllDictPlantTypesAsync();
        Task<List<GetOccuranceDTO>> GetAllDictOccurancesAsync();
        Task<List<GetShapeDTO>> GetAllDictShapesAsync();
        Task<List<GetFlavourDTO>> GetAllDictFlavoursAsync();
        Task<List<GetPoisonabilityDTO>> GetAllDictPoisonabilitiesAsync();
        Task<List<GetSurfaceDTO>> GetAllDictSurfacesAsync();
        Task<List<GetThicknessDTO>> GetAllDictThicknessesAsync();
        Task<List<GetColorDTO>> GetAllColorAsync();
        Task<int> CreateDictionaryItemAsync(string dictionary, DictionaryItemDTO dto);
        Task<bool> UpdateDictionaryItemAsync(string dictionary, int id, DictionaryItemDTO dto);
        Task<bool> DeleteDictionaryItemAsync(string dictionary, int id);
        Task<List<GetMonthOfHarvestDTO>> GetAllDictMonthOfHarvestAsync();
    }

    public class DbService(HerbRecognitionDbContext data) : IDbService 
    {
        public async Task<bool> ValidateColorAsync(int colorId, string table)
        {
            if (!await data.Colors.AnyAsync(x => x.Id == colorId))
                throw new KeyNotFoundException($"Color with Id {colorId} does not exist, for {table}!");
            return true;
        }
        private async Task<bool> ValidateShapeAsync(int shapeId, string table)
        {
            if (!await data.Shapes.AnyAsync(x => x.Id == shapeId))
                throw new KeyNotFoundException($"Shape with Id {shapeId} does not exist, for {table}!");
            return true;
        }
        private async Task<bool> ValidateSurfaceAsync(int SurfaceId, string table)
        {
            if (!await data.Surfaces.AnyAsync(x => x.Id == SurfaceId))
                throw new KeyNotFoundException($"Surface with Id {SurfaceId} does not exist, for {table}!");
            return true;
        }
        private async Task<bool> ValidateThicknessAsync(int ThicknessId, string table)
        {
            if(!await data.Thicknesses.AnyAsync(x => x.Id == ThicknessId))
                throw new KeyNotFoundException($"Thickness with Id {ThicknessId} does not exist, for {table}!");
            return true;
        }
        private async Task<bool> ValidateFlavourAsync(int? FlavourId, string table)
        {
            if(!await data.Flavours.AnyAsync(x => x.Id == FlavourId))
                throw new KeyNotFoundException($"Flavour with Id {FlavourId} does not exist, for {table}!");
            return true;
        }
        public async Task<bool> ValidateMonthOfHarvestAsync(int? MonthId, string table)
        {
            if (MonthId is null) return true; ;// miesiac jest opcjonalny

            if (!await data.MonthOfHarvests.AnyAsync(x => x.Id == MonthId))
                throw new KeyNotFoundException($"Month with Id {MonthId} does not exist, for {table}!");
            return true;
        }

        // ----------- Plants Controller api/plants/ ------
        public static readonly Expression<Func<Plant, GetPlantDTO>> plantsDetailsProjection = p => new GetPlantDTO
        {
            Id = p.Id,
            Name = p.Name,
            PolishName = p.Polishname,
            LatinName = p.Latinname,
            Subriquet = p.Subriquet,
            MoreInfo = p.moreInfo,

            PlantTypeDTO = new GetPlantTypeDTO
            {
                PlantTypeId = p.Planttypeid,
                Name = p.Planttype.Name
            },

            GetSapDTO = p.Sap == null ? null : new GetSapDTO
            {
                SapColor = p.Sap.Color.ColorName,
                SapLeavesStains = p.Sap.Leavesstains,
                SapSticky = p.Sap.Sticky,
                SapMoreInfo = p.Sap.moreInfo,
                SapMonthOfHarvestName = p.Sap.MonthOfHarvest != null ? p.Sap.MonthOfHarvest.MonthName : null,
            },
            GetRootDTO = p.Root == null ? null : new GetRootDTO
            {
                RootColorName = p.Root.Color.ColorName,
                RootSurfaceName = p.Root.Surface.SurfaceName,
                RootThicknessName = p.Root.Thickness.ThicknessName,
                RootIsHollow = p.Root.isHollow,
                RootMoreInfo = p.Root.moreInfo,
                RootMonthOfHarvestName = p.Root.MonthOfHarvest != null ? p.Root.MonthOfHarvest.MonthName : null,
            },
            GetStalkDTO = p.Stalk == null ? null : new GetStalkDTO
            {
                StalkColorName = p.Stalk.Color.ColorName,
                StalkShapeName = p.Stalk.Shape.ShapeName,
                StalkSurfaceName = p.Stalk.Surface.SurfaceName,
                StalkIsHollow = p.Stalk.isHollow,
                StalkMoreInfo = p.Stalk.moreInfo,
                StalkMonthOfHarvestName = p.Stalk.MonthOfHarvest != null ? p.Stalk.MonthOfHarvest.MonthName : null,
            },

            Occurance = new GetOccuranceDTO
            {
                Occurance = p.Occurance.OccuranceName
            },

            GetHatDTO = p.Hat == null ? null : new GetHatDTO
            {
                HatColorName = p.Hat.Color.ColorName,
                HatShapeName = p.Hat.Shape.ShapeName,
                HatSurfaceName = p.Hat.Surface.SurfaceName,
                HatThicknessName = p.Hat.Thickness.ThicknessName,
                HatHasGills = p.Hat.Hasgills,
                HatHasSpots = p.Hat.Hasspots,
                HatHasRing = p.Hat.HasRing,
                HatMoreInfo = p.Hat.moreInfo,
                HatMonthOfHarvestName = p.Hat.MonthOfHarvest != null ? p.Hat.MonthOfHarvest.MonthName : null,
            },
            GetLeafDTO = p.Leaf == null ? null : new GetLeafDTO
            {
                LeafHasStripes = p.Leaf.Stripes,
                LeafHasSpots = p.Leaf.Spots,
                LeafHasHoles = p.Leaf.Holes,
                LeafShapeName = p.Leaf.Leafshape.ShapeName,
                LeafColorName = p.Leaf.Leafcolor.ColorName,
                LeafSurfaceName = p.Leaf.Surface.SurfaceName,
                LeafLength = p.Leaf.Leaflength,
                LeafThicknessName = p.Leaf.Thickness.ThicknessName,
                LeafFlavourName = p.Leaf.Flavour.FlavourName,
                LeafMoreInfo = p.Leaf.moreInfo,
                LeafMonthOfHarvestName = p.Leaf.MonthOfHarvest != null ? p.Leaf.MonthOfHarvest.MonthName : null,
            },
            GetFlowerDTO = p.Flower == null ? null : new GetFlowerDTO
            {
                FlowerSizeInCm = p.Flower.Sizeincm,
                FlowerColorName = p.Flower.Color.ColorName,
                FlowerShapeName = p.Flower.Shape.ShapeName,
                FlowerFlavourName = p.Flower.Flavour.FlavourName,
                FlowerScentPower = p.Flower.Scentpower,
                FlowerMoreInfo = p.Flower.moreInfo,
                FlowerMonthOfHarvestName = p.Flower.MonthOfHarvest != null ? p.Flower.MonthOfHarvest.MonthName : null,
            },
            GetFruitDTO = p.Fruit == null ? null : new GetFruitDTO
            {
                FruitFlavourName = p.Fruit.Flavour.FlavourName,
                FruitShapeName = p.Fruit.Shape.ShapeName,
                FruitColorName = p.Fruit.Color.ColorName,
                FruitSurfaceName = p.Fruit.Surface.SurfaceName,
                FruitThicnkessName = p.Fruit.Thickness.ThicknessName,
                FruitMoreInfo = p.Fruit.moreInfo,
                FruitMonthOfHarvestName= p.Fruit.MonthOfHarvest != null ? p.Fruit.MonthOfHarvest.MonthName : null,
            },

            SimiliarPlantDTO = p.SimilarPlants.Select(sp => new GetSimilarPlantDTO
            {
                Id = sp.SimilarPlant.Id,
                Name = sp.SimilarPlant.Name,
                PolishName = sp.SimilarPlant.Polishname
            }).ToList(),

            PoisonabilityDescription = p.Poisonability == null ? null : p.Poisonability.Description,
            MonthOfHarvestName = p.MonthOfHarvest != null ? p.MonthOfHarvest.MonthName : null
        };
        private static readonly Expression<Func<Plant, GetPlantDTO>> plantPreviewProjection = p => new GetPlantDTO
        {
            Id = p.Id,
            Name = p.Name,
            PolishName = p.Polishname,
            LatinName = p.Latinname,
            Subriquet = p.Subriquet,
            PlantTypeDTO = new GetPlantTypeDTO { Name = p.Planttype.Name },
            PoisonabilityDescription = p.Poisonability == null ? null : p.Poisonability.Description,
            MonthOfHarvestName = p.MonthOfHarvest != null ? p.MonthOfHarvest.MonthName : null
        };
        public async Task<IEnumerable<GetPlantDTO>> GetAllPlantsAsync()
        {
             return await data.Plants
                .Select(plantsDetailsProjection)
                .ToListAsync();
        }
        public async Task<IEnumerable<GetPlantDTO>> GetPlantsAsync() // tylko do wyswietlenia preview w bibliotece
        {
            var plants_preview = await data.Plants
               .Select(
                    p => new GetPlantDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        PolishName = p.Polishname,
                        LatinName = p.Latinname,
                        Subriquet = p.Subriquet,
                        
                        PlantTypeDTO = new GetPlantTypeDTO
                        {
                            Name = p.Planttype.Name
                        },
                        PoisonabilityDescription = p.Poisonability == null? null :p.Poisonability.Description,
                        MonthOfHarvestName = p.MonthOfHarvest != null ? p.MonthOfHarvest.MonthName : null,
                    }
                )
               .OrderBy(p => p.PolishName)
               .ToListAsync();

            return plants_preview;
        }
        public async Task<GetPlantDTO?> GetPlantByIdAsync(int id)
        {
            return await data.Plants//todo blad jesli zaakceptowana roslina bez lisci itp
                .Where(p => p.Id == id)
                .Select(plantsDetailsProjection)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<GetPlantDTO?>> GetPlantByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<GetPlantDTO?>();
            
            //name = name.Trim();
            name = LikePattern.Limit(name);
            //var pattern = $"%{name}%";
            var pattern = LikePattern.Contains(name);

            // 1. szybkie wyszukiwanie w bazie – bez rozróżniania wielkości liter
            var found = await data.Plants
                .Where(p =>
                    EF.Functions.ILike(p.Name, pattern, LikePattern.EscapeCharacter) ||
                    EF.Functions.ILike(p.Polishname, pattern, LikePattern.EscapeCharacter) ||
                    (p.Latinname != null && EF.Functions.ILike(p.Latinname, pattern, LikePattern.EscapeCharacter)) ||
                    (p.Subriquet != null && EF.Functions.ILike(p.Subriquet, pattern, LikePattern.EscapeCharacter)))
                .Select(plantPreviewProjection)
                .OrderBy(p => p.PolishName)
                .ToListAsync();

            if (found.Count > 0)
                return found;

            // 2. literówki i polskie znaki – porównanie słowo do słowa
            var all = await data.Plants.Select(plantPreviewProjection).ToListAsync();

            return all
                .Where(p =>
                    FuzzyText.IsSimilar(p.Name, name) ||
                    FuzzyText.IsSimilar(p.PolishName, name) ||
                    FuzzyText.IsSimilar(p.LatinName, name) ||
                    FuzzyText.IsSimilar(p.Subriquet, name))
                .OrderBy(p => p.PolishName)
                .ToList();
        }

        // ----------- dictionaries Controller api/dictionaries/ ------
        public async Task<List<GetPlantTypeDTO>> GetAllDictPlantTypesAsync() // planttype
        {
            var planttypes = await data.Planttypes
                .Select(p => new GetPlantTypeDTO
                {
                    PlantTypeId = p.Id,
                    Name = p.Name
                }).ToListAsync();

            return planttypes;
        }
        public async Task<List<GetOccuranceDTO>> GetAllDictOccurancesAsync()// occurance
        {
            return await data.Occurances
                .Select(p => new GetOccuranceDTO
                {
                    OccuranceId = p.Id,
                    Occurance = p.OccuranceName
                }).ToListAsync();
        }
        public async Task<List<GetShapeDTO>> GetAllDictShapesAsync() //shape
        {
            return await data.Shapes
                .Select(p => new GetShapeDTO
                {
                    ShapeId = p.Id,
                    ShapeName = p.ShapeName
                }).ToListAsync() ;  
        }
        public async Task<List<GetFlavourDTO>> GetAllDictFlavoursAsync() //flavour
        {
            return await data.Flavours
                .Select(p => new GetFlavourDTO
                {
                    FlavourId = p.Id,
                    FlavourName = p.FlavourName
                }).ToListAsync() ;
        }
        public async Task<List<GetPoisonabilityDTO>> GetAllDictPoisonabilitiesAsync()//poisonability
        {
            return await data.Poisonabilities
                .Select(p => new GetPoisonabilityDTO
                {
                    PoisonabilityId = p.Id,
                    PoisonabilityDescription = p.Description
                }).ToListAsync () ;
        }
        public async Task<List<GetSurfaceDTO>> GetAllDictSurfacesAsync()//surface
        {
            return await data.Surfaces
                .Select(p=> new GetSurfaceDTO
                {
                    SurfaceId = p.Id,
                    SurfaceName = p.SurfaceName
                }).ToListAsync ();
        }
        public async Task<List<GetThicknessDTO>> GetAllDictThicknessesAsync()//thickness
        {
            return await data.Thicknesses
                .Select(p => new GetThicknessDTO
                {
                    ThicknessId = p.Id,
                    ThicknessName = p.ThicknessName
                }).ToListAsync();
        }
        public async Task<List<GetColorDTO>> GetAllColorAsync()//color
        {
            return await data.Colors
                .Select(p => new GetColorDTO
                {
                    ColorId = p.Id,
                    ColorName = p.ColorName,
                    ColorR = p.R,
                    ColorB = p.B,
                    ColorG = p.G
                }).ToListAsync();
        }
        public async Task<List<GetMonthOfHarvestDTO>> GetAllDictMonthOfHarvestAsync()//monthOfHarvest
        {
            return await data.MonthOfHarvests
                .Select(p => new GetMonthOfHarvestDTO
                {
                    MonthOfHarvestId = p.Id,
                    MonthName = p.MonthName
                }).ToListAsync() ;
        }

        // ----------- UserInput Controller api/userInput/ ------
        private async Task<List<SimilarPlantCandidateDTO>> MapSubmissionsToCandidatesAsync(IEnumerable<UserInput> submissions)
        {
            var plantTypes = await data.Planttypes.ToDictionaryAsync(t => t.Id, t => t.Name);
            var result = new List<SimilarPlantCandidateDTO>();

            foreach (var submission in submissions)
            {
                CreatePlantDTO? dto;
                try
                {
                    dto = JsonSerializer.Deserialize<CreatePlantDTO>(submission.Inputdata);
                }
                catch (JsonException ex)
                {
                    Console.Error.WriteLine(
                        $"Pominięto zgłoszenie {submission.Id} – nieprawidłowy JSON: {ex.Message}");
                    continue;
                }

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

            return result;
        }
        public async Task<List<SimilarPlantCandidateDTO>> GetPendingSimilarToPlantAsync(int plantId)
        {
            var submissions = await data.UserInputs
                .FromSqlInterpolated($@"
            SELECT * FROM userinput
            WHERE inputdata->'SimilarPlantIds' @> jsonb_build_array({plantId})
            ORDER BY id DESC")
                .AsNoTracking()
                .ToListAsync();

            return await MapSubmissionsToCandidatesAsync(submissions);
        }
        public async Task<List<SimilarPlantCandidateDTO>> SearchSimilarCandidatesAsync(string query, int limit = 10)
        {
            query = query.Trim();
            if (query.Length < 2)
                return new List<SimilarPlantCandidateDTO>();

            var pattern = LikePattern.Contains(query);

            // --- zatwierdzone (plant) --- wyszukiwanie przez linQ
            var results = await data.Plants
                .Where(p =>
                    EF.Functions.ILike(p.Name, pattern, LikePattern.EscapeCharacter) ||
                    EF.Functions.ILike(p.Polishname, pattern, LikePattern.EscapeCharacter) ||
                    (p.Latinname != null && EF.Functions.ILike(p.Latinname, pattern, LikePattern.EscapeCharacter)) ||
                    (p.Subriquet != null && EF.Functions.ILike(p.Subriquet, pattern, LikePattern.EscapeCharacter)))
                .OrderBy(p => p.Polishname)
                .Take(limit)
                .Select(p => new SimilarPlantCandidateDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    PolishName = p.Polishname,
                    LatinName = p.Latinname,
                    PlantTypeName = p.Planttype.Name,
                    IsApproved = true
                })
                .ToListAsync();

            // --- czekające na weryfikację (userinput, pola wewnątrz jsonb) --- wyszukanie przez sql, bo trzymane w JSONie
            var pending = await data.UserInputs
                .FromSqlInterpolated($@"
            SELECT * FROM userinput
            WHERE inputdata->>'Name'       ILIKE {pattern} ESCAPE '\'
               OR inputdata->>'PolishName' ILIKE {pattern} ESCAPE '\'
               OR inputdata->>'LatinName'  ILIKE {pattern} ESCAPE '\'
               OR inputdata->>'Subriquet'  ILIKE {pattern} ESCAPE '\'
            ORDER BY id DESC
            LIMIT {limit}")//pattern i limit przeciw sql injection
                .AsNoTracking()
                .ToListAsync();

            results.AddRange(await MapSubmissionsToCandidatesAsync(pending));
            return results;

        }
        public async Task<int> PostUserInputPlantAsync(CreatePlantDTO dto)
        {
            // -----  walidacja dla similar plants ------

            if (dto.SimilarPlantIds is { Count: > 0 })
            {
                var ids = dto.SimilarPlantIds.Distinct().ToList();
                if (await data.Plants.CountAsync(p => ids.Contains(p.Id)) != ids.Count)
                    throw new KeyNotFoundException("Część wskazanych podobnych roślin nie istnieje.");
            }

            if (dto.SimilarUserInputIds is { Count: > 0 })
            {
                var ids = dto.SimilarUserInputIds.Distinct().ToList();
                if (await data.UserInputs.CountAsync(u => ids.Contains(u.Id)) != ids.Count)
                    throw new KeyNotFoundException("Część wskazanych zgłoszeń nie istnieje.");
            }

            // ------------------------------------------

            var json = JsonSerializer.Serialize(dto);

            var submission = new UserInput
            {
                Inputdata = json
            };

            data.UserInputs.Add(submission);
            await data.SaveChangesAsync();

            return submission.Id;
        }
        public async Task<CreatePlantDTO?> GetUserInputPlantAsync(int id)
        {
            var subbmission = await data.UserInputs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subbmission is null)
            {
                return null;
            }

            var dto = JsonSerializer.Deserialize<CreatePlantDTO>(subbmission.Inputdata);

            return dto;
        }

        // ----------- Admin Controller api/admin --------------
        private static readonly Dictionary<string, (Type Type, string NameProperty)> DictionaryMap =new(StringComparer.OrdinalIgnoreCase)
                {
                    ["occurance"] = (typeof(Occurance), nameof(Occurance.OccuranceName)),
                    ["shape"] = (typeof(Shape), nameof(Shape.ShapeName)),
                    ["flavour"] = (typeof(Flavour), nameof(Flavour.FlavourName)),
                    ["poisonability"] = (typeof(Poisonability), nameof(Poisonability.Description)),
                    ["surface"] = (typeof(Surface), nameof(Surface.SurfaceName)),
                    ["thickness"] = (typeof(Thickness), nameof(Thickness.ThicknessName)),
                    ["color"] = (typeof(Models.Color), nameof(Models.Color.ColorName)),
                    ["monthOfHarvest"] = (typeof(MonthOfHarvest), nameof(MonthOfHarvest.MonthName))
                };
        private static readonly HashSet<string> EditOnlyDictionaries = new(StringComparer.OrdinalIgnoreCase)//stała liczba, edycja istniejacych
                {
                    "monthOfHarvest"
                };
        private static (Type Type, string NameProperty) GetDictionary(string dictionary) =>
            DictionaryMap.TryGetValue(dictionary, out var d)
              ? d
              : throw new ArgumentException($"Nieznany słownik: {dictionary}");
        public async Task<int> CreateDictionaryItemAsync(string dictionary, DictionaryItemDTO dto)
        {
            if (EditOnlyDictionaries.Contains(dictionary))
                throw new ArgumentException($"Do tego słownika {dictionary} nie można dodawać wpisów - można tylko edytować obecne");
            var (type, nameProp) = GetDictionary(dictionary);

            var entity = Activator.CreateInstance(type)!;
            data.Add(entity);
            var entry = data.Entry(entity);
            ApplyValues(entry, nameProp, dto);

            await data.SaveChangesAsync();

            var keyName = entry.Metadata.FindPrimaryKey()!.Properties[0].Name;
            return Convert.ToInt32(entry.Property(keyName).CurrentValue);

        }
        public async Task<bool> UpdateDictionaryItemAsync(string dictionary, int id, DictionaryItemDTO dto)
        {
            var (type, nameProp) = GetDictionary(dictionary);

            var entity = await data.FindAsync(type, id);
            if (entity is null) return false;

            ApplyValues(data.Entry(entity), nameProp, dto);
            await data.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteDictionaryItemAsync(string dictionary, int id)
        {
            if (EditOnlyDictionaries.Contains(dictionary))
                throw new ArgumentException($"Do tego słownika {dictionary} nie można dodawać wpisów - można tylko edytować obecne");

            var (type, _) = GetDictionary(dictionary);

            var entity = await data.FindAsync(type, id);
            if (entity is null) return false;

            data.Remove(entity);
            await data.SaveChangesAsync(); // DbUpdateException, jeśli wpis jest używany, todo w przyszlosci flaga czy aktywny
            return true;
        }
        private static void ApplyValues(EntityEntry entry, string nameProp, DictionaryItemDTO dto)
        {
            entry.Property(nameProp).CurrentValue = dto.Name.Trim();

            if (entry.Metadata.ClrType == typeof(Models.Color))
            {
                // nazwy kolumn RGB dopasuj do swojej encji Color
                SetNumber(entry, "R", dto.R);
                SetNumber(entry, "G", dto.G);
                SetNumber(entry, "B", dto.B);
            }

        }
        private static void SetNumber(EntityEntry entry, string propertyName, int? value)
        {
            var property = entry.Property(propertyName);
            var targetType = Nullable.GetUnderlyingType(property.Metadata.ClrType)
                             ?? property.Metadata.ClrType;
            property.CurrentValue = Convert.ChangeType(value ?? 0, targetType);
        }
    }
}
