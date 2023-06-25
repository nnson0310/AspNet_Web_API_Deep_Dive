
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Profiles;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet("api/author/all", Name = "GetAllAuthors")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
        [FromQuery] AuthorResourcesParameters authorResourcesParameters)
    {

        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourcesParameters);

        var previousPageLink = authorsFromRepo.HasPrevious ? CreatePaginationMetaData(HasPage.HasPrevious, authorResourcesParameters) : null;
        var nextPageLink = authorsFromRepo.HasNext ? CreatePaginationMetaData(HasPage.HasNext, authorResourcesParameters) : null;

        var paginationMetaData = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            totalPages = authorsFromRepo.TotalPages,
            currentPage = authorsFromRepo.CurrentPage,
            previousPageLink,
            nextPageLink
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetaData));

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }

    private string? CreatePaginationMetaData(HasPage type, AuthorResourcesParameters authorResourcesParameters)
    {
        return type switch
        {
            HasPage.HasPrevious => Url.Link("GetAllAuthors", new
            {
                pageNumber = authorResourcesParameters.PageNumber - 1,
                pageSize = authorResourcesParameters.PageSize,
                mainCategory = authorResourcesParameters.MainCategory,
                searchQuery = authorResourcesParameters.SearchQuery
            }),
            HasPage.HasNext => Url.Link("GetAllAuthors", new
            {
                pageNumber = authorResourcesParameters.PageNumber + 1,
                pageSize = authorResourcesParameters.PageSize,
                mainCategory = authorResourcesParameters.MainCategory,
                searchQuery = authorResourcesParameters.SearchQuery
            }),
            _ => null,
        };
    }

    [HttpGet("api/author/{authorId}", Name = "GetAuthor")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost("api/author/create")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }

    [HttpOptions("/api/authors")]
    public ActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET, HEAD, POST, OPTIONS");
        return Ok();
    }
}
