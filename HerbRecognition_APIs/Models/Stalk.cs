using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Stalk
{
    public int Id { get; set; }

    public int Shapeid { get; set; }

    public int Colorid { get; set; }

    public int Surfaceid { get; set; }
    public bool IsHollow {  get; set; }

    public virtual Color Color { get; set; } = null!;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual Shape Shape { get; set; } = null!;

    public virtual Surface Surface { get; set; } = null!;
}
