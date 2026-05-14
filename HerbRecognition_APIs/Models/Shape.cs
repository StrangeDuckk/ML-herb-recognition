using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Shape
{
    public int Id { get; set; }

    public string Shape1 { get; set; } = null!;

    public virtual ICollection<Flower> Flowers { get; set; } = new List<Flower>();

    public virtual ICollection<Fruit> Fruits { get; set; } = new List<Fruit>();

    public virtual ICollection<Hat> Hats { get; set; } = new List<Hat>();

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();

    public virtual ICollection<Stalk> Stalks { get; set; } = new List<Stalk>();
}
