using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    /// <summary>
    /// Wyszukiwanie przepisów przez formularz. Każde pole jest opcjonalne:
    /// null / pusta lista (albo 0 dla Id) = "nie wiem" – takie pole nie jest porównywane.
    /// </summary>
    public class ProductSearchCriteriaDTO
    {
        // ----- tekst (waga 1) -----
        [StringLength(300)]
        public string? Name { get; set; }

        /// <summary>Słowa z przepisu, np. "miód", "napar".</summary>
        [StringLength(300)]
        public string? RecipeText { get; set; }

        // ----- słowniki (waga 3 za każdy wybrany wpis) -----
        public int? ProductTypeId { get; set; }
        public List<int>? PlantIds { get; set; }

        /// <summary>Rośliny z kwarantanny (Id zgłoszeń) – też można je wybrać w formularzu.</summary>
        public List<int>? PlantUserInputIds { get; set; }
        public List<int>? DiseaseIds { get; set; }
        public List<int>? HealthPropertyIds { get; set; }
        public List<int>? ActiveSubstanceIds { get; set; }

        /// <summary>Filtr, nie procent: przepisy z którymkolwiek z tych przeciwwskazań są pomijane.</summary>
        public List<int>? ExcludeContraindicationIds { get; set; }

        // ----- ustawienia wyszukiwania -----
        [Range(0, 100)]
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
        /// <summary>Liczba porównywanych cech + liczba wykluczonych przeciwwskazań.</summary>
        public int CriteriaCount { get; set; }
        public int MinMatchPercent { get; set; }
        public int CheckedCount { get; set; }
        public List<ProductSearchResultDTO> Results { get; set; } = new();
    }
}
