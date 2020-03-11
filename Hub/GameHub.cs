using System.Linq;

namespace MultiMinesweeper.Hub
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    
    public class GameHub : Hub<IGameClient>
    {
        private IGameRepository _repository;
        private readonly Random _random;

        public GameHub()
        {
            
        }
        
//        public GameHub(IGameRepository repository, Random random)
//        {
//            _repository = repository;
//            _random = random;
//        }
        
        public async Task UpdateUser(string email, string name)
        {
            var game = _repository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
            if (game != null)
            {
                var player = game.GetPlayer(Context.ConnectionId);
                player.Email = email;
                player.Name = name;
                await Clients.Group(game.Id).PlayerTurn(game.Player1, game.Player2);
            }
        }
        
        public async Task CellClick(int row, int cell)
        {
            Console.WriteLine("Cell is clicked " + cell);
            await Clients.All.ClickedCell();
//            var game = _repository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
//            if (game is null)
//            {
//                return;
//            }
//
//            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
//            {
//                return;
//            }
//
//            if (!game.InProgress) return;

//            await Clients.Group(game.Id.ToString()).GenerateGameField(game.GameField);
        }
        
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("GameHub hub connected");
            //Найти или создать новую игру
//            var game = _repository.Games.FirstOrDefault(g => !g.InProgress);
//            if (game is null)
//            {
//                game = new Game {Id = Guid.NewGuid().ToString(), Player1 = {ConnectionId = Context.ConnectionId}};
//                _repository.Games.Add(game);
//            }
//            else
//            {
//                game.Player2.ConnectionId = Context.ConnectionId;
//                game.InProgress = true;
//            }
//
//            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
            await base.OnConnectedAsync();

//            if (game.InProgress)
//            {
//                await Clients.Group(game.Id.ToString()).GenerateGameField(game.GameField);
//            }
        }
        
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("GameHub disconnected");
            return base.OnDisconnectedAsync(exception);
        }
    }
}