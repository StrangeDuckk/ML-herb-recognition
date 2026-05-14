using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Sap
{
    public int Id { get; set; }

    public int Colorid { get; set; }

    public bool Leavesstains { get; set; }

    public bool Sticky { get; set; }

    public virtual Color Color { get; set; } = null!;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
