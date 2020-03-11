using System;
using System.Collections.Generic;

namespace MultiMinesweeper
{
    public class Game
    {
        public string Id { get; set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        
        public Player CurrentPlayer { get; set; }
        
        public const int NumberOfRows = 16;
        
        public const int NumberOfColumns = 16;
        
        public GameField[][] GameField;
        
        public const string EmptyCell = "white";
        public bool InProgress { get; set; }
        public Game()
        {
            Player1 = new Player();
            Player2 = new Player();
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
            
            for (int x = 0; x < NumberOfRows; x++)
            {
                for (int y = 0; y < NumberOfColumns; y++)
                {
                    GameField[x][y].Cell = EmptyCell;
                    GameField[x][y].ClickedCell = true;
                }
            }
            return GameField;
        }
    }
}