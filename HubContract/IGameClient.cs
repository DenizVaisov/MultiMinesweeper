using System.Threading.Tasks;
using MultiMinesweeper.Game;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.HubContract
{
    public interface IGameClient
    {
        Task PlayersConnectionIds(Player player);
        Task OwnField(GameField[][] mineField);
        Task ShowField(GameField[][] mineField);
        Task HintField(GameField[][] mineField);
        Task HideEnemyMines();
        Task EnemyField(GameField[][] mineField);
        Task PrepareRound();
        Task PlayerTurn(Player player);
        Task CompetitiveStage();
        Task Players(Player player1, Player player2);
        Task Points(int player1, int player2);
        Task Lose();
        Task GameOver(Player win, Player lose);
        Task MinesPlaced(int minesPlaced);
        Task MinesAllowed(int minesCount);
        Task Mined();
        Task Status(Player player);
        Task StopTimer();
        Task TimeIsRacing();
        Task YourTurn();
        Task Timeout();
        Task NotYourTurn();
        Task ReceiveMessage(string player, string message);
        Task Reconnect(Player player, GameField[][] ownField, GameField[][] enemyField);
        Task ToLobby();
    }
}