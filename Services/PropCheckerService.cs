using System.Reflection;

namespace CourseLibrary.API.Services;

public class PropCheckerService : IPropCheckerService
{
    public bool TypeHasProperties<T>(string? fields)
    {

        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        else
        {
            var fieldNames = fields.Split(',');
            foreach (var fieldName in fieldNames)
            {
                var propertyInfo = typeof(T).GetProperty(fieldName,
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase |
                    BindingFlags.Instance);
                if (propertyInfo is null)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

