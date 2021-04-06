using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace MultiMinesweeper.Controllers
{
    public class LobbyController : Controller
    {
        private readonly RepositoryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LobbyController(RepositoryContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _httpContextAccessor = contextAccessor;
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

        [HttpGet]
        [Route("Lobby/CheckIdentityCookie")]
        public JsonResult CheckIdentityCookie()
        {
            JObject cookie;
            string identityCookie = _httpContextAccessor.HttpContext.Request.Cookies[".AspNetCore.Cookies"];
            if (identityCookie == null)
            {
                cookie = new JObject(new JProperty("isCookieAlive", false));
                return Json(cookie);
            }
            cookie = new JObject(new JProperty("isCookieAlive", true));
            return Json(cookie);
        }
        
        [Authorize]
        [HttpGet]
        [Route("Lobby/Identity")]
        public JsonResult Identity()
        {
           var playerPoints = _context.Users.Where(l => l.Login == User.Identity.Name).Select(l => l.Points ).SingleOrDefault();
           
            JObject player = new JObject(
                new JProperty("player", User.Identity.Name),
                new JProperty("points", playerPoints));

            return Json(player);
        }
    }
}