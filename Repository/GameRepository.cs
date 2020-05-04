using System.Collections.Generic;

namespace MultiMinesweeper.Repository
{
    public static class GameRepository 
    {
        public static Dictionary<string, Game> Games { get; } = new Dictionary<string, Game>();
//        public List<Game> Games { get; } = new List<Game>();
    }
}