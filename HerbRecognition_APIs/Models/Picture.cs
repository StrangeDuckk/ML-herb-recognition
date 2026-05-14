using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Picture
{
    public int Id { get; set; }

    public int Plantid { get; set; }

    public string Picturelink { get; set; } = null!;

    public virtual Plant Plant { get; set; } = null!;
}
