using System;
using MultiMinesweeper.Game;

namespace MultiMinesweeper.Controllers
{
    using Microsoft.AspNetCore.Mvc; 
    public class GameLogicController : Controller
    {
        [Route("GameLogic/GameField")]
        public JsonResult GameField()
        {
            GameLogic game = new GameLogic();
            return Json(game.InitialiazeOwnField());
        }
    }
}