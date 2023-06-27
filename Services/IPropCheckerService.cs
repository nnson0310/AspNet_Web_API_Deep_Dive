namespace CourseLibrary.API.Services
{
    public interface IPropCheckerService
    {
        bool TypeHasProperties<T>(string? fields);
    }
}