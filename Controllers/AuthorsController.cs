using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Profiles;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;
    private readonly IPropCheckerService _propCheckerService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper,
        IPropCheckerService propCheckerService,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _propCheckerService = propCheckerService ?? throw new ArgumentNullException(nameof(propCheckerService));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    [HttpGet("api/author/all", Name = "GetAllAuthors")]
    public async Task<IActionResult> GetAuthors(
        [FromQuery] AuthorResourcesParameters authorResourcesParameters)
    {
        if (!_propCheckerService.TypeHasProperties<AuthorDto>(authorResourcesParameters.Fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                detail: $"Not all request data shaping exist on resources {authorResourcesParameters.Fields}"));
        }

        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourcesParameters);

        var paginationMetaData = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            totalPages = authorsFromRepo.TotalPages,
            currentPage = authorsFromRepo.CurrentPage,
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetaData));

        //create links
        var links = CreateLinkForAuthors(authorResourcesParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);
        var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo).ShapeData(authorResourcesParameters.Fields);

        var shapedAuthorWithLinks = shapedAuthors.Select(author =>
        {
            var authorAsDictionary = author as IDictionary<string, object?>;
            var authorLinks = CreateLinkForAuthor((Guid)authorAsDictionary["Id"]!, null);
            authorAsDictionary.Add("links", authorLinks);
            return authorAsDictionary;
        });

        var linkedCollectionResources = new
        {
            value = shapedAuthorWithLinks,
            links = links
        };

        // return them
        return Ok(linkedCollectionResources);
    }

    private string? CreatePaginationMetaData(HasPage type, AuthorResourcesParameters authorResourcesParameters)
    {
        return type switch
        {
            HasPage.HasPrevious => Url.Link("GetAllAuthors", new
            {
                fields = authorResourcesParameters.Fields,
                pageNumber = authorResourcesParameters.PageNumber - 1,
                pageSize = authorResourcesParameters.PageSize,
                mainCategory = authorResourcesParameters.MainCategory,
                searchQuery = authorResourcesParameters.SearchQuery
            }),
            HasPage.HasNext => Url.Link("GetAllAuthors", new
            {
                fields = authorResourcesParameters.Fields,
                pageNumber = authorResourcesParameters.PageNumber + 1,
                pageSize = authorResourcesParameters.PageSize,
                mainCategory = authorResourcesParameters.MainCategory,
                searchQuery = authorResourcesParameters.SearchQuery
            }),
            HasPage.Current => Url.Link("GetAllAuthors", new
            {
                fields = authorResourcesParameters.Fields,
                pageNumber = authorResourcesParameters.PageNumber,
                pageSize = authorResourcesParameters.PageSize,
                mainCategory = authorResourcesParameters.MainCategory,
                searchQuery = authorResourcesParameters.SearchQuery
            }),
            _ => null,
        };
    }

    [HttpGet("api/author/{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(Guid authorId, string? fields)
    {
        if (!_propCheckerService.TypeHasProperties<AuthorDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                detail: $"Not all request data shaping exist on resources {fields}"));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        var linkResources = CreateLinkForAuthor(authorId, fields);

        var returnResources = _mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields) as IDictionary<string, object>;
        returnResources.Add("links", linkResources);

        // return author
        return Ok(returnResources);
    }

    [HttpPost("api/author/create")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
        var links = CreateLinkForAuthor(authorToReturn.Id, null);
        var linkResources = authorToReturn.ShapeData(null) as IDictionary<string, object?>;
        linkResources.Add("links", links);

        return CreatedAtRoute("GetAuthor",
            new { authorId = linkResources["Id"] },
            linkResources);
    }

    [HttpOptions("/api/authors")]
    public ActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET, HEAD, POST, OPTIONS");
        return Ok();
    }

    private IEnumerable<LinkDto> CreateLinkForAuthor(Guid authorId, string? fields)
    {
        var links = new List<LinkDto>();
        if (string.IsNullOrWhiteSpace(fields))
        {
            links.Add(new(Url.Link("GetAuthor", new { authorId }), "self", "GET"));
        }
        else
        {
            links.Add(new(Url.Link("GetAuthor", new { authorId, fields }), "self", "GET"));
        }

        links.Add(new(Url.Link("CreateCourseForAuthor", new { authorId }), "create-course", "POST"));

        links.Add(new(Url.Link("GetCoursesForAuthor", new { authorId }), "get-courses", "GET"));

        return links;
    }

    private IEnumerable<LinkDto> CreateLinkForAuthors(
        AuthorResourcesParameters authorResourcesParameters,
        bool hasNext,
        bool hasPrevious)
    {
        var links = new List<LinkDto>();

        links.Add(new(CreatePaginationMetaData(HasPage.Current, authorResourcesParameters), "self", "GET"));

        if (hasNext)
        {
            links.Add(new(CreatePaginationMetaData(HasPage.HasNext, authorResourcesParameters), "next-page", "GET"));
        }

        if (hasPrevious)
        {
            links.Add(new(CreatePaginationMetaData(HasPage.HasPrevious, authorResourcesParameters), "previous-page", "GET"));
        }

        return links;
    }
}
