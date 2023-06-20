using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

[DescriptionNotIncludeSpecialChars]
public abstract class CourseForManipulateDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(50, ErrorMessage = "Title should not exceed 50 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Description should not exceed 100 characters")]
    public virtual string? Description { get; set; } = string.Empty;
}
