using System;
using System.Collections.Generic;

namespace HerbRecognition_APIs.Models;

public partial class Picture
{
    public int Id { get; set; }

    // dokładnie jeden z czterech właścicieli jest ustawiony (CHECK picture_one_owner)
    public int? Plantid { get; set; }
    public int? UserInputId { get; set; }
    public int? ProductId { get; set; }
    public int? ProductInputId { get; set; }

    /// <summary>
    /// Zewnętrzny URL (stare dane) albo ścieżka pliku na serwerze:
    /// /media/pictures/{guid}.webp – miniatura obok jako {guid}_thumb.webp.
    /// </summary>
    public string Picturelink { get; set; } = null!;

    public string? Zrodlo { get; set; }

    public bool CzyGlowneZdjecie { get; set; }

    public virtual Plant? Plant { get; set; }
}
