using System.Collections.Generic;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Repository
{
    public static class LobbyRepository
    {
        public static Queue<string> Players { get; } = new Queue<string>();
        public static List<Rating> ConnectedPlayersRating { get; } = new List<Rating>();
        public static List<string> PlayersConnectionId { get; } = new List<string>();
    }
}