using System.Collections.Generic;
using System.Threading.Tasks;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public interface ILobbyClient
    {
        Task ReceiveMessage(string user, string message);
        Task PlayersOnline(Queue<string> players);

        Task ToTheGame();
    }
}