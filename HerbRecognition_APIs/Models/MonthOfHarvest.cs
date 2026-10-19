using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class MonthOfHarvest
{
    public int Id { get; set; }

    public string MonthName { get; set; } = null!;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual ICollection<Flower> Flowers { get; set; } = new List<Flower>();

    public virtual ICollection<Fruit> Fruits { get; set; } = new List<Fruit>();

    public virtual ICollection<Root> Roots { get; set; } = new List<Root>();

    public virtual ICollection<Hat> Hats { get; set; } = new List<Hat>();

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();

    public virtual ICollection<Sap> Saps { get; set; } = new List<Sap>();

    public virtual ICollection<Stalk> Stalks { get; set; } = new List<Stalk>();
}