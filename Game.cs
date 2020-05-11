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
        public Player Player { get; set; }
        public Player CurrentPlayer { get; set; }
        public const int NumberOfRows = 16;
        
        public GameField[][] OwnField;
        public GameField[][] EnemyField;
        
        public bool InProgress { get; set; }
        public Game()
        {
            InitialiazeOwnField();
            InitialiazeEnemyField();
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
                Console.WriteLine($"Current player is {CurrentPlayer.Name}");
            }
            else
            {
                CurrentPlayer = Player1;
                Console.WriteLine($"Current player is {CurrentPlayer.Name}");
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
        
        public GameField[][] InitialiazeOwnField()
        {
            OwnField = new GameField[NumberOfRows][];
            
            for (int i = 0; i < NumberOfRows; i++)
                OwnField[i] = new GameField[NumberOfRows];
            
            return OwnField;
        }
        
        public GameField[][] InitialiazeEnemyField()
        {
            EnemyField = new GameField[NumberOfRows][];
            
            for (int i = 0; i < NumberOfRows; i++)
                OwnField[i] = new GameField[NumberOfRows];
            
            return EnemyField;
        }

        public GameField[][] OpenCell(int row, int cell)
        {
            OwnField[row][cell].ClickedCell = true;

            if (OwnField[row][cell].Merged)
            {
                return OwnField;
            }

            return OwnField;
        }

        public GameField[][] CountMines(int row, int cell)
        {
            if (row == 0 && cell != 0)
            {
                for (int x = 0; x < 1; x++)
                {
                    for (int y = 0; y < 1; y++)
                    {
                        if (OwnField[row + x][cell + y].MinedCell)
                            OwnField[row][cell].NeighbourCells++;
                    }
                }
            }
            else if (row == 0 && cell == 0)
            {
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        if (OwnField[row][cell].MinedCell)
                            OwnField[row+x][cell].NeighbourCells++;
                    }
                }
            }
            else if (row == 0 && cell == NumberOfRows)
            {
                if (OwnField[row][cell].MinedCell)
                {
                    OwnField[row][cell-1].NeighbourCells++;
                    OwnField[row+1][cell].NeighbourCells++;
                    OwnField[row+1][cell-1].NeighbourCells++;
                }
//                for (int x = 0; x < 1; x++)
//                {
//                    for (int y = -2; y < 0; y++)
//                    {
//                        if (GameField[row + x][cell + y].MinedCell)
//                            GameField[row][cell].NeighbourCells++;
//                    }
//                }
            }
            else
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        if (OwnField[row + x][cell + y].MinedCell)
                            OwnField[row][cell].NeighbourCells++;
                    }
                }
            }

            return OwnField;
        }

        public GameField[][] ClearNeighbour(int row, int cell)
        {
            OwnField[row][cell].NeighbourCells = 0;

            return OwnField;
        }

        public GameField[][] PlaceMines(int row, int cell)
        {
            OwnField[row][cell].MinedCell = true;
            OwnField[row][cell].ClickedCell = true;
            
            return OwnField;
        }

        public GameField[][] PlaceFlags(int row, int cell)
        {
            OwnField[row][cell].Merged = true;
            OwnField[row][cell].MinedCell = false;
            OwnField[row][cell].ClickedCell = true;

            return OwnField;
        }
    }
}