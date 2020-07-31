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
        
        [HttpGet]
        [Route("Lobby/Records")]
        public JsonResult Records()
        {
            var efQuery = _context.Records.FromSql("SELECT * FROM records");
            var sortByRating = efQuery.OrderByDescending(p => p.Points);
            
            return Json(sortByRating);
        }
        
        [Authorize]
        [HttpGet]
        [Route("Lobby/Identity")]
        public JsonResult Identity()
        {
           var efQuery = _context.Users.Where(l => l.Login == User.Identity.Name).Select(l => l.Points ).SingleOrDefault();
           
            JObject player = new JObject(
                new JProperty("player", User.Identity.Name),
                new JProperty("points", efQuery));

            return Json(player);
        }
    }
}