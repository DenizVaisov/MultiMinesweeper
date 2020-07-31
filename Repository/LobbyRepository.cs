using System.Collections.Generic;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Repository
{
    public static class LobbyRepository
    {
        public static Queue<string> Players { get; set; } = new Queue<string>();
        public static List<Rating> ConnectedPlayersRating { get; set; } = new List<Rating>();
        public static List<string> PlayersConnectionId { get; set; } = new List<string>();
    }
}