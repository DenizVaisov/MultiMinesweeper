using System;
using System.Collections.Generic;
using MultiMinesweeper.Game;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Repository
{
    public static class GameRepository 
    {
        public static Dictionary<string, GameLogic> Games { get; } = new Dictionary<string, GameLogic>();
        public static List<Player> Players { get; } = new List<Player>();
    }
}