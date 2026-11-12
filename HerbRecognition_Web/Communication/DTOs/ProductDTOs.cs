namespace HerbRecognition_Web.Communication.DTOs
{
    // =====================================================================
    //  Zgłoszenie / edycja produktu (ten sam kształt co w API)
    // =====================================================================

    public class CreateProductDTO
    {
        public string Name { get; set; } = "";
        public string Recipe { get; set; } = "";
        public int ProductTypeId { get; set; }

        public List<int>? PlantIds { get; set; }
        public List<int>? PlantUserInputIds { get; set; }

        public List<int>? HealthPropertyIds { get; set; }
        public List<int>? ActiveSubstanceIds { get; set; }
        public List<int>? DiseaseIds { get; set; }
        public List<int>? ContraindicationIds { get; set; }

        public List<string>? NewHealthProperties { get; set; }
        public List<string>? NewActiveSubstances { get; set; }
        public List<NewDiseaseDTO>? NewDiseases { get; set; }
        public List<string>? NewContraindications { get; set; }
    }

    public class NewDiseaseDTO
    {
        public string Name { get; set; } = "";
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

    public class PlantRefDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsApproved { get; set; }
    }

    public class ProductPreviewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ProductTypeName { get; set; }
        public List<PlantRefDTO> Plants { get; set; } = new();
        public List<string> Diseases { get; set; } = new();
        public List<string> HealthProperties { get; set; } = new();
        public string? MainThumbnailUrl { get; set; }
        public int PictureCount { get; set; }
        public int NewEntriesCount { get; set; }
        public bool IsPending { get; set; }
        public bool IsCorrupted { get; set; }
    }

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

    public class ProductTypeOption
    {
        public int ProductTypeId { get; set; }
        public string Name { get; set; } = "";
    }

    // =====================================================================
    //  Słowniki produktu w formularzu
    // =====================================================================

    public enum ProductDictionary { HealthProperty, ActiveSubstance, Disease, Contraindication }

    /// <summary>Wpis słownika: Id, nazwa i (dla chorób) objawy.</summary>
    public sealed record DictionaryEntry(int Id, string Name, string? Description);

    /// <summary>Propozycja nowego wpisu wpisana przez użytkownika.</summary>
    public sealed class DictionaryProposal
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }   // objawy – tylko choroby
    }

    public static class ProductDictionaries
    {
        public static readonly ProductDictionary[] All =
        {
            ProductDictionary.Disease,
            ProductDictionary.HealthProperty,
            ProductDictionary.ActiveSubstance,
            ProductDictionary.Contraindication
        };

        /// <summary>Adres w api/dictionaries/… i api/admin/dictionaries/…</summary>
        public static string Endpoint(this ProductDictionary d) => d switch
        {
            ProductDictionary.HealthProperty => "healthProperty",
            ProductDictionary.ActiveSubstance => "activeSubstance",
            ProductDictionary.Disease => "disease",
            _ => "contraindication"
        };

        public static string Title(this ProductDictionary d) => d switch
        {
            ProductDictionary.HealthProperty => "Właściwości zdrowotne",
            ProductDictionary.ActiveSubstance => "Substancje czynne",
            ProductDictionary.Disease => "Choroby",
            _ => "Przeciwwskazania"
        };

        public static int MaxLength(this ProductDictionary d) =>
            d == ProductDictionary.Contraindication ? 1000 : 300;

        public static bool HasDescription(this ProductDictionary d) => d == ProductDictionary.Disease;

        /// <summary>Nazwy pól w JSON-ie z api/dictionaries/…</summary>
        public static (string Id, string Name, string? Description) JsonFields(this ProductDictionary d) => d switch
        {
            ProductDictionary.HealthProperty => ("healthPropertyId", "property", null),
            ProductDictionary.ActiveSubstance => ("activeSubstanceId", "activeSubstance", null),
            ProductDictionary.Disease => ("diseaseId", "name", "symptoms"),
            _ => ("contraindicationId", "name", null)
        };
    }
}
