using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Occurance
{
    public int Id { get; set; }

    public string Occurance1 { get; set; } = null!;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
