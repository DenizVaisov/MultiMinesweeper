﻿using System.Threading.Tasks;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public interface IGameClient
    {
        Task OwnField(GameField[][] mineField);
        Task ShowField(GameField[][] mineField);
        Task HintField(GameField[][] mineField);
        Task HideEnemyMines();
        Task EnemyField(GameField[][] mineField);
        Task PrepareRound();
        Task PlayerTurn(Player player);
        Task RollCall(Player player1, Player player2);
        Task Players(Player player1, Player player2);
        Task Points(int player1, int player);
        Task Lose();
        Task GameOver(Player win, Player lose);
        Task MinesPlaced(int minesPlaced);
        Task MinesAllowed(int minesCount);
        Task Status(Player player);
        Task TimeIsUp();
        Task Win();
        Task StopTimer();
        Task TimeIsRacing();
        Task YourTurn();
        Task Timeout();
        Task NotYourTurn();
        Task ReceiveMessage(string player, string message);
        Task Concede();
    }
}