using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Plant
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Polishname { get; set; } = null!;

    public string? Latinname { get; set; }

    public string? Subriquet { get; set; }

    public int Planttypeid { get; set; }

    public int? Sapid { get; set; }

    public int? Rootid { get; set; }

    public int Stalkid { get; set; }

    public int Occuranceid { get; set; }

    public int? Hatid { get; set; }

    public int? Leafid { get; set; }

    public int? Flowerid { get; set; }

    public int? Fruitid { get; set; }

    public int? Similarplantsid { get; set; }

    public int? Poisonabilityid { get; set; }

    public virtual Flower? Flower { get; set; }

    public virtual Fruit? Fruit { get; set; }

    public virtual Hat? Hat { get; set; }

    public virtual ICollection<Plant> InverseSimilarplants { get; set; } = new List<Plant>();

    public virtual Leaf? Leaf { get; set; }

    public virtual Occurance Occurance { get; set; } = null!;

    public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();

    public virtual ICollection<PlantProduct> PlantProducts { get; set; } = new List<PlantProduct>();

    public virtual Planttype Planttype { get; set; } = null!;

    public virtual Poisonability? Poisonability { get; set; }

    public virtual Root? Root { get; set; }

    public virtual Sap? Sap { get; set; }

    //public virtual List<Plant>? Similarplants { get; set; }//todo sprawdzic czy nie bedzie powodowac bledow, rekord -> list
    public virtual ICollection<PlantSimilarPlant> SimilarPlants { get; set; } = new List<PlantSimilarPlant>();
    public virtual ICollection<PlantSimilarPlant> SimilarTo { get; set; } = new List<PlantSimilarPlant>();
    public virtual Stalk Stalk { get; set; } = null!;
}
