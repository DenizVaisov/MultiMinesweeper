using System.Threading.Tasks;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public interface IGameClient
    {
        Task GameFields(GameField[][] mineField1, GameField[][] mineField2);
        Task OwnField(GameField[][] mineField1);
        Task ShowField(GameField[][] mineField1);
        Task HintField(GameField[][] mineField);
        Task HideEnemyMines();
        Task EnemyField(GameField[][] mineField);
        Task PrepareRound();
        Task PlayerTurn(Player player);
        Task RollCall(Player player1, Player player2);
        Task Players(Player player1, Player player2);
        Task Points(int player1, int player);
        Task Lose();
        Task Win(Player winner, Player loser);
        Task Status(Player player);
        Task YourTurn();
        Task Timeout();
        Task NotYourTurn();
        Task ReceiveMessage(string player, string message);
        Task Concede();
    }
}