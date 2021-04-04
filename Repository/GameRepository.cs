using System;
using System.Collections.Generic;
using MultiMinesweeper.Game;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Repository
{
    public static class GameRepository 
    {
        public static Dictionary<string, GameLogic> Games { get; } = new Dictionary<string, GameLogic>();
        
        public static Dictionary<string, string> PlayersOnGameHubConnect { get; } = new Dictionary<string, string>();
        public static Dictionary<string, string> PlayersConnections { get; } = new Dictionary<string, string>();
    }
}