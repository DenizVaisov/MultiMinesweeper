using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using MultiMinesweeper.Model;
using MultiMinesweeper.Repository;
using ServiceStack;

namespace MultiMinesweeper.Hub
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    
    [Authorize] 
    public class GameHub : Hub<IGameClient>
    {
        private readonly IHubContext<GameHub> _hubContext;
        private const int mineCount = 24;

        public GameHub(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task TimeIsUp()
        {
            Console.WriteLine("Time is up");
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            await Clients.Client(game.CurrentPlayer.ConnectionId).StopTimer();
            game.NextPlayer();
            await Clients.Client(game.CurrentPlayer.ConnectionId).TimeIsRacing();
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
        }

        public async Task CheckTime()
        {
            Console.WriteLine("Invoke CheckTime");
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            await TimeIsUp();

            if (game.Prepare == false) return;
            
            await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
            await Clients.Group(game.Id).Status(game.CurrentPlayer);
            game.Prepare = false;
            await Clients.Client(game.Player1.ConnectionId).ShowField(game.Field1);
            await Clients.Client(game.Player2.ConnectionId).ShowField(game.Field2);
            await Clients.Client(game.Player1.ConnectionId).EnemyField(game.Field2);
            await Clients.Client(game.Player1.ConnectionId).HideEnemyMines();
            await Clients.Client(game.Player2.ConnectionId).EnemyField(game.Field1);
            await Clients.Client(game.Player2.ConnectionId).HideEnemyMines();
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
            await Clients.Group(game.Id).Players(game.Player1, game.Player2);
        }

        public async Task PrepareToBattle(int row, int cell)
        {
            Console.WriteLine(Context.ConnectionId);
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
           
            if (game == null) return;
            
            if (game.Prepare)
            {
                if (game.Player1.ConnectionId == Context.ConnectionId)
                {
                    if (game.Field1[row][cell].ClickedCell) return;
                    if (game.FirstPlayerMineCounter == mineCount) return;
                    
                    game.FirstPlayerMineCounter++;
                    await Clients.Client(game.Player1.ConnectionId).MinesPlaced(game.FirstPlayerMineCounter);
                    await Clients.Client(game.Player1.ConnectionId).OwnField(game.Field1);
                    await Clients.Client(game.Player1.ConnectionId).OwnField(game.PlaceMines(row, cell, game.Field1));
                }

                else
                {
                    if (game.Field2[row][cell].ClickedCell) return;
                    if (game.SecondPlayerMineCounter == mineCount) return;
                    
                    game.SecondPlayerMineCounter++;
                    await Clients.Client(game.Player2.ConnectionId).MinesPlaced(game.SecondPlayerMineCounter);
                    await Clients.Client(game.Player2.ConnectionId).OwnField(game.Field2);
                    await Clients.Client(game.Player2.ConnectionId).OwnField(game.PlaceMines(row, cell, game.Field2));
                }
            }
        }

        public async Task CheckCell(int row, int cell, GameField[][] field)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            
            if (game == null) return;
            if (game.Prepare) return;
            if (!game.InProgress) return;

            if (field[row][cell].MinedCell)
            {
                field[row][cell].Kaboom = true;
                game.CurrentPlayer.Lifes -= 1;
                game.CurrentPlayer.Points -= 1;
                field[row][cell].MinedCell = false;
                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
                await Clients.Client(Context.ConnectionId).Status(game.CurrentPlayer);
                if (game.CurrentPlayer.Lifes == 0)
                {
                    await Clients.Client(Context.ConnectionId).Lose();
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);
                    if (game.CurrentPlayer.Name == game.Player1.Name)
                    {
                         await Clients.Group(game.Id).GameOver(game.Player2, game.CurrentPlayer);
                         GameRepository.Games.Remove(game.Id);
                    }
                    else
                    {
                        await Clients.Group(game.Id).GameOver(game.Player1, game.Player2);
                        GameRepository.Games.Remove(game.Id);
                    }
                }
            }
            else
            {
                if (field[row][cell].Merged) return;
                
                game.CurrentPlayer.Points += 1;
                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
            }
            await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
        }

        public async Task OpenCell(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            Console.WriteLine($"Context.ConnectionId: {Context.ConnectionId}");
            
            if (game == null)
                return;
            
            if(game.Prepare) return;
            
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
                return;
            }

            if (!game.InProgress) return;

            if (game.Player1 == game.CurrentPlayer)
            {
                await CheckCell(row, cell, game.Field2);
                await Clients.Client(game.Player1.ConnectionId).EnemyField(game.OpenCell(row, cell, game.Field2));
                await Clients.Client(game.Player1.ConnectionId).EnemyField(game.CountMines(row, cell, game.Field2));
                await Clients.Client(game.Player2.ConnectionId).HintField(game.OpenCell(row, cell, game.Field2));
                if (game.IsWin(game.Field2))
                    await Clients.Client(game.Player1.ConnectionId).Win();
            }

            else
            {
                await CheckCell(row, cell, game.Field1);
                await Clients.Client(game.Player2.ConnectionId).EnemyField(game.OpenCell(row, cell, game.Field1));
                await Clients.Client(game.Player2.ConnectionId).EnemyField(game.CountMines(row, cell, game.Field1));
                await Clients.Client(game.Player1.ConnectionId).HintField(game.OpenCell(row, cell, game.Field1));
                if (game.IsWin(game.Field1))
                    await Clients.Client(game.Player2.ConnectionId).Win();
            }

            await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
            await Clients.Client(game.CurrentPlayer.ConnectionId).StopTimer();
            game.NextPlayer();
            await Clients.Client(game.CurrentPlayer.ConnectionId).TimeIsRacing();
            await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
        }

        public async Task PlaceFlag(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            Console.WriteLine($"Context.ConnectionId: {Context.ConnectionId}");
            
            if (game == null) return;
            
            if(game.Prepare) return;

            
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
                Console.WriteLine($"Now is not your turn {Context.User.Identity.Name}");
                return;
            }

            if (!game.InProgress) return;
            

            if (game.Player1.ConnectionId == game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(game.CurrentPlayer.ConnectionId).EnemyField(game.PlaceFlags(row, cell, game.Field2));
            }

            else
            {
                await Clients.Client(game.CurrentPlayer.ConnectionId).EnemyField(game.PlaceFlags(row, cell, game.Field1));
            }
            
            game.NextPlayer();

            await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
        }
        public async Task SendMessage(string message)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            
            Console.WriteLine("Message Received");
            if (game != null)
            {
                Console.WriteLine(Context.ConnectionId);
                await Clients.Group(game.Id).ReceiveMessage(Context.User.Identity.Name, message);
            }
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("GameHub hub connected");
            Console.WriteLine($"Player {Context.User.Identity.Name} connected");

            var game = GameRepository.Games.FirstOrDefault(g => !g.Value.InProgress).Value;
            
            if (game == null)
            {
                game = new Game
                {
                    Id = Guid.NewGuid().ToString(), 
                    Player1 = {
                        ConnectionId = Context.ConnectionId, 
                        Name = Context.User.Identity.Name, 
                        Lifes = 3, 
                        Points = 0
                    }
                };
                GameRepository.Games[game.Id] = game;
            }
           
            else
            {
                game.Player2.ConnectionId = Context.ConnectionId;
                game.Player2.Name = Context.User.Identity.Name;
                game.Player2.Lifes = 3;
                game.Player2.Points = 0;
                game.InProgress = true;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

            if (game.InProgress)
            {
                Console.WriteLine($"Player1 ID: {game.Player1.ConnectionId}");
                Console.WriteLine($"Player2 ID: {game.Player2.ConnectionId}");
                await Clients.Group(game.Id).PrepareRound();
                await Clients.Group(game.Id).Timeout();
                await Clients.Client(game.Player1.ConnectionId).OwnField(game.Field1);
                await Clients.Client(game.Player2.ConnectionId).OwnField(game.Field2);
                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
                game.Prepare = true;
                game.NextPlayer();
                Console.WriteLine("Prepare round: 20 sec");
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
                GameRepository.Games.Remove(game.Id);
            }
            Console.WriteLine("GameHub disconnected\n");
            await base.OnDisconnectedAsync(exception);
        }
    }
}