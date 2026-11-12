using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    // =====================================================================
    //  Zgłoszenie / edycja produktu
    //  Ten sam DTO trafia jako JSON do kwarantanny (productinput) – jak CreatePlantDTO.
    // =====================================================================

    public class CreateProductDTO
    {
        [Required, StringLength(300, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        [Required, StringLength(10000, MinimumLength = 10)]
        public string Recipe { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int ProductTypeId { get; set; }

        // ----- rośliny: zatwierdzone (plant) i z kwarantanny (userinput) -----
        public List<int>? PlantIds { get; set; }
        public List<int>? PlantUserInputIds { get; set; }

        // ----- wpisy wybrane ze słowników -----
        public List<int>? HealthPropertyIds { get; set; }
        public List<int>? ActiveSubstanceIds { get; set; }
        public List<int>? DiseaseIds { get; set; }
        public List<int>? ContraindicationIds { get; set; }

        // ----- propozycje nowych wpisów (tylko w zgłoszeniu) -----
        // Admin przed zatwierdzeniem zamienia każdą na Id: wybiera istniejący wpis
        // albo dodaje nowy do słownika. Wpis, który już istnieje w słowniku
        // (bez względu na wielkość liter), API zamienia na Id automatycznie.
        public List<string>? NewHealthProperties { get; set; }
        public List<string>? NewActiveSubstances { get; set; }
        public List<NewDiseaseDTO>? NewDiseases { get; set; }
        public List<string>? NewContraindications { get; set; }
    }

    public class NewDiseaseDTO
    {
        [Required, StringLength(300, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [StringLength(4000)]
        public string? Symptoms { get; set; }
    }

    // =====================================================================
    //  Odczyt
    // =====================================================================

    public class NamedRefDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class DiseaseRefDTO : NamedRefDTO
    {
        public string Symptoms { get; set; } = "";
    }

    /// <summary>Roślina w produkcie – zatwierdzona (Id rośliny) albo z kwarantanny (Id zgłoszenia).</summary>
    public class PlantRefDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsApproved { get; set; }
    }

    /// <summary>Kafelek biblioteki i wiersz listy w panelu admina.</summary>
    public class ProductPreviewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ProductTypeName { get; set; }
        public List<PlantRefDTO> Plants { get; set; } = new();
        public List<string> Diseases { get; set; } = new();
        public List<string> HealthProperties { get; set; } = new();
        public string? MainThumbnailUrl { get; set; }

        // ----- tylko panel admina / kwarantanna -----
        public int PictureCount { get; set; }
        public int NewEntriesCount { get; set; }   // propozycje nowych wpisów do rozstrzygnięcia
        public bool IsPending { get; set; }
        public bool IsCorrupted { get; set; }
    }

    /// <summary>Strona szczegółów produktu (zdjęcia: api/products/{id}/pictures).</summary>
    public class GetProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Recipe { get; set; } = "";
        public string? ProductTypeName { get; set; }
        public List<PlantRefDTO> Plants { get; set; } = new();
        public List<NamedRefDTO> HealthProperties { get; set; } = new();
        public List<NamedRefDTO> ActiveSubstances { get; set; } = new();
        public List<DiseaseRefDTO> Diseases { get; set; } = new();
        public List<NamedRefDTO> Contraindications { get; set; } = new();
    }

    // =====================================================================
    //  Słowniki produktów (api/dictionaries/...)
    // =====================================================================

    public class GetProductTypeDTO
    {
        public int ProductTypeId { get; set; }
        public string Name { get; set; } = "";
    }

    public class GetHealthPropertyDTO
    {
        public int HealthPropertyId { get; set; }
        public string Property { get; set; } = "";
    }

    public class GetActiveSubstanceDTO
    {
        public int ActiveSubstanceId { get; set; }
        public string ActiveSubstance { get; set; } = "";
    }

    public class GetDiseaseDTO
    {
        public int DiseaseId { get; set; }
        public string Name { get; set; } = "";
        public string Symptoms { get; set; } = "";
    }

    public class GetContraindicationDTO
    {
        public int ContraindicationId { get; set; }
        public string Name { get; set; } = "";
    }
}
