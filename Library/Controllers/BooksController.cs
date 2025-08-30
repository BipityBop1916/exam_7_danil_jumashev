namespace Library.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using System.Threading.Tasks;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 8;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var books = await _context.Books
            .OrderByDescending(b => b.DateAdded)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        int totalBooks = await _context.Books.CountAsync();
        ViewBag.TotalPages = (int)Math.Ceiling(totalBooks / (double)PageSize);
        ViewBag.CurrentPage = page;

        return View(books);
    }
    
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Author,CoverImageUrl,YearPublished,Description")] Book book)
    {
        if (ModelState.IsValid)
        {
            book.DateAdded = DateTime.Now;
            book.Status = "Available";

            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }
    
    public async Task<IActionResult> Borrow(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BorrowBook(int id, string email)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null || book.Status == "Loaned out")
        {
            ViewBag.ErrorMessage = "Book unavailable";
            return View("Borrow", book);
        }

        var user = await _context.Users.Include(u => u.BookLoans)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            ViewBag.ErrorMessage = "User not found";
            return View("Borrow", book);
        }

        int activeLoans = user.BookLoans.Count(bl => bl.DateReturned == null);
        if (activeLoans >= 3)
        {
            ViewBag.ErrorMessage = "User can't have more than 3 books";
            return View("Borrow", book);
        }

        book.Status = "Loaned out";
        var loan = new BookLoan
        {
            BookId = book.Id,
            UserId = user.Id,
            DateIssued = DateTime.Now
        };

        _context.BookLoans.Add(loan);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Books");
    }
}
