using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    public class LoginDTO
    {
        [Required][StringLength(100)] public string Username { get; set; } = "";
        [Required][StringLength(100)] public string Password { get; set; } = "";
    }
}
