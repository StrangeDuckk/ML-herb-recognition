using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Flavour
{
    public int Id { get; set; }

    public string FlavourName { get; set; } = null!;

    public virtual ICollection<Flower> Flowers { get; set; } = new List<Flower>();

    public virtual ICollection<Fruit> Fruits { get; set; } = new List<Fruit>();

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();
}
