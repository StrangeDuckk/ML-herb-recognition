using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class PlantProduct
{
    public int Id { get; set; }

    public int Plantsid { get; set; }

    public int Productsid { get; set; }

    public virtual Plant Plants { get; set; } = null!;

    public virtual Product Products { get; set; } = null!;
}
