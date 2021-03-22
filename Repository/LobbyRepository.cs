using System.Collections.Generic;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Repository
{
    public static class LobbyRepository
    {
        public static List<string> Players { get; } = new List<string>();
        public static List<Rating> ConnectedPlayersRating { get; } = new List<Rating>();
        public static List<string> PlayersToGame { get; } = new List<string>();
    }
}