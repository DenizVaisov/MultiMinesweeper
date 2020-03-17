namespace MultiMinesweeper
{
    public class GameSession
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }

        public GameSession()
        {
            Player1.ConnectionId = "";
            Player2.ConnectionId = "";
        }
    }
}