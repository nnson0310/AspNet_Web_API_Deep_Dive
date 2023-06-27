namespace CourseLibrary.API.Services;

public class PropMapping<TSource, TDestination>: IPropMapping
{
    public Dictionary<string, PropMappingValue> MappingValues { get; private set; }

    public PropMapping(Dictionary<string, PropMappingValue> mappingValues)
    {
        MappingValues = mappingValues ?? throw new ArgumentNullException(nameof(mappingValues));
    }
}

