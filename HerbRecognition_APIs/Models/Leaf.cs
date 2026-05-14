using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Leaf
{
    public int Id { get; set; }

    public bool Stripes { get; set; }

    public bool Spots { get; set; }

    public bool Holes { get; set; }

    public int Leafshapeid { get; set; }

    public int Leafcolorid { get; set; }

    public int Surfaceid { get; set; }

    public decimal Leaflength { get; set; }

    public int Thicknessid { get; set; }

    public int? Flavourid { get; set; }

    public virtual Flavour? Flavour { get; set; }

    public virtual Color Leafcolor { get; set; } = null!;

    public virtual Shape Leafshape { get; set; } = null!;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual Surface Surface { get; set; } = null!;

    public virtual Thickness Thickness { get; set; } = null!;
}
