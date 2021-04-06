using System.Collections.Generic;
using System.Threading.Tasks;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.HubContract
 {
     public interface ILobbyClient
     {
         Task ReceiveMessage(string user, string message, string time);
         Task ReceiveAllMessages(List<Chat> messages);
         Task PlayersOnline(List<string> players);
         Task PlayersInRanking(int number);
         Task ToTheGame();
     }
 }