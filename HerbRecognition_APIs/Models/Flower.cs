using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Flower
{
    public int Id { get; set; }

    public decimal Sizeincm { get; set; }

    public int Colorid { get; set; }

    public int Shapeid { get; set; }

    public int? Flavourid { get; set; }

    public int Scentpower { get; set; }

    public virtual Color Color { get; set; } = null!;

    public virtual Flavour? Flavour { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual Shape Shape { get; set; } = null!;
}
