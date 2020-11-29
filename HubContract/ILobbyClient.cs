using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiMinesweeper.HubContract
 {
     public interface ILobbyClient
     {
         Task ReceiveMessage(string user, string message);
         Task PlayersOnline(Queue<string> players);
         Task ToTheGame();
     }
 }