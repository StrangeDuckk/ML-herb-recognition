using Microsoft.AspNetCore.Mvc;

namespace HerbRecognition_APIs.DTOs
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
