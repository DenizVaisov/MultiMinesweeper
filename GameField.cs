namespace MultiMinesweeper
{
    public struct GameField
    {
        public bool ClickedCell { get; set; }
        public int NeighbourCells { get; set; }
        public bool MinedCell { get; set; }
    }
}