using System.Threading.Tasks;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public interface IGameClient
    {
        Task GameField(GameField[][] mineField);
        Task MakeTurn(string player);
        Task PlayerTurn(Player player);
        Task RollCall(Player player1, Player player2);
        Task Players(Player player1, Player player2);
        Task Points(int player1, int player);
        Task Lose();
        Task HideMines();
        Task Win();
        Task Status(Player player);
        Task YourTurn();
        Task NotYourTurn();
        Task ReceiveMessage(string player, string message);
        Task Concede();
    }
}