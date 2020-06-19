using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MultiMinesweeper.Model;
using Newtonsoft.Json.Linq;

namespace MultiMinesweeper.Controllers
{
    public class LobbyController : Controller
    {
        private readonly RepositoryContext _context;
        public LobbyController(RepositoryContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Lobby/HighScores")]
        public JsonResult HighScores()
        {
            var efQuery = _context.HighScores.FromSql("SELECT * FROM high_scores");
            return Json(efQuery);
        }
        
        [Authorize]
        [HttpGet]
        [Route("Lobby/Identity")]
        public JsonResult Identity()
        {
           var efQuery = _context.Users.Where(l => l.Login == User.Identity.Name).Select(l => l.Points ).SingleOrDefault();
//           User user = new User{Login = efQuery};
//           Console.WriteLine(user.Login);
//
//            Console.WriteLine(User.Identity.Name);
//            
            JObject player = new JObject(
                new JProperty("player", User.Identity.Name),
                new JProperty("points", efQuery));

            return Json(player);
        }
    }
}