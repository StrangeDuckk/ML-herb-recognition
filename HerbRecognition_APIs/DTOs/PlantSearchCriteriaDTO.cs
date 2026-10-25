namespace HerbRecognition_APIs.DTOs
{
    /// <summary>
    /// Kryteria wyszukiwania przez formularz. Każde pole jest opcjonalne:
    /// null (albo 0 dla Id) = "nie wiem" – takie pole nie jest porównywane.
    /// </summary>
    public class PlantSearchCriteriaDTO
    {
        // ----- tekst (waga 1) -----
        public string? Name { get; set; }
        public string? PolishName { get; set; }
        public string? LatinName { get; set; }
        public string? Subriquet { get; set; }
        public string? MoreInfo { get; set; }

        // ----- słowniki (waga 3) -----
        public int? PlantTypeId { get; set; }
        public int? OccuranceId { get; set; }

        /// <summary>null = nie wiem, 0 = nietrująca, inne = konkretny wpis słownika.</summary>
        public int? PoisonabilityId { get; set; }

        public int? MonthOfHarvestId { get; set; }

        // ----- części rośliny: null = użytkownik nie opisywał tej części -----
        public PartCriteriaDTO? Stalk { get; set; }
        public PartCriteriaDTO? Sap { get; set; }
        public PartCriteriaDTO? Root { get; set; }
        public PartCriteriaDTO? Hat { get; set; }
        public PartCriteriaDTO? Leaf { get; set; }
        public PartCriteriaDTO? Flower { get; set; }
        public PartCriteriaDTO? Fruit { get; set; }

        // ----- ustawienia wyszukiwania -----
        public int MinMatchPercent { get; set; }
        public bool IncludePending { get; set; } = true;
    }

    /// <summary>
    /// Wspólny opis części rośliny. Każda część używa tylko swoich pól
    /// (np. sok: ColorId, LeavesStain, Sticky), reszta zostaje null.
    /// </summary>
    public class PartCriteriaDTO
    {
        // słowniki (waga 3)
        public int? ShapeId { get; set; }
        public int? ColorId { get; set; }
        public int? SurfaceId { get; set; }
        public int? ThicknessId { get; set; }
        public int? FlavourId { get; set; }
        public int? MonthOfHarvestId { get; set; }

        // Tak / Nie (waga 2), null = nie wiem
        public bool? IsHollow { get; set; }
        public bool? LeavesStain { get; set; }
        public bool? Sticky { get; set; }
        public bool? HasSpots { get; set; }
        public bool? HasGills { get; set; }
        public bool? HasRing { get; set; }
        public bool? HasStripes { get; set; }
        public bool? HasHoles { get; set; }

        // liczby (waga 1)
        public decimal? Size { get; set; }        // długość liścia / rozmiar kwiatu w cm
        public int? ScentPower { get; set; }      // 0–5

        // tekst (waga 1)
        public string? MoreInfo { get; set; }
    }
}
