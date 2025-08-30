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
}
