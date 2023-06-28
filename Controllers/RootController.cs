using CourseLibrary.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : Controller
    {

        [HttpGet(Name = "GetRoot")]
        public IActionResult RootApi()
        {
            var links = new List<LinkDto>();
            links.Add(new(
                Url.Link("GetRoot", new {}),
                "self",
                "GET"));

            return Ok(links);
        }
    }
}
