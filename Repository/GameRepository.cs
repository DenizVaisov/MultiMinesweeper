using System.Collections.Generic;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Repository
{
    public static class GameRepository 
    {
        public static Dictionary<string, Game> Games { get; } = new Dictionary<string, Game>();
    }
}