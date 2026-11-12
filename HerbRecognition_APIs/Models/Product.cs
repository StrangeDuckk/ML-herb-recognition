using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Recipe { get; set; } = null!;

    public int Producttypeid { get; set; }

    public virtual Producttype Producttype { get; set; } = null!;

    // cztery słowniki – każdy wiele-do-wielu
    public virtual ICollection<Healthproperty> Healthproperties { get; set; } = new List<Healthproperty>();

    public virtual ICollection<Activesubstance> Activesubstances { get; set; } = new List<Activesubstance>();

    public virtual ICollection<Disease> Diseases { get; set; } = new List<Disease>();

    public virtual ICollection<Contraindication> Contraindications { get; set; } = new List<Contraindication>();

    public virtual ICollection<PlantProduct> PlantProducts { get; set; } = new List<PlantProduct>();
}