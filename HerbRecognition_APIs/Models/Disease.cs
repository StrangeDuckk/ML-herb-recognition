using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

/// <summary>Słownik chorób.</summary>
public partial class Disease
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Symptoms { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}