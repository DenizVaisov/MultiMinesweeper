using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MultiMinesweeper.Hub;
using MultiMinesweeper.Model;
using MultiMinesweeper.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiMinesweeper.Controllers
{
    using Microsoft.AspNetCore.Mvc; 
    public class GameLogicController : Controller
    {
        private readonly RepositoryContext _repositoryContext;
        public GameLogicController(RepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }
        
        [Route("GameLogic/GameField")]
        public JsonResult GameField()
        {
            Game game = new Game();
            return Json(game.InitialiazeOwnField());
        }

        [HttpPost]
        [Route("GameLogic/GameResult")]
        public async Task GameResult([FromBody] HighScores highScores)
        {
            _repositoryContext.HighScores.Add(new HighScores
            {
                Id = Guid.NewGuid(),
                Points = highScores.Points,
                PlusRating = highScores.PlusRating,
                MinusRating = highScores.MinusRating,
                Win = highScores.Win,
                Lose = highScores.Lose
            });

            await _repositoryContext.SaveChangesAsync();

            var query = await _repositoryContext.Users.FirstOrDefaultAsync(item => item.Login == highScores.Win); 
            
            if (query != null)
            {
                query.Points = highScores.PlusRating;
                _repositoryContext.Users.Update(query);
                _repositoryContext.SaveChanges();
            }
            
            var _query = await _repositoryContext.Users.FirstOrDefaultAsync(item => item.Login == highScores.Lose);
            if (_query != null)
            {
                _query.Points = highScores.MinusRating;
                _repositoryContext.Users.Update(_query);
                _repositoryContext.SaveChanges();
            }
        }
    }
}