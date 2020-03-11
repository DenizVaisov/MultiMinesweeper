using System.Collections.Generic;

namespace MultiMinesweeper
{
    public interface IGameRepository
    {
        List<Game> Games { get; }
    }
}