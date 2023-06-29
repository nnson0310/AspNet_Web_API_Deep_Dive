using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : Controller
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;

        public RootController(ICourseLibraryRepository courseLibraryRepository)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
        }


        [HttpGet(Name = "GetRoot")]
        public async Task<IActionResult> RootApi()
        {
            var authors = await this._courseLibraryRepository.GetAuthorsAsync();
            var authorId = authors.FirstOrDefault()!.Id;

            var links = new List<LinkDto>();
            links.Add(new(
                Url.Link("GetRoot", new { }),
                "self",
                "GET"));

            links.Add(new(
                Url.Link("GetAllAuthors", new { }),
                "get-authors",
                "GET"));

            links.Add(new(
                Url.Link("GetAuthor", new { authorId }),
                "get-author",
                "GET"));

            return Ok(links);
        }
    }
}
