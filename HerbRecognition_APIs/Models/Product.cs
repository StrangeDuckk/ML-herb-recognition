using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Recipe { get; set; } = null!;

    public int Healthpropertyid { get; set; }

    public string? Contraindication { get; set; }

    public int Producttypeid { get; set; }

    public virtual Healthproperty Healthproperty { get; set; } = null!;

    public virtual ICollection<PlantProduct> PlantProducts { get; set; } = new List<PlantProduct>();

    public virtual Producttype Producttype { get; set; } = null!;
}
