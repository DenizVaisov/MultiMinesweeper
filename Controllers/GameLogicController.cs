using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MultiMinesweeper.Game;
using MultiMinesweeper.Model;

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
            GameLogic game = new GameLogic();
            return Json(game.InitialiazeOwnField());
        }

        [HttpPost]
        [Route("GameLogic/GameResult")]
        public async Task GameResult([FromBody] HighScores highScores)
        {
            _repositoryContext.HighScores.Add(new HighScores
            {
                Id = Guid.NewGuid(), Points = highScores.Points, PlusRating = highScores.PlusRating,
                MinusRating = highScores.MinusRating, Win = highScores.Win, Lose = highScores.Lose
            });

            await _repositoryContext.SaveChangesAsync();

            var query = await _repositoryContext.Users.FirstOrDefaultAsync(item => item.Login == highScores.Win);
            var firstPlayer = await _repositoryContext.Users.FirstOrDefaultAsync(item => item.Login == highScores.Win);

            if (query != null)
            {
                query.Points = firstPlayer.Points + highScores.PlusRating;
                _repositoryContext.Users.Update(query);
                _repositoryContext.SaveChanges();
            }

            var records = await _repositoryContext.Records.FirstOrDefaultAsync(item => item.Login == highScores.Win);
            if (records == null)
            {
                _repositoryContext.Records.Add(new Records
                {
                    Login = highScores.Win, Lose = 0, Win = 1, Points = firstPlayer.Points
                });

                await _repositoryContext.SaveChangesAsync();
            }
            else
            {
                var _recordQuery =
                    await _repositoryContext.Records.FirstOrDefaultAsync(item => item.Login == highScores.Win);
                var recordQuery =
                    await _repositoryContext.Records.FirstOrDefaultAsync(item => item.Login == highScores.Win);

                recordQuery.Points = firstPlayer.Points;
                recordQuery.Login = firstPlayer.Login;
                recordQuery.Win = _recordQuery.Win + 1;
                _repositoryContext.Records.Update(recordQuery);
                await _repositoryContext.SaveChangesAsync();
            }

            var _query = await _repositoryContext.Users.FirstOrDefaultAsync(item => item.Login == highScores.Lose);
            var secondPlayer = await _repositoryContext.Users.FirstOrDefaultAsync(item => item.Login == highScores.Lose);
            
            var _records = await _repositoryContext.Records.FirstOrDefaultAsync(item => item.Login == highScores.Lose);
            if (_records == null)
            {
                _repositoryContext.Records.Add(new Records
                {
                    Login = highScores.Lose, Lose = 1, Win = 0, Points = 0
                });

                await _repositoryContext.SaveChangesAsync();
            }
            else
            {
                var recQuery =
                    await _repositoryContext.Records.FirstOrDefaultAsync(item => item.Login == highScores.Lose);
                var _recQuery =
                    await _repositoryContext.Records.FirstOrDefaultAsync(item => item.Login == highScores.Lose);

                if (secondPlayer.Points + highScores.MinusRating < 0)
                {
                    _recQuery.Points = 0;
                    _recQuery.Login = secondPlayer.Login;
                    _recQuery.Lose = recQuery.Lose + 1;
                    _repositoryContext.Records.Update(_recQuery);
                    await _repositoryContext.SaveChangesAsync();
                }
                else
                {
                    _recQuery.Points = secondPlayer.Points + highScores.MinusRating;
                    _recQuery.Login = secondPlayer.Login;
                    _recQuery.Lose = recQuery.Lose + 1;
                    _repositoryContext.Records.Update(_recQuery);
                    await _repositoryContext.SaveChangesAsync();
                }
            }
            
            if (_query != null)
            {
                if (secondPlayer.Points + highScores.MinusRating < 0)
                {
                    _query.Points = 0;
                    _repositoryContext.Users.Update(_query);
                    _repositoryContext.SaveChanges();
                }
                else
                {
                    _query.Points = secondPlayer.Points + highScores.MinusRating;
                    _repositoryContext.Users.Update(_query);
                    _repositoryContext.SaveChanges();
                }
            }

        }
    }
}