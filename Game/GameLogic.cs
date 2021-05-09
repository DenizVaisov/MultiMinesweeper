using System;
using MultiMinesweeper.Model;

namespace MultiMinesweeper.Game
{
    public class GameLogic 
    {
        public string Id { get; set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Player CurrentPlayer { get; set; }
        
        public string FirstPlayerName { get; set; }
        public string SecondPlayerName { get; set; }
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
        
        public bool HasPlayer(string connectionIdOrName)
        {
            if (Player1 != null && (Player1.ConnectionId == connectionIdOrName || Player1.Name == connectionIdOrName))
                return true;
            
            if (Player2 != null && (Player2.ConnectionId == connectionIdOrName || Player2.Name == connectionIdOrName))
                return true;
            
            return false;
        }

        public GameField[][] InitialiazeOwnField()
        {
            Field1 = new GameField[(int)GameSettings.NumberOfRows][];
            
            for (int i = 0; i < (int)GameSettings.NumberOfRows; i++)
                Field1[i] = new GameField[(int)GameSettings.NumberOfRows];
            
            return Field1;
        }
        
        public GameField[][] InitialiazeEnemyField()
        {
            Field2 = new GameField[(int)GameSettings.NumberOfRows][];
            
            for (int i = 0; i < (int)GameSettings.NumberOfRows; i++)
                Field2[i] = new GameField[(int)GameSettings.NumberOfRows];
            
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
            for (int x = row - 1; x <= row + 1; x++) {
                for (int y = cell - 1; y <= cell + 1; y++) {
                    if(x >= 0 && x < (int)GameSettings.NumberOfRows && y >= 0 && y < (int)GameSettings.NumberOfRows) {
                        if (field[x][y].MinedCell)
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
            for (int i = 0; i < (int)GameSettings.NumberOfRows; i++)
            {
                for (int j = 0; j < (int)GameSettings.NumberOfRows; j++)
                {
                    if (clickedCellCounter == (int)GameSettings.NumberOfRows >> 2)
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