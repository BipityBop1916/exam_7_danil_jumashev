using Microsoft.AspNetCore.Mvc.Rendering;

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

    public async Task<IActionResult> Index(string sortOrder, string searchString, int page = 1)
    {
        var books = _context.Books.Include(b => b.Category).AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower();
            books = books.Where(b => b.Title.ToLower().Contains(searchString)
                                     || b.Author.ToLower().Contains(searchString)
                                     || b.Status.ToLower().Contains(searchString));
            ViewBag.SearchString = searchString;
        }

        books = sortOrder switch
        {
            "title_desc" => books.OrderByDescending(b => b.Title),
            "author" => books.OrderBy(b => b.Author),
            "author_desc" => books.OrderByDescending(b => b.Author),
            "status" => books.OrderBy(b => b.Status),
            "status_desc" => books.OrderByDescending(b => b.Status),
            _ => books.OrderBy(b => b.Title)
        };

        int totalBooks = await books.CountAsync();
        int pageSize = 8;
        int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

        var booksq = await books
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;
        ViewBag.SortOrder = sortOrder;

        return View(booksq);
    }

    
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
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
    
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (book == null) return NotFound();

        return View(book);
    }
    
    public async Task<IActionResult> Loaned()
    {
        var loans = await _context.BookLoans
            .Include(bl => bl.Book)
            .Include(bl => bl.User)
            .Where(bl => bl.DateReturned == null)
            .ToListAsync();

        return View(loans);
    }

    public IActionResult AddCategory()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(string categoryName)
    {
        if (!string.IsNullOrEmpty(categoryName))
        {
            var category = new Category
            {
                Name = categoryName
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "Books");
        }

        ViewBag.ErrorMessage = "Category name cannot be empty.";
        return View();
    }
    
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category) 
            .FirstOrDefaultAsync(m => m.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");

        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,CoverImageUrl,YearPublished,Description,Status,CategoryId")] Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");

        return View(book);
    }
    
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category) 
            .FirstOrDefaultAsync(m => m.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}
