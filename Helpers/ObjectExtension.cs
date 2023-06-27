using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;
public static class ObjectExtension
{
    public static ExpandoObject ShapeData<TSource>(
        this TSource source,
        string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var dataShapedObj = new ExpandoObject();

        if (string.IsNullOrWhiteSpace(fields))
        {
            var propertyInfos = typeof(TSource).GetProperties(
                BindingFlags.Public |
                BindingFlags.IgnoreCase |
                BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                var propertyValue = propertyInfo.GetValue(source, null);

                ((IDictionary<string, object?>)dataShapedObj).Add(propertyInfo.Name, propertyValue);
            }
        }
        else
        {
            var fieldNames = fields.Split(',');
            foreach (var fieldName in fieldNames)
            {
                var propertyInfo = typeof(TSource).GetProperty(fieldName,
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase |
                    BindingFlags.Instance);
                if (propertyInfo is null)
                {
                    throw new ArgumentNullException($"{fieldName} does not exist.");
                }
                var propertyValue = propertyInfo.GetValue(source, null);

                ((IDictionary<string, object?>)dataShapedObj).Add(propertyInfo.Name, propertyValue);
            }
        }

        return dataShapedObj;
    }
}

