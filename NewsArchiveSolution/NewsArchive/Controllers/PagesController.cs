using Microsoft.AspNetCore.Mvc;

namespace NewsArchive.Controllers
{
    public class PagesController : Controller
    {
        [HttpGet("about")]
        public IActionResult About() => View();

        [HttpGet("contact")]
        public IActionResult Contact() => View();

        [HttpGet("editorial-policy")]
        public IActionResult EditorialPolicy() => View();

        [HttpGet("privacy")]
        public IActionResult Privacy() => View();

        [HttpGet("terms")]
        public IActionResult Terms() => View();

        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            Response.StatusCode = code;
            ViewBag.Code = code;
            return View();
        }
    }
}
