using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using System.Drawing;
using System.Xml;

namespace HerbRecognition_APIs.Services
{
    public interface IDbService
    {
        Task<Plant> CreatePlantAsync(CreatePlantDTO dto);
        Task<IEnumerable<GetPlantDTO>> GetAllPlantsAsync();
        Task<List<GetPlantTypeDTO>> GetAllPlantTypesAsync();
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

        public async Task<IEnumerable<GetPlantDTO>> GetAllPlantsAsync()
        {
            var plants = await data.Plants
                .Select(p => new GetPlantDTO
                {
                    Name = p.Name,
                    PolishName = p.Polishname,
                    LatinName = p.Latinname,
                    Subriquet = p.Subriquet,

                    PlantTypeDTO = new GetPlantTypeDTO
                    {
                        Name = p.Planttype.Name
                    },

                    GetSapDTO = p.Sap == null ? null : new GetSapDTO
                    {
                        SapColor = p.Sap.Color.ColorName,
                        SapLeavesStains = p.Sap.Leavesstains,
                        SapSticky = p.Sap.Sticky,
                    },
                    GetRootDTO = p.Root == null ? null : new GetRootDTO
                    {
                        RootColorName = p.Root.Color.ColorName,
                        RootSurfaceName = p.Root.Surface.SurfaceName,
                        RootThicknessName = p.Root.Thickness.ThicknessName
                    },
                    GetStalkDTO = new GetStalkDTO
                    {
                        StalkColorName = p.Stalk.Color.ColorName,
                        StalkShapeName = p.Stalk.Shape.ShapeName,
                        StalkSurfaceName = p.Stalk.Surface.SurfaceName
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
                        HatHasRing = p.Hat.HasRing
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
                        LeafFlavourName = p.Leaf.Flavour.FlavourName
                    },
                    GetFlowerDTO = p.Flower == null ? null : new GetFlowerDTO
                    {
                        FlowerSizeInCm = p.Flower.Sizeincm,
                        FlowerColorName = p.Flower.Color.ColorName,
                        FlowerShapeName = p.Flower.Shape.ShapeName,
                        FlowerFlavourName = p.Flower.Flavour.FlavourName,
                        FlowerScentPower = p.Flower.Scentpower
                    },
                    GetFruitDTO = p.Fruit == null ? null : new GetFruitDTO
                    {
                        FruitFlavourName = p.Fruit.Flavour.FlavourName,
                        FruitShapeName = p.Fruit.Shape.ShapeName,
                        FruitColorName = p.Fruit.Color.ColorName,
                        FruitSurfaceName = p.Fruit.Surface.SurfaceName,
                        FruitThicnkessName = p.Fruit.Thickness.ThicknessName
                    },

                    SimiliarPlantDTO = p.SimilarPlants == null ? null :
                        p.SimilarPlants.Select(sp => new GetSimilarPlantDTO
                        {
                            Id = sp.SimilarPlant.Id,
                            Name = sp.SimilarPlant.Name,
                            PolishName = sp.SimilarPlant.Polishname
                        }).ToList(),

                    PoisonabilityDescription = p.Poisonability.Description
                })
                .ToListAsync();

            return plants;
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

                // --------------- tworzenie nowej rosliny -----------------
                var plant = new Plant
                {
                    Name = dto.Name,
                    Polishname = dto.PolishName,
                    Latinname = dto.LatinName,
                    Subriquet = dto.Subriquet,

                    Planttypeid = dto.PlantTypeID,
                    Occuranceid = dto.OccuranceId,
                    Poisonabilityid = dto.PoisonabilityId
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
                     plant.Sap = new Sap
                     {
                         Colorid = dto.CreateSapDTO.SapColorId,
                         Leavesstains = dto.CreateSapDTO.SapLeavesStain,
                         Sticky = dto.CreateSapDTO.SapSticky
                     };
                }

                // -- root
                if (dto.CreateRootDTO is not null)
                {
                    await ValidateColorAsync(dto.CreateRootDTO.RootColorId, "ROOT");
                    await ValidateSurfaceAsync(dto.CreateRootDTO.RootSurfaceId, "ROOT");
                    await ValidateThicknessAsync(dto.CreateRootDTO.RootThicknessId, "ROOT");
                    
                    plant.Root = new Root
                    {
                        Colorid = dto.CreateRootDTO.RootColorId,
                        Surfaceid = dto.CreateRootDTO.RootSurfaceId,
                        Thicknessid = dto.CreateRootDTO.RootThicknessId
                    };
                }

                // -- stalk - obowiazkowe
                await ValidateShapeAsync(dto.CreateStalkDTO.StalkShapeId, "STALK");
                await ValidateColorAsync(dto.CreateStalkDTO.StalkColorId, "STALK");
                await ValidateSurfaceAsync(dto.CreateStalkDTO.StalkSurfaceId, "STALK");
                
                plant.Stalk = new Stalk
                {
                    Shapeid = dto.CreateStalkDTO.StalkShapeId,
                    Colorid = dto.CreateStalkDTO.StalkColorId,
                    Surfaceid = dto.CreateStalkDTO.StalkSurfaceId,
                    IsHollow = dto.CreateStalkDTO.StalkIsHollow
                };
                

                // -- hat
                if (dto.CreateHatDTO is not null)
                {
                    await ValidateColorAsync(dto.CreateHatDTO.HatColorId, "HAT");
                    await ValidateShapeAsync(dto.CreateHatDTO.HatShapeId, "HAT");
                    await ValidateThicknessAsync(dto.CreateHatDTO.HatThicknessId, "HAT");
                    await ValidateSurfaceAsync(dto.CreateHatDTO.HatSurfaceId, "HAT");
                    
                    plant.Hat = new Hat
                    {
                        Colorid = dto.CreateHatDTO.HatColorId,
                        Shapeid = dto.CreateHatDTO.HatShapeId,
                        Thicknessid = dto.CreateHatDTO.HatThicknessId,
                        Surfaceid = dto.CreateHatDTO.HatSurfaceId,
                        Hasspots = dto.CreateHatDTO.HatHasSpots,
                        Hasgills = dto.CreateHatDTO.HatHasGills,
                        HasRing = dto.CreateHatDTO.HatHasRing
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
                    
                    plant.Leaf = new Leaf
                    {
                        Stripes = dto.CreateLeafDTO.LeafStripes,
                        Spots = dto.CreateLeafDTO.LeafSpots,
                        Holes = dto.CreateLeafDTO.LeafHoles,
                        Leafshapeid = dto.CreateLeafDTO.LeafShapeId,
                        Leafcolorid = dto.CreateLeafDTO.LeafColorId,
                        Surfaceid = dto.CreateLeafDTO.LeafSurfaceId,
                    };
                }

                // -- flower
                if (dto.CreateFlowerDTO is not null)
                {
                    if (dto.CreateFlowerDTO.FlowerFlavourId.HasValue)
                        await ValidateFlavourAsync(dto.CreateFlowerDTO.FlowerFlavourId, "FLOWER");

                    await ValidateColorAsync(dto.CreateFlowerDTO.FlowerColorId, "FLOWER");
                    await ValidateShapeAsync(dto.CreateFlowerDTO.FlowerShapeId, "FLOWER");

                    plant.Flower = new Flower
                    {
                        Sizeincm = dto.CreateFlowerDTO.FlowerSizeInCm,
                        Colorid = dto.CreateFlowerDTO.FlowerColorId,
                        Shapeid = dto.CreateFlowerDTO.FlowerShapeId,
                        Flavourid = dto.CreateFlowerDTO.FlowerFlavourId,
                        Scentpower = dto.CreateFlowerDTO.FlowerScentPower
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
                    
                    plant.Fruit = new Fruit
                    {
                        Flavourid = dto.CreateFruitDTO.FruitFlavourId,
                        Shapeid = dto.CreateFruitDTO.FruitShapeId,
                        Colorid = dto.CreateFruitDTO.FruitColorId,
                        Surfaceid = dto.CreateFruitDTO.FruitSurfaceId,
                        Thicknessid = dto.CreateFruitDTO.FruitThicknessId
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

        // ----------- dictionaries Controller api/dictionaries/ ------
        public async Task<List<GetPlantTypeDTO>> GetAllPlantTypesAsync() // planttype
        {
            var planttypes = await data.Planttypes
                .Select(p => new GetPlantTypeDTO
                {
                    Name = p.Name
                }).ToListAsync();

            return planttypes;
        }
    }
}
