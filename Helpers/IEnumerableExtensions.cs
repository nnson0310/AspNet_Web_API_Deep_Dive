using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;
public static class IEnumerableExtensions
{
    public static IEnumerable<ExpandoObject> ShapeData<TSource>(
        this IEnumerable<TSource> source,
        string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var expandoObjectList = new List<ExpandoObject>();
        var propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            var propertyInfos = typeof(TSource).GetProperties(
                BindingFlags.Public |
                BindingFlags.IgnoreCase |
                BindingFlags.Instance);
            propertyInfoList.AddRange(propertyInfos);
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
                propertyInfoList.Add(propertyInfo);
            }
        }

        foreach (TSource item in source)
        {
            var dataShapedObject = new ExpandoObject();

            foreach (var propertyInfo in propertyInfoList)
            {
                var propValue = propertyInfo.GetValue(item, null);
                ((IDictionary<string, object?>)dataShapedObject).Add(propertyInfo.Name, propValue);
            }

            expandoObjectList.Add(dataShapedObject);
        }

        return expandoObjectList;
    }
}

