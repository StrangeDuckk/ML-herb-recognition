using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class PlantProduct
{
    public int Id { get; set; }

    /// <summary>Zatwierdzona roślina. Null, gdy wskazane jest zgłoszenie rośliny.</summary>
    public int? Plantsid { get; set; }

    /// <summary>Roślina z kwarantanny. Po jej zatwierdzeniu przechodzi na Plantsid.</summary>
    public int? UserInputId { get; set; }

    public int Productsid { get; set; }

    public virtual Plant? Plants { get; set; }

    public virtual Product Products { get; set; } = null!;
}