using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseForCreationDto: CourseForManipulateDto
{
    [Required(ErrorMessage = "Description is required")]
    public override string? Description { get => base.Description; set => base.Description = value; }
}

