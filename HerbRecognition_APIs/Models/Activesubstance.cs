using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

/// <summary>Słownik substancji czynnych.</summary>
public partial class Activesubstance
{
    public int Id { get; set; }

    public string Activesubstance1 { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}