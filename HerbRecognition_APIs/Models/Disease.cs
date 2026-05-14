using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Disease
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Symptoms { get; set; } = null!;

    public virtual ICollection<Healthproperty> Healthproperties { get; set; } = new List<Healthproperty>();
}
