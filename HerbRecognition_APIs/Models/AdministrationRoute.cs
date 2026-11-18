using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

/// <summary>Słownik sposobów podania (np. doustnie, inhalacja).</summary>
public partial class AdministrationRoute
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
