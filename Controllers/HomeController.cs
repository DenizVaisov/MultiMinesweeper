using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MultiMinesweeper.Controllers
{
    public class HomeController : Controller
    {
        private readonly RepositoryContext _context;

        public HomeController(RepositoryContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            var query = _context.Newses.FromSql("SELECT * FROM news").ToList();
            return View(query);
        }
    }
}