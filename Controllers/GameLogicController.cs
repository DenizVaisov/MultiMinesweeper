using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        [Route("GameLogic/GameField")]
        public JsonResult GameField()
        {
            Game game = new Game();
            return Json(game.InitialiazeOwnField());
        }

        [Route("GameLogic/FromClient")]
        public JsonResult FromClient()
        {
            List<object> mineFields = new List<object>();
            string str = null;
            try
            {
                string writePath = @"C:\С#_projects\MultiMinesweeper\MultiMinesweeper\json\file.json";
                using (StreamReader streamReader = new StreamReader(writePath))
                {
                    str = streamReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Json(str);
        }

        [HttpPost]
        [Route("GameLogic/ClickedCell")]
        public JsonResult ClickedCell([FromBody] MineField mineField)
        {
            List<object> rows = new List<object>();
            List<object> cols = new List<object>();
            
            rows.Add(mineField.Rows);
            cols.Add(mineField.Columns);
            
            
            string writePath = @"C:\С#_projects\MultiMinesweeper\MultiMinesweeper\json\file.json";
           
            
            JObject mf = new JObject(
                new JProperty("Columns", mineField.Columns),
                new JProperty("Rows", mineField.Rows));
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

            return Json(mf);
        }
    }
}