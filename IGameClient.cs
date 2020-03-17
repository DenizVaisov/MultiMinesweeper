using System.Threading.Tasks;

namespace MultiMinesweeper
{
    public interface IGameClient
    {
        Task GenerateGameField(GameField[][] mineField);
        Task MakeTurn(string player);
        Task PlayerTurn(Player player1, Player player2);
        Task ClickedCell();

        Task Concede();
    }
}