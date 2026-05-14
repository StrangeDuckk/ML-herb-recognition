using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Planttype
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
