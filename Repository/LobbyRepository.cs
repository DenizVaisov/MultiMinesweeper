using System.Collections.Generic;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Repository
{
    public static class LobbyRepository
    {
        public static List<string> Players { get; } = new List<string>();
        public static List<Rating> ConnectedPlayersRating { get; } = new List<Rating>();
        public static List<Chat> ChatMessages { get; } = new List<Chat>();
        public static Dictionary<string, string> PlayersToGame { get; } = new Dictionary<string, string>();
    }
}