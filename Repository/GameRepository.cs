using System.Collections.Generic;

namespace MultiMinesweeper.Repository
{
    public static class GameRepository 
    {
        public static Dictionary<string, Game> Games { get; } = new Dictionary<string, Game>();
//        public static List<string> Players { get; set; } = new List<string>();
//        public static Dictionary<string, string> Players { get; set; } = new Dictionary<string, string>();
        public static Queue<string> Players { get; set; } = new Queue<string>();
        public static Queue<string> PlayersInGame { get; set; } = new Queue<string>();
        public static List<string> ConnectionId { get; set; } = new List<string>();
//        public List<Game> Games { get; } = new List<Game>();
    }
}