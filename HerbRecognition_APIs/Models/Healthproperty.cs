using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Healthproperty
{
    public int Id { get; set; }

    public string Property { get; set; } = null!;

    public int Activesubstanceid { get; set; }

    public int Diseaseid { get; set; }

    public virtual Activesubstance Activesubstance { get; set; } = null!;

    public virtual Disease Disease { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
