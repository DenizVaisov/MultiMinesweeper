using System;
using System.Collections.Generic;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public class Game
    {
        public string Id { get; set; }
        
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        
        public Player CurrentPlayer { get; set; }
        
        public const int NumberOfRows = 16;
        public List<GameField[][]> Cells { get; set; }

        public const int NumberOfColumns = 16;
        
        public GameField[][] GameField;
        
        public const string EmptyCell = "white";
        public bool InProgress { get; set; }
        public Game()
        {
            InitialiazeGameField();
            Player1 = new Player();
            Player2 = new Player();
            CurrentPlayer = new Player();
        }

        public Player GetPlayer(string connectionId)
        {
            if (Player1 != null && Player1.ConnectionId == connectionId)
            {
                return Player1;
            }
            if (Player2 != null && Player2.ConnectionId == connectionId)
            {
                return Player2;
            }
            return null;
        }
        
        public void NextPlayer()
        {
            if (CurrentPlayer == Player1)
            {
                CurrentPlayer = Player2;
                Console.WriteLine($"Current player is {CurrentPlayer}");
            }
            else
            {
                CurrentPlayer = Player1;
                Console.WriteLine($"Current player is {CurrentPlayer}");
            }
        }
        
        public bool HasPlayer(string connectionId)
        {
            if (Player1 != null && Player1.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player2 != null && Player2.ConnectionId == connectionId)
            {
                return true;
            }
            return false;
        }
        
        public GameField[][] InitialiazeGameField()
        {
            GameField = new GameField[NumberOfRows][];
            
            for (int i = 0; i < NumberOfRows; i++)
                GameField[i] = new GameField[NumberOfRows];
            
            return GameField;
        }

        public GameField[][] PlaceMines(int row, int cell)
        {
            GameField[row][cell].MinedCell = true;
            GameField[row][cell].ClickedCell = true;
            
            return GameField;
        }
    }
}