using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Producttype
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
