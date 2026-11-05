using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Picture
{
    public int Id { get; set; }

    public int? Plantid { get; set; } // null jesli zdjecie dla zgloszenia

    public string Picturelink { get; set; } = null!; //link albo sciezka na serwerze: /media/pictures/{guid}.webp – miniatura obok jako {guid}_thumb.webp.
    public string? Zrodlo { get; set; }

    public bool CzyGlowneZdjecie { get; set; } = false;

    public int? UserInputId { get; set; } // jesli null to zdjecie do zatwierdoznego

    public virtual Plant Plant { get; set; } = null!;
}
