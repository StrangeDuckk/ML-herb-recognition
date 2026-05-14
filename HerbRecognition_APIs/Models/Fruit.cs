using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Fruit
{
    public int Id { get; set; }

    public int? Flavourid { get; set; }

    public int Shapeid { get; set; }

    public int Colorid { get; set; }

    public int Surfaceid { get; set; }

    public int Thicknessid { get; set; }

    public virtual Color Color { get; set; } = null!;

    public virtual Flavour? Flavour { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual Shape Shape { get; set; } = null!;

    public virtual Surface Surface { get; set; } = null!;

    public virtual Thickness Thickness { get; set; } = null!;
}
