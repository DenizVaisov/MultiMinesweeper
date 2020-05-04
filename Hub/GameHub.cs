using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using MultiMinesweeper.Model;
using MultiMinesweeper.Repository;

namespace MultiMinesweeper.Hub
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    
    [Authorize] 
    public class GameHub : Hub<IGameClient>
    {
//        private IGameRepository _repository;
        private readonly Random _random;

        public static Dictionary<string, string> Players = new Dictionary<string, string>();
        
        public GameHub(Random random)
        {
            _random = random;
        }
        
        public async Task UpdateUser()
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            if (game != null)
            {
                var player = game.GetPlayer(Context.ConnectionId);
//                player.Name = Context.User.Identity.Name;
                Console.WriteLine($"Player name is {game.CurrentPlayer.Name}");
                await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
//                await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
            }
        }
        
        public async Task SendMessage(string player, string message)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            
            Console.WriteLine("Message Received");
            if (game != null)
            {
//                await Clients.Group(game.Id).PlayerTurn(game.Player1, game.Player2);
                await Clients.Group(game.Id).ReceiveMessage(Context.User.Identity.Name, message);
            }
        }
        
        public async Task CellClick(int row, int cell)
        {
            Console.WriteLine();
            Console.WriteLine($"Cell:{cell} is clicked on row:{row}");
           
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            Console.WriteLine($"Context.ConnectionId: {Context.ConnectionId}");
            
            if (game == null)
            {
                return;
            }

            Console.WriteLine($"Current player is: {Context.User.Identity.Name}");
//            game.CurrentPlayer.ConnectionId = Context.ConnectionId;
          
            //Ignore player clicking if it's not their turn
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                Console.WriteLine($"Now is not your turn {Context.User.Identity.Name}");
                return;
            }

            if (!game.InProgress) return;
            
            var placedMines = game.PlaceMines(row, cell);
            Console.WriteLine($"ClickedCell: {game.GameField[row][cell].ClickedCell}");
            Console.WriteLine($"Current player id: {game.CurrentPlayer.ConnectionId}");
            game.NextPlayer();
            Console.WriteLine($"Next player is {game.CurrentPlayer.ConnectionId}");

            await Clients.Group(game.Id.ToString()).GenerateGameField(game.GameField);
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
        }
        
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("GameHub hub connected");
            Console.WriteLine($"Player {Context.User.Identity.Name} connected");
            Console.ForegroundColor = ConsoleColor.Green;
            //Найти или создать новую игру
//            var game = _repository.Games.FirstOrDefault(g => !g.InProgress);
            var game = GameRepository.Games.FirstOrDefault(g => !g.Value.InProgress).Value;
            Console.WriteLine(GameRepository.Games.FirstOrDefault(g => !g.Value.InProgress).Value);
            if (game == null)
            {
                game = new Game {Id = Guid.NewGuid().ToString(), Player1 = {ConnectionId = Context.ConnectionId, Name = Context.User.Identity.Name}};
                Console.WriteLine($"Player1 connection id: {game.Player1.ConnectionId}");
                GameRepository.Games[game.Id] = game;
                Console.WriteLine($"GameID: {GameRepository.Games[game.Id]}");
            }
            else
            {
                game.Player2.ConnectionId = Context.ConnectionId;
                game.Player2.Name = Context.User.Identity.Name;
                Console.WriteLine($"Player2 connection id: {game.Player2.ConnectionId}");
                game.InProgress = true;
                game.CurrentPlayer.ConnectionId = game.Player1.ConnectionId;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
            await base.OnConnectedAsync();

            if (game.InProgress)
            {
                Console.WriteLine($"Game in progress: {game.InProgress}");
                await Clients.Group(game.Id.ToString()).GenerateGameField(game.GameField);
                await Clients.Group(game.Id.ToString()).Players(game.Player1, game.Player2);
            }
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.Player1.ConnectionId == Context.ConnectionId || g.Value.Player2.ConnectionId == Context.ConnectionId).Value;
            if (game != null)
            {
                Console.WriteLine($"Player1 connectionid: {game.Player1.ConnectionId}");
                Console.WriteLine($"Player2 connectionid: {game.Player2.ConnectionId}");
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);
                await Clients.Group(game.Id).Concede();
                GameRepository.Games.Remove(game.Id);
//                _repository.Games.Remove(game);
            }
            Console.WriteLine("GameHub disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}