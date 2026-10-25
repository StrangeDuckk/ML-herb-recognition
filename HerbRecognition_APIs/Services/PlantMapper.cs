using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;

namespace HerbRecognition_APIs.Services
{
    public static class PlantMapper
    {
        /// <summary>
        /// Roślina z bazy (wczytana z częściami) w postaci CreatePlantDTO.
        /// Ten sam kształt co zgłoszenie – dzięki temu oba porównujemy tak samo.
        /// Nie uzupełnia list podobnych roślin.
        /// </summary>
        public static CreatePlantDTO ToCreateDto(Plant p) => new()
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
}
