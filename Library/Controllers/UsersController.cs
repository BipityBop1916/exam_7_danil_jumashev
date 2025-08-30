using Microsoft.EntityFrameworkCore;

namespace Library.Controllers;

using Microsoft.AspNetCore.Mvc;
using Library.Models;
using System.Threading.Tasks;

public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Phone")] User user)
    {
        if (ModelState.IsValid)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Books");
        }
        return View(user);
    }
    
    public IActionResult Profile()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string email, string firstName, string lastName, string phone)
    {
        if (string.IsNullOrEmpty(email))
        {
            ViewBag.ErrorMessage = "Enter your Email";
            return View();
        }

        var user = await _context.Users
            .Include(u => u.BookLoans)
            .ThenInclude(bl => bl.Book)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phone))
            {
                ViewBag.ErrorMessage = "New user, enter your name, last name and number";
                return View();
            }

            user = new User
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var loans = user.BookLoans.Where(bl => bl.DateReturned == null).Select(bl => bl.Book).ToList();
        return View("MyBooks", loans);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnBook(int bookId)
    {
        var loan = await _context.BookLoans
            .Include(bl => bl.User)
            .Include(bl => bl.Book)
            .FirstOrDefaultAsync(bl => bl.BookId == bookId && bl.DateReturned == null);
        
        if (loan == null)
        {
            return RedirectToAction("Profile");
        }

        if (loan != null)
        {
            loan.DateReturned = DateTime.Now;
            var book = await _context.Books.FindAsync(bookId);
            book.Status = "Available";
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Profile", new { email = loan.User.Email });
    }

}