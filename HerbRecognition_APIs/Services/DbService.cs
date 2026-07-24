using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Services
{
    public interface IDbService
    {
        Task<IEnumerable<GetPlantDTO>> GetAllPlantsAsync();
    }

    public class DbService(HerbRecognitionDbContext data) : IDbService
    {
        public async Task<IEnumerable<GetPlantDTO>> GetAllPlantsAsync()
        {
            var plants = await data.Plants
                .Select(p => new GetPlantDTO
                {
                    Name = p.Name,
                    PolishName = p.Polishname,
                    LatinName = p.Latinname,
                    Subriquet = p.Subriquet,

                    PlantTypeName = p.Planttype.Name,

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

                    Occurance = p.Occurance.OccuranceName,

                    GetHatDTO = p.Hat == null ? null : new GetHatDTO
                    {
                        HatColorName = p.Hat.Color.ColorName,
                        HatShapeName = p.Hat.Shape.ShapeName,
                        HatSurfaceName = p.Hat.Surface.SurfaceName,
                        HatThicknessName = p.Hat.Thickness.ThicknessName,
                        HatHasGills = p.Hat.Hasgills,
                        HatHasSpots = p.Hat.Hasspots,
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
                        p.SimilarPlants.Select(sp => new SimilarPlantDTO
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
    }
}
