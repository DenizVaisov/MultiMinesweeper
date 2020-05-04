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
    public class AuthController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly RepositoryContext _context;
        public AuthController(RepositoryContext context)
        {
            _context = context;
        }
        
        public void LoggedUser(string player)
        {
            string writePath = @"C:\С#_projects\MultiMinesweeper\MultiMinesweeper\json\player.json";

            JObject mf = new JObject(
                new JProperty("player", player));
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(writePath, false, System.Text.Encoding.Default))
                {
                    streamWriter.Write(mf);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
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
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Login && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(model.Login);
                    LoggedUser(model.Login);
                    return RedirectPermanent("http://localhost:8080/");
                }
                return View(new SignIn{ Message = "Пользователя с таким логином или паролем нет", Login = model.Login});
            }
            return View(model);
        }
        
        [HttpGet] 
        public ActionResult Register()
        {
            return View(new Register{ Message = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Login && u.Password == model.Password);
                
                if (user == null)
                {
                    _context.Users.Add(new User { Email = model.Login, Password = model.Password });
                    await _context.SaveChangesAsync();
 
                    await Authenticate(model.Login); 
 
                    return RedirectToAction("Index", "Auth");
                }
                return View(new Register{ Message = "Данный пользователь уже зарегистрирован", Login = model.Login});
            }

            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Auth");
        }
    }
}