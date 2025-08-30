namespace Library.Models;
using System.ComponentModel.DataAnnotations;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200)]
    public string Title { get; set; }

    [Required(ErrorMessage = "Author is required")]
    [StringLength(150)]
    public string Author { get; set; }

    [Required(ErrorMessage = "Cover image is required")]
    public string CoverImageUrl { get; set; }

    public int? YearPublished { get; set; }

    [DataType(DataType.MultilineText)]
    public string Description { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.Now;

    public string Status { get; set; } = "Available";
    
    //тут я хотел поставить required но с sqlite проблемы возникли "SQLite Error 19: 'FOREIGN KEY constraint failed'", починить не получилось
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}