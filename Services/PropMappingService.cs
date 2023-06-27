using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Services;

public class PropMappingService : IPropMappingService
{
    private readonly Dictionary<string, PropMappingValue> _authorPropMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Id", new(new[] { "Id" }) },
        { "MainCategory", new(new[] { "MainCategory" }) },
        { "Age", new(new[] { "DateOfBirth" } , true ) },
        { "Name", new(new[] { "FirstName", "LastName" }) }
    };

    private readonly IList<IPropMapping> _propMappings = new List<IPropMapping>();

    public PropMappingService()
    {
        _propMappings.Add(new PropMapping<AuthorDto, Author>(_authorPropMapping));
    }

    public Dictionary<string, PropMappingValue> GetPropMapping<TSource, TDestination>()
    {
        var matching = _propMappings.OfType<PropMapping<TSource, TDestination>>();

        if (matching.Count() == 1)
        {
            return matching.First().MappingValues;
        }

        throw new Exception($"Can not find exact mapping instance for <{typeof(TSource)}, {typeof(TDestination)}>");
    }
}

