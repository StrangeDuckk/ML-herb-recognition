using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class ProductInput
{
    public int Id { get; set; }

    public string Inputdata { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}