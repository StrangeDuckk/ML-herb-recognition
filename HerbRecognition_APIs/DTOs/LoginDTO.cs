using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    public class LoginDTO
    {
        [Required] public string Username { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }
}
