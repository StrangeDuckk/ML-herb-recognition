using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

/// <summary>Słownik przeciwwskazań (np. "ciąża", "alergia na astrowate").</summary>
public partial class Contraindication
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}