using System.Collections.Generic;

namespace MultiMinesweeper
{
    public class GameRepository : IGameRepository
    {
        public List<Game> Games { get; } = new List<Game>();
    }
}