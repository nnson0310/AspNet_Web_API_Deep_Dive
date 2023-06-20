
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController] 
public class AuthorsCollectionController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    public AuthorsCollectionController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpPost("api/authors/create")] 
    public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateListOfAuthors(
        IEnumerable<AuthorForCreationDto> authorForCreations)
    {
        var authorEntities = _mapper.Map<IEnumerable<Author>>(authorForCreations);
        foreach(var author in authorEntities)
        {

            _courseLibraryRepository.AddAuthor(author);
        }

        await _courseLibraryRepository.SaveAsync();

        var listAuthorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
        var authorIdsAsString = string.Join(",", listAuthorsToReturn.Select(a => a.Id));

        // return them
        return CreatedAtRoute("GetListOfAuthors", new { authorIds = authorIdsAsString }, listAuthorsToReturn);
    }

    [HttpPost("({authorIds})", Name = "GetListOfAuthors")]
    public async Task<ActionResult<IEnumerable<AuthorForCreationDto>>> GetListOfAuthors(
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]
        [FromRoute] IEnumerable<Guid> authorIds)
    {
        var authorEntities = await _courseLibraryRepository.GetAuthorsAsync(authorIds);
        if (authorIds.Count() != authorEntities.Count())
        {
            return NotFound();
        }

        var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

        return Ok(authorsToReturn);
    }
}
