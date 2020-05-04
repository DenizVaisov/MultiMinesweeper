using System.Threading.Tasks;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public interface IGameClient
    {
        Task GenerateGameField(GameField[][] mineField);
        Task MakeTurn(string player);
        Task PlayerTurn(Player player);
        
        Task RollCall(Player player1, Player player2);
        Task Players(Player player1, Player player2);

        Task ReceiveMessage(string player, string message);
        
        Task Concede();
    }
}