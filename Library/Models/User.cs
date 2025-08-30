namespace Library.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; }

    [Required, StringLength(100)]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, Phone]
    public string Phone { get; set; }

    public ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
}