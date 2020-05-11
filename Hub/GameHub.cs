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
                Console.WriteLine($"Player name is {game.CurrentPlayer.Name}");
                await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            }
        }

        public async Task PrepareToBattle(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.InProgress).Value;

            if (game == null)
            {
                return;
            }
            
            if (!game.InProgress) return;
            
            await Clients.Client(game.Id).GameField(game.PlaceMines(row, cell));
            await Clients.Group(game.Id).GameField(game.OwnField);
        }

        public async Task CheckCell(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            Console.WriteLine($"Context.ConnectionId: {Context.ConnectionId}");
            
            if (game == null)
            {
                return;
            }

            if (!game.InProgress) return;

            if (game.OwnField[row][cell].MinedCell)
            {
                game.CurrentPlayer.Lifes -= 1;
                game.OwnField[row][cell].MinedCell = false;
                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
                Console.WriteLine($"{game.CurrentPlayer.Name} lifes: {game.CurrentPlayer.Lifes}");
                await Clients.Client(Context.ConnectionId).Status(game.CurrentPlayer);
                if (game.CurrentPlayer.Lifes == 0)
                {
                    await Clients.Client(Context.ConnectionId).Lose();
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);
                    await Clients.Group(game.Id).Win();
                    GameRepository.Games.Remove(game.Id);
                }
            }
            else
            {
                if (game.OwnField[row][cell].Merged)
                {
                    return;
                }
                game.CurrentPlayer.Points += 50;
                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
            }
            await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
            await Clients.Group(game.Id).GameField(game.OwnField);
        }

        public async Task OpenCell(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            Console.WriteLine($"Context.ConnectionId: {Context.ConnectionId}");
            
            if (game == null)
            {
                return;
            }
//            //Ignore player clicking if it's not their turn
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
//                Console.WriteLine($"Now is not your turn {Context.User.Identity.Name}");
                return;
            }

            if (!game.InProgress) return;

            await Clients.Group(game.Id).GameField(game.OpenCell(row, cell));
            await CheckCell(row, cell);
            await Clients.Group(game.Id).GameField(game.CountMines(row, cell));
            game.NextPlayer();
            await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
//            await Clients.Group(game.Id).GameField(game.ClearNeighbour(row, cell));
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
        }

        public async Task PlaceMine(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            Console.WriteLine($"Context.ConnectionId: {Context.ConnectionId}");
            
            if (game == null)
            {
                return;
            }
            //Ignore player clicking if it's not their turn
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
//                Console.WriteLine($"Now is not your turn {Context.User.Identity.Name}");
                return;
            }

            if (!game.InProgress) return;
            game.NextPlayer();
            
            await Clients.Group(game.Id).GameField(game.PlaceMines(row, cell));
            await Clients.Group(game.Id).GameField(game.OwnField);
            await Clients.Group(game.Id).HideMines();
            await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
        }

        public async Task PlaceFlag(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            Console.WriteLine($"Context.ConnectionId: {Context.ConnectionId}");
            
            if (game == null)
            {
                return;
            }
            //Ignore player clicking if it's not their turn
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
                Console.WriteLine($"Now is not your turn {Context.User.Identity.Name}");
                return;
            }

            if (!game.InProgress) return;
            game.NextPlayer();

            await Clients.Group(game.Id).GameField(game.PlaceFlags(row, cell));
            await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
        }
        
        public async Task SendMessage(string player, string message)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            
            Console.WriteLine("Message Received");
            if (game != null)
            {
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
            //Ignore player clicking if it's not their turn
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
                Console.WriteLine($"Now is not your turn {Context.User.Identity.Name}");
                return;
            }

            if (!game.InProgress) return;
            
            var placedMines = game.PlaceMines(row, cell);
            Console.WriteLine($"ClickedCell: {game.OwnField[row][cell].ClickedCell}");
//            Console.WriteLine($"Current player id: {game.CurrentPlayer.ConnectionId}");
            game.NextPlayer();
//            Console.WriteLine($"Next player is {game.CurrentPlayer.ConnectionId}");
            Console.WriteLine($"GameID: {game.Id}");
            await Clients.Group(game.Id.ToString()).GameField(game.OwnField);
            await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
        }
        
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("GameHub hub connected");
            Console.WriteLine($"Player {Context.User.Identity.Name} connected");

            //Найти или создать новую игру
//            var game = _repository.Games.FirstOrDefault(g => !g.InProgress);
            var game = GameRepository.Games.FirstOrDefault(g => !g.Value.InProgress).Value;
            if (game == null)
            {
                game = new Game {Id = Guid.NewGuid().ToString(), Player1 = {ConnectionId = Context.ConnectionId, Name = Context.User.Identity.Name, Lifes = 3, Points = 0}};
                GameRepository.Games[game.Id] = game;
                Console.WriteLine($"GameID: {GameRepository.Games[game.Id]}");
            }
            else
            {
                game.Player2.ConnectionId = Context.ConnectionId;
                game.Player2.Name = Context.User.Identity.Name;
                game.Player2.Lifes = 3;
                game.Player2.Points = 0;
                game.InProgress = true;
                Console.WriteLine($"Game inProgress: {game.InProgress}");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
//            await base.OnConnectedAsync();

            if (game.InProgress)
            {
                await Clients.Group(game.Id).GameField(game.OwnField);
//                await Clients.Group(game.Id).GameField(game.GameField);
                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
                Console.WriteLine("Prepare round: 30 sec");
                Thread.Sleep(30000);
                Console.WriteLine("GO");
                Console.WriteLine($"Game in progress: {game.InProgress}");
                game.NextPlayer();
                await Clients.Group(game.Id).Status(game.CurrentPlayer);
                await Clients.Group(game.Id).GameField(game.OwnField);
//                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
                await Clients.Clients(game.CurrentPlayer.ConnectionId).YourTurn();
                await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
                await Clients.Group(game.Id).Players(game.Player1, game.Player2);
            }
            await base.OnConnectedAsync();
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
            }
            Console.WriteLine("GameHub disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}