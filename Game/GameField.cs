﻿namespace MultiMinesweeper.Game
{
    public struct GameField
    {
        public bool ClickedCell { get; set; }
        public bool Merged { get; set; }
        public int NeighbourCells { get; set; }
        public bool MinedCell { get; set; }
        public bool Kaboom { get; set; }
        public bool NumberCell { get; set; }
    }
}