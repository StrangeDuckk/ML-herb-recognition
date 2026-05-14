using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Poisonability
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
