namespace CourseLibrary.API.Services;

public class PropMappingValue
{
    public IEnumerable<string> Properties { get; private set; }

    public bool ReverseOrder { get; private set; }

    public PropMappingValue(IEnumerable<string> properties, bool reverseOrder = false)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        ReverseOrder = reverseOrder;
    }
}

