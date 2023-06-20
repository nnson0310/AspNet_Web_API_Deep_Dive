using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseLibrary.API.Entities;

public class Course
{
    [Key]
    public Guid Id { get; set; }
     
    [Required]
    [MaxLength(50)]
    public string Title { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    [ForeignKey("AuthorId")]
    public Author Author { get; set; } = null!;

    public Guid AuthorId { get; set; }

    public Course(string title)
    {
        Title = title; 
    }
}

