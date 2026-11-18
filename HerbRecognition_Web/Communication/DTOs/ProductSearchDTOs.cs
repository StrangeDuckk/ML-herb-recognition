namespace HerbRecognition_Web.Communication.DTOs
{
    public class ProductSearchCriteriaDTO
    {
        public string? Name { get; set; }
        public string? RecipeText { get; set; }

        public int? ProductTypeId { get; set; }
        public List<int>? PlantIds { get; set; }
        public List<int>? PlantUserInputIds { get; set; }
        public List<int>? DiseaseIds { get; set; }
        public List<int>? HealthPropertyIds { get; set; }
        public List<int>? ActiveSubstanceIds { get; set; }
        public List<int>? AdministrationRouteIds { get; set; }
        public List<int>? ExcludeContraindicationIds { get; set; }

        public int MinMatchPercent { get; set; }
        public bool IncludePending { get; set; } = true;
    }

    public class ProductSearchResultDTO : ProductPreviewDTO
    {
        public int MatchPercent { get; set; }
        public int MatchedCount { get; set; }
        public int ComparedCount { get; set; }
    }

    public class ProductSearchResponseDTO
    {
        public int CriteriaCount { get; set; }
        public int MinMatchPercent { get; set; }
        public int CheckedCount { get; set; }
        public List<ProductSearchResultDTO> Results { get; set; } = new();
    }
}
