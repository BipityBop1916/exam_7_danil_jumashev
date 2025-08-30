namespace Library.Models;

public class BookLoan
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; }

    public DateTime DateIssued { get; set; } = DateTime.Now;

    public DateTime? DateReturned { get; set; }
}