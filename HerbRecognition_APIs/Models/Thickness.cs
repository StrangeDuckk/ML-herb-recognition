using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Thickness
{
    public int Id { get; set; }

    public string Thickness1 { get; set; } = null!;

    public virtual ICollection<Fruit> Fruits { get; set; } = new List<Fruit>();

    public virtual ICollection<Hat> Hats { get; set; } = new List<Hat>();

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();

    public virtual ICollection<Root> Roots { get; set; } = new List<Root>();
}
