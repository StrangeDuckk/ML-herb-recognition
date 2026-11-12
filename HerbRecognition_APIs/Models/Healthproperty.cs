using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

/// <summary>Słownik właściwości zdrowotnych (np. "przeciwzapalne").</summary>
public partial class Healthproperty
{
    public int Id { get; set; }

    public string Property { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}