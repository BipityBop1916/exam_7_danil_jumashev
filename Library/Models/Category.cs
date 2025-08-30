namespace Library.Models;
using System.ComponentModel.DataAnnotations;

public class Category
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}