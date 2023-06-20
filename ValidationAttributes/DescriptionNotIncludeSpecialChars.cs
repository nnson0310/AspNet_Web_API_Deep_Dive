using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class DescriptionNotIncludeSpecialChars: ValidationAttribute
{
    public DescriptionNotIncludeSpecialChars()
    {

    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not CourseForManipulateDto course)
        {
            throw new Exception($"Attribute {nameof(DescriptionNotIncludeSpecialChars)} must be applied to {nameof(CourseForManipulateDto)}");
        }

        string specialChars = "!@#$%^&*()_+{}:\"<>?[];',./";
        if (course.Description!.Any(d => specialChars.Contains(d)))
        {
            return new ValidationResult("Description must not contain special chars", new[] { nameof(CourseForManipulateDto) });
        }

        return ValidationResult.Success;
    }
}
