using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiMinesweeper.Model;
using Newtonsoft.Json.Linq;

namespace MultiMinesweeper.Controllers
{
    public class AuthController : Controller
    {
        private readonly RepositoryContext _context;
        public AuthController(RepositoryContext context)
        {
            _context = context;
        }
        
        public ActionResult Index()
        {
            return RedirectToAction("SignIn");
        }
        
        [HttpGet]
        public ActionResult SignIn()
        {
            return View(new SignIn{ Message = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn (SignIn model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(model.Login);
                    return RedirectPermanent("http://192.168.43.159:8080");
                }
                return View(new SignIn{ Message = "Пользователя с таким логином или паролем не найден", Login = model.Login});
            }
            return View(model);
        }
        
        [HttpGet] 
        public ActionResult Register()
        {
            return View(new Register{ Message = "" });
        }

		[Route("Auth/Register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.
                    FirstOrDefaultAsync(u => u.Login == model.Login);
                if (user == null)
                {
                    _context.Users.Add(new User { Login = model.Login, Password = model.Password, Points = 0});
                    await _context.SaveChangesAsync();
                    await Authenticate(model.Login); 
 
                    return RedirectToAction("Index", "Auth");
                }
                return View(new Register{ Message = "Пользователь с таким логином уже зарегистрирован", Login = model.Login});
            }
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
				ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", 
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Auth");
        }
    }
}