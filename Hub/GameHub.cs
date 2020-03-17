using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MultiMinesweeper.Controller;

namespace MultiMinesweeper.Hub
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    
    public class GameHub : Hub<IGameClient>
    {
        private IGameRepository _repository;
        private readonly Random _random;

        public static Dictionary<string, string> Players = new Dictionary<string, string>();
        
        public GameHub(IGameRepository repository, Random random)
        {
            _repository = repository;
            _random = random;
        }
        
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
            Console.WriteLine($"Player {Context.ConnectionId} clicked cell");
            Console.WriteLine($"Cell:{cell} is clicked on row:{row}");
           
            var game = _repository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
            Console.WriteLine($"Context.ConnectionId {Context.ConnectionId}");
            
            if (game is null)
            {
                return;
            }

            game.CurrentPlayer.ConnectionId = Context.ConnectionId;
            Console.WriteLine($"Current player is {game.CurrentPlayer.ConnectionId}");
          
            game.GetPlayer(game.CurrentPlayer.ConnectionId);
          
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                return;
            }

            if (!game.InProgress) return;
            
            var placedMines = game.PlaceMines(row, cell);
            Console.WriteLine(game.GameField[row][cell].ClickedCell);
            
            Console.WriteLine(game.CurrentPlayer.ConnectionId);
            await Clients.Group(game.Id.ToString()).GenerateGameField(game.GameField);
            game.NextPlayer();

            await Clients.Group(game.Id).PlayerTurn(game.Player1, game.Player2);
        }
        
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("GameHub hub connected");
            Console.WriteLine($"Player {Context.ConnectionId} connected");
            Console.ForegroundColor = ConsoleColor.Green;
            //Найти или создать новую игру
            var game = _repository.Games.FirstOrDefault(g => !g.InProgress);
            if (game is null)
            {
                game = new Game {Id = Guid.NewGuid().ToString(), Player1 = {ConnectionId = Context.ConnectionId}};
                Console.WriteLine($"Player1 connection id: {game.Player1.ConnectionId}");
                _repository.Games.Add(game);
                Console.WriteLine($"Game progress: {game.InProgress}");
            }
            else
            {
                game.Player2.ConnectionId = Context.ConnectionId;
                Console.WriteLine($"Player2 connection id: {game.Player2.ConnectionId}");
                game.InProgress = true;
                Console.WriteLine($"Game progress: {game.InProgress}");
            }

            Console.WriteLine($"Current connection items: {Context.Items}");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
            await base.OnConnectedAsync();

            if (game.InProgress)
            {
                Console.WriteLine($"Game in progress: {game.InProgress}");
                await Clients.Group(game.Id.ToString()).GenerateGameField(game.GameField);
            }
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var game = _repository.Games.FirstOrDefault(g => g.Player1.ConnectionId == Context.ConnectionId || g.Player2.ConnectionId == Context.ConnectionId);
            if (!(game is null))
            {
                Console.WriteLine($"Player1 connectionid: {game.Player1.ConnectionId}");
                Console.WriteLine($"Player2 connectionid: {game.Player2.ConnectionId}");
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);
                await Clients.Group(game.Id).Concede();
                _repository.Games.Remove(game);
            }
            Console.WriteLine("GameHub disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}