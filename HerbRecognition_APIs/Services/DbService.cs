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
        Task<Plant> CreatePlantAsync(CreatePlantDTO dto);
        Task<IEnumerable<GetPlantDTO>> GetAllPlantsAsync();
        Task<GetPlantDTO?> GetPlantByIdAsync(int id);
        Task<IEnumerable<GetPlantDTO?>> GetPlantByNameAsync(string name);
        Task<IEnumerable<GetPlantDTO>> GetPlantsAsync();
        Task<CreatePlantDTO> GetUserInputPlantAsync(int id);
        Task<int> PostUserInputPlantAsync(CreatePlantDTO dto);
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
            GetStalkDTO = new GetStalkDTO
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
        public async Task<Plant> CreatePlantAsync(CreatePlantDTO dto)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            try
            {
                // ---- walidacja slownikow -------
                // -- plantType
                if (!await data.Planttypes.AnyAsync(x => x.Id == dto.PlantTypeID))
                    throw new KeyNotFoundException($"PlantType with ID {dto.PlantTypeID} does not exist!");

                // -- occurance
                if (!await data.Occurances.AnyAsync(x => x.Id == dto.OccuranceId))
                    throw new KeyNotFoundException($"Occurance with ID {dto.OccuranceId} does not exist!");

                // -- poisonability
                if (dto.PoisonabilityId.HasValue && !await data.Poisonabilities.AnyAsync(x => x.Id == dto.PoisonabilityId))
                    throw new KeyNotFoundException($"Poisonability with ID {dto.PoisonabilityId} does not exist!");

                await ValidateMonthOfHarvestAsync(dto.MonthOfHarvestId, "Plant"); 

                // --------------- tworzenie nowej rosliny -----------------
                var plant = new Plant
                {
                    Name = dto.Name,
                    Polishname = dto.PolishName,
                    Latinname = dto.LatinName,
                    Subriquet = dto.Subriquet,
                    moreInfo = dto.MoreInfo,

                    Planttypeid = dto.PlantTypeID,
                    Occuranceid = dto.OccuranceId,
                    Poisonabilityid = dto.PoisonabilityId,
                    MonthOfHarvestId = dto.MonthOfHarvestId
                };

                data.Plants.Add( plant );
                await data.SaveChangesAsync();

                // ----------- walidacja i tworzenie bocznych tabel -----------
                // -- similar plants --------
                if (dto.SimilarPlantIds is not null && dto.SimilarPlantIds.Count > 0)
                {
                    var similarPlantIds = dto.SimilarPlantIds
                        .Distinct()
                        .ToList();

                    var existingSimilarPlantsIds = await data .Plants
                        .Where(p => similarPlantIds.Contains(p.Id))
                        .Select(plant => plant.Id)
                        .ToListAsync();

                    var missingIds = similarPlantIds
                        .Distinct()
                        .Except(existingSimilarPlantsIds)
                        .ToList();

                    if(missingIds.Count > 0)
                    {
                        throw new KeyNotFoundException($"Similar plant with ID {string.Join(',', missingIds)} does not exist");
                    }

                    var similarPlants = await data.Plants
                        .Where(p => dto.SimilarPlantIds.Contains(p.Id))
                        .ToListAsync();

                    foreach(var simPlant in similarPlants)
                    {
                        data.PlantSimilarplants.Add(new PlantSimilarPlant
                        {
                            PlantId = plant.Id,
                            SimilarPlantId = simPlant.Id
                        });
                    }

                    await data.SaveChangesAsync();
                }

                // -- sap
                if (dto.CreateSapDTO is not null)
                {
                    await ValidateColorAsync(dto.CreateSapDTO.SapColorId, "SAP");
                    await ValidateMonthOfHarvestAsync(dto.CreateSapDTO.SapMonthOfHarvestId, "SAP");
                     plant.Sap = new Sap
                     {
                         Colorid = dto.CreateSapDTO.SapColorId,
                         Leavesstains = dto.CreateSapDTO.SapLeavesStain,
                         Sticky = dto.CreateSapDTO.SapSticky,
                         MonthOfHarvestId = dto.CreateSapDTO.SapMonthOfHarvestId,
                         moreInfo = dto.CreateSapDTO.SapMoreInfo
                     };
                }

                // -- root
                if (dto.CreateRootDTO is not null)
                {
                    await ValidateColorAsync(dto.CreateRootDTO.RootColorId, "ROOT");
                    await ValidateSurfaceAsync(dto.CreateRootDTO.RootSurfaceId, "ROOT");
                    await ValidateThicknessAsync(dto.CreateRootDTO.RootThicknessId, "ROOT");
                    await ValidateMonthOfHarvestAsync(dto.CreateRootDTO.RootMonthOfHarvestId, "ROOT");

                    plant.Root = new Root
                    {
                        Colorid = dto.CreateRootDTO.RootColorId,
                        Surfaceid = dto.CreateRootDTO.RootSurfaceId,
                        Thicknessid = dto.CreateRootDTO.RootThicknessId,
                        isHollow = dto.CreateRootDTO.RootIsHollow,
                        MonthOfHarvestId= dto.CreateRootDTO.RootMonthOfHarvestId,
                        moreInfo = dto.CreateRootDTO.RootMoreInfo
                    };
                }

                // -- stalk
                if (dto.CreateStalkDTO is not null)
                {
                    await ValidateShapeAsync(dto.CreateStalkDTO.StalkShapeId, "STALK");
                    await ValidateColorAsync(dto.CreateStalkDTO.StalkColorId, "STALK");
                    await ValidateSurfaceAsync(dto.CreateStalkDTO.StalkSurfaceId, "STALK");
                    await ValidateMonthOfHarvestAsync(dto.CreateStalkDTO.StalkMonthOfHarvestId, "STALK");

                    plant.Stalk = new Stalk
                    {
                        Shapeid = dto.CreateStalkDTO.StalkShapeId,
                        Colorid = dto.CreateStalkDTO.StalkColorId,
                        Surfaceid = dto.CreateStalkDTO.StalkSurfaceId,
                        isHollow = dto.CreateStalkDTO.StalkIsHollow,
                        MonthOfHarvestId = dto.CreateStalkDTO.StalkMonthOfHarvestId,
                        moreInfo = dto.CreateStalkDTO.StalkMoreInfo
                    };
                }
                

                // -- hat
                if (dto.CreateHatDTO is not null)
                {
                    await ValidateColorAsync(dto.CreateHatDTO.HatColorId, "HAT");
                    await ValidateShapeAsync(dto.CreateHatDTO.HatShapeId, "HAT");
                    await ValidateThicknessAsync(dto.CreateHatDTO.HatThicknessId, "HAT");
                    await ValidateSurfaceAsync(dto.CreateHatDTO.HatSurfaceId, "HAT");
                    await ValidateMonthOfHarvestAsync(dto.CreateHatDTO.HatMonthOfHarvestId, "HAT");

                    plant.Hat = new Hat
                    {
                        Colorid = dto.CreateHatDTO.HatColorId,
                        Shapeid = dto.CreateHatDTO.HatShapeId,
                        Thicknessid = dto.CreateHatDTO.HatThicknessId,
                        Surfaceid = dto.CreateHatDTO.HatSurfaceId,
                        Hasspots = dto.CreateHatDTO.HatHasSpots,
                        Hasgills = dto.CreateHatDTO.HatHasGills,
                        HasRing = dto.CreateHatDTO.HatHasRing,
                        MonthOfHarvestId = dto.CreateHatDTO.HatMonthOfHarvestId,
                        moreInfo = dto.CreateHatDTO.HatMoreInfo
                    };
                }

                // -- leaf
                if (dto.CreateLeafDTO is not null)
                {
                    if(dto.CreateLeafDTO.LeafFlavourId.HasValue)
                        await ValidateFlavourAsync(dto.CreateLeafDTO.LeafFlavourId, "LEAF");

                    await ValidateShapeAsync(dto.CreateLeafDTO.LeafShapeId, "LEAF");
                    await ValidateColorAsync(dto.CreateLeafDTO.LeafColorId, "LEAF");
                    await ValidateSurfaceAsync(dto.CreateLeafDTO.LeafSurfaceId, "LEAF");
                    await ValidateThicknessAsync(dto.CreateLeafDTO.LeafThicknessId, "LEAF");
                    await ValidateMonthOfHarvestAsync(dto.CreateLeafDTO.LeafMonthOfHarvestId, "LEAF");

                    plant.Leaf = new Leaf
                    {
                        Stripes = dto.CreateLeafDTO.LeafStripes,
                        Spots = dto.CreateLeafDTO.LeafSpots,
                        Holes = dto.CreateLeafDTO.LeafHoles,
                        Leafshapeid = dto.CreateLeafDTO.LeafShapeId,
                        Leafcolorid = dto.CreateLeafDTO.LeafColorId,
                        Surfaceid = dto.CreateLeafDTO.LeafSurfaceId,
                        Thicknessid = dto.CreateLeafDTO.LeafThicknessId,
                        Flavourid = dto.CreateLeafDTO.LeafFlavourId,
                        Leaflength = dto.CreateLeafDTO.LeafLength,
                        MonthOfHarvestId = dto.CreateLeafDTO.LeafMonthOfHarvestId,
                        moreInfo = dto.CreateLeafDTO.LeafMoreInfo
                    };
                }

                // -- flower
                if (dto.CreateFlowerDTO is not null)
                {
                    if (dto.CreateFlowerDTO.FlowerFlavourId.HasValue)
                        await ValidateFlavourAsync(dto.CreateFlowerDTO.FlowerFlavourId, "FLOWER");

                    await ValidateColorAsync(dto.CreateFlowerDTO.FlowerColorId, "FLOWER");
                    await ValidateShapeAsync(dto.CreateFlowerDTO.FlowerShapeId, "FLOWER");
                    await ValidateMonthOfHarvestAsync(dto.CreateFlowerDTO.FlowerMonthOfHarvestId, "FLOWER");

                    plant.Flower = new Flower
                    {
                        Sizeincm = dto.CreateFlowerDTO.FlowerSizeInCm,
                        Colorid = dto.CreateFlowerDTO.FlowerColorId,
                        Shapeid = dto.CreateFlowerDTO.FlowerShapeId,
                        Flavourid = dto.CreateFlowerDTO.FlowerFlavourId,
                        Scentpower = dto.CreateFlowerDTO.FlowerScentPower,
                        MonthOfHarvestId = dto.CreateFlowerDTO.FlowerMonthOfHarvestId,
                        moreInfo = dto.CreateFlowerDTO.FlowerMoreInfo
                    };

                }

                // -- fruit
                if (dto.CreateFruitDTO is not null)
                {
                    if (dto.CreateFruitDTO.FruitFlavourId.HasValue)
                        await ValidateFlavourAsync(dto.CreateFruitDTO.FruitFlavourId, "FRUIT");

                    await ValidateShapeAsync(dto.CreateFruitDTO.FruitShapeId, "FRUIT");
                    await ValidateColorAsync(dto.CreateFruitDTO.FruitColorId, "FRUIT");
                    await ValidateSurfaceAsync(dto.CreateFruitDTO.FruitSurfaceId, "FRUIT");
                    await ValidateThicknessAsync(dto.CreateFruitDTO.FruitThicknessId, "FRUIT");
                    await ValidateMonthOfHarvestAsync(dto.CreateFruitDTO.FruitMonthOfHarvestId, "FRUIT");

                    plant.Fruit = new Fruit
                    {
                        Flavourid = dto.CreateFruitDTO.FruitFlavourId,
                        Shapeid = dto.CreateFruitDTO.FruitShapeId,
                        Colorid = dto.CreateFruitDTO.FruitColorId,
                        Surfaceid = dto.CreateFruitDTO.FruitSurfaceId,
                        Thicknessid = dto.CreateFruitDTO.FruitThicknessId,
                        MonthOfHarvestId = dto.CreateFruitDTO.FruitMonthOfHarvestId,
                        moreInfo = dto.CreateFruitDTO.FruitMoreInfo
                    };
                }

                await data.SaveChangesAsync();

                await transaction.CommitAsync();
                return plant;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<GetPlantDTO?> GetPlantByIdAsync(int id)
        {
            return await data.Plants
                .Where(p => p.Id == id)
                .Select(plantsDetailsProjection)
                .FirstOrDefaultAsync();
        }
        public static bool IsSimilar(string? text, string search)
        {
            if(string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim().ToLowerInvariant();
            search = search.Trim().ToLowerInvariant();

            //dokladna nazwa
            if (text ==  search) return true; 

            //czesciowe
            if(text.Contains(search)) return true;

            //literowki
            var literowkaDistance = LevenshteinDistance(text, search);

            return literowkaDistance <= 2;//maksymalnie dwie różnice
        }
        private static int LevenshteinDistance(string a, string b)//todo poprawic //o ile liter roznia sie od siebie dwa wyrazy
        {//zalozenie ze poczatek jest w miare poprawny
            var matrix = new int[a.Length + 1, b.Length + 1];

            //porownanie roznicy pomiedzy dwoma wyrazami,
            //porownanie najpierw pierwszego do pustego, potem pierwszego do 1 chara z drugiego
            for (int i = 0; i <= a.Length; i++)
                matrix[i, 0] = i;

            //potem drugiego do pierwszego w ten sam sposob
            for (int j = 0; j <= b.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    //obliczenie jaki jest koszt przejscia z kazda na kazda kombinacje
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1; //sprawdzenie czy aktualne znaki sa takie same

                    matrix[i, j] = Math.Min( //wybranie najtanszej mozliwosci (dodanie znaku, usuniecie znaku albo zmiana znaku)
                        Math.Min(
                            matrix[i - 1, j] + 1,
                            matrix[i, j - 1] + 1
                        ),
                        matrix[i - 1, j - 1] + cost
                    );
                }
            }

            return matrix[a.Length, b.Length];
        }
        public async Task<IEnumerable<GetPlantDTO?>> GetPlantByNameAsync(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<GetPlantDTO?>();

            var query = data.Plants
                .Select(p => new GetPlantDTO
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

                    PoisonabilityDescription =
                        p.Poisonability == null
                            ? null
                            : p.Poisonability.Description
                });

            name = name.Trim();

            //najpierw wyszukanie dla contains
            var containsSearchPlants = await data.Plants
                .Where(p =>
                    p.Name.Contains(name) ||
                    p.Polishname.Contains(name) ||
                    p.Latinname.Contains(name) ||
                    p.Subriquet.Contains(name)
                )
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
                        PoisonabilityDescription = p.Poisonability == null ? null : p.Poisonability.Description
                    }
                )
               .OrderBy(p => p.PolishName)
               .ToListAsync();

            if (containsSearchPlants.Any())
                return containsSearchPlants;

            //awaryjne wyszukanie dla literowek
            var allPlantsCandidates = await data.Plants
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
                        PoisonabilityDescription = p.Poisonability == null ? null : p.Poisonability.Description
                    }
                )
               .ToListAsync();

            // fuzzy search
            var plants = allPlantsCandidates
                .Where(p =>
                    IsSimilar(p.Name, name) ||
                    IsSimilar(p.PolishName, name) ||
                    IsSimilar(p.LatinName, name) ||
                    IsSimilar(p.Subriquet, name))
                .OrderBy(p => p.PolishName)
                .ToList();


            return plants;
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
        public async Task<int> PostUserInputPlantAsync(CreatePlantDTO dto)
        {
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
