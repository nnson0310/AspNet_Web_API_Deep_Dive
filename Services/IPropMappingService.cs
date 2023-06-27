namespace CourseLibrary.API.Services
{
    public interface IPropMappingService
    {
        Dictionary<string, PropMappingValue> GetPropMapping<TSource, TDestination>();
    }
}