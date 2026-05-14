using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Activesubstance
{
    public int Id { get; set; }

    public string Activesubstance1 { get; set; } = null!;

    public virtual ICollection<Healthproperty> Healthproperties { get; set; } = new List<Healthproperty>();
}
