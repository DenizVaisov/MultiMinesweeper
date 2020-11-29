using System;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Game
{
    public class GameLogic 
    {
        public string Id { get; set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Player ReconnectedPlayer { get; set; }
        public Player CurrentPlayer { get; set; }
        private const int NumberOfRows = 16;
        public int FirstPlayerMineCounter { get; set; }
        public int SecondPlayerMineCounter { get; set; }
        public int FirstPlayerFlagCounter { get; set; }
        public int SecondPlayerFlagCounter { get; set; }
        public int FirstPlayerCellClicked { get; set; }
        public int SecondPlayerCellClicked { get; set; }
        
        public GameField[][] Field1;
        public GameField[][] Field2; 
        
        public bool Prepare { get; set; }
        public bool InProgress { get; set; }
        public GameLogic()
        {
            InitialiazeOwnField();
            InitialiazeEnemyField();
            Player1 = new Player();
            Player2 = new Player();
            CurrentPlayer = new Player();
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

        public bool HasPlayerId(long id)
        {
            if (Player1 != null && Player1.PlayerId == id)
            {
                return true;
            }
            if (Player2 != null && Player2.PlayerId == id)
            {
                return true;
            }
            return false;
        }
        
        public GameField[][] InitialiazeOwnField()
        {
            Field1 = new GameField[NumberOfRows][];
            
            for (int i = 0; i < NumberOfRows; i++)
                Field1[i] = new GameField[NumberOfRows];
            
            return Field1;
        }
        
        public GameField[][] InitialiazeEnemyField()
        {
            Field2 = new GameField[NumberOfRows][];
            
            for (int i = 0; i < NumberOfRows; i++)
                Field2[i] = new GameField[NumberOfRows];
            
            return Field2;
        }

        public GameField[][] OpenCell(int row, int cell, GameField[][] Field)
        {
            Field[row][cell].ClickedCell = true;
            
            if(!Field[row][cell].MinedCell)
                Field[row][cell].NumberCell = true;

            if (Field[row][cell].Merged)
                return Field;

            return Field;
        }
        public GameField[][] CountMines(int row, int cell, GameField[][] field)
        {
            if (row > 0 && cell == 0)
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int y = 0; y < 1; y++)
                    {
                        if (field[row][cell+1].MinedCell)
                            field[row][cell].NeighbourCells++;
                    } 
                }
            }
            else if (row > 0 && cell == NumberOfRows-1)
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int y = 0; y < 1; y++)
                    {
                        if (field[row][cell-1].MinedCell)
                            field[row][cell].NeighbourCells++;
                    } 
                }
            }
            else if (row == NumberOfRows-1 && cell > 0)
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int y = 0; y < 1; y++)
                    {
                        if (field[row-1][cell].MinedCell)
                            field[row][cell].NeighbourCells++;
                    } 
                }
            }
            else if (row == 0 && cell > 0)
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int y = 0; y < 1; y++)
                    {
                        if (field[row+1][cell].MinedCell)
                            field[row][cell].NeighbourCells++;
                    } 
                }
            }
            else if (row == NumberOfRows-1 && cell == 0)
            {
                if (field[row-1][cell+1].MinedCell)
                    field[row][cell].NeighbourCells++;
                
                if (field[row-1][cell].MinedCell)
                    field[row][cell].NeighbourCells++;
                
                if (field[row][cell+1].MinedCell)
                    field[row][cell].NeighbourCells++;
            }
            else if(row == NumberOfRows-1 && cell == NumberOfRows-1)
            {
                if (field[row-1][cell].MinedCell)
                    field[row][cell].NeighbourCells++;
                
                if (field[row-1][cell-1].MinedCell)
                    field[row][cell].NeighbourCells++;
                
                if (field[row][cell-1].MinedCell)
                    field[row][cell].NeighbourCells++;
            }
            else if (row == 0 && cell == 0)
            {
                if (field[row][cell+1].MinedCell)
                    field[row][cell].NeighbourCells++;
                
                if (field[row+1][cell].MinedCell)
                    field[row][cell].NeighbourCells++;
                
                if (field[row+1][cell+1].MinedCell)
                    field[row][cell].NeighbourCells++;
            }
            else
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        if (field[row + x][cell + y].MinedCell)
                            field[row][cell].NeighbourCells++;
                    }
                }
            }

            return field;
        }
        public GameField[][] PlaceMines(int row, int cell, GameField[][] field)
        {
            field[row][cell].MinedCell = true;
            field[row][cell].ClickedCell = true;
            
            return field;
        }
        
        public bool IsWin(int clickedCellCounter)
        {
            for (int i = 0; i < NumberOfRows; i++)
            {
                for (int j = 0; j < NumberOfRows; j++)
                {
                    if (clickedCellCounter == NumberOfRows * NumberOfRows)
                        return true;
                }
            }

            return false;
        }

        public GameField[][] PlaceFlags(int row, int cell, GameField[][] field)
        {
            field[row][cell].Merged = true;
            field[row][cell].MinedCell = false;
            field[row][cell].ClickedCell = true;

            return field;
        }
    }
}