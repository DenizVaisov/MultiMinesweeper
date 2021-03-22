using System.Linq;
using Microsoft.AspNetCore.Authorization;
using MultiMinesweeper.Game;
using MultiMinesweeper.HubContract;
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
        private readonly IHubContext<GameHub> _hubContext;
        private readonly RepositoryContext _context;
        private long playerID;

        public GameHub(IHubContext<GameHub> hubContext, RepositoryContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task TimeIsUp()
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            await Clients.Client(game.CurrentPlayer.ConnectionId).StopTimer();
            game.NextPlayer();
            Console.WriteLine($"Time is racing {game.CurrentPlayer.Name}");
            await Clients.Client(game.CurrentPlayer.ConnectionId).TimeIsRacing();
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
        }

        public async Task CheckTime()
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;

            if (!game.Prepare)
            {
                await TimeIsUp();
                return;
            }

            game.Prepare = false;
            
            await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
            await Clients.Group(game.Id).Status(game.Player1);
            await Clients.Group(game.Id).Status(game.Player2);
            
            await Clients.Client(game.Player1.ConnectionId).ShowField(game.Field1);
            await Clients.Client(game.Player2.ConnectionId).ShowField(game.Field2);
            
            await Clients.Client(game.Player1.ConnectionId).EnemyField(game.Field2);
            await Clients.Client(game.Player1.ConnectionId).HideEnemyMines();
            
            await Clients.Client(game.Player2.ConnectionId).EnemyField(game.Field1);
            await Clients.Client(game.Player2.ConnectionId).HideEnemyMines();
            
            game.CurrentPlayer = game.Player2;
//            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
            await Clients.Group(game.Id).CompetitiveStage();
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
                    if (game.Field1[row][cell].NumberCell) return;
                    if (game.FirstPlayerMineCounter == (int)GameSettings.MinesCount) return;
                    
                    game.FirstPlayerMineCounter++;
                    game.FirstPlayerCellClicked++;
                    await Clients.Client(game.Player1.ConnectionId).MinesPlaced(game.FirstPlayerMineCounter);
                    await Clients.Client(game.Player1.ConnectionId).OwnField(game.Field1);
                    await Clients.Client(game.Player1.ConnectionId).OwnField(game.PlaceMines(row, cell, game.Field1));
                }
                else
                {
                    if (game.Field2[row][cell].NumberCell) return;
                    if (game.SecondPlayerMineCounter == (int)GameSettings.MinesCount) return;
                    
                    game.SecondPlayerMineCounter++;
                    game.SecondPlayerCellClicked++;
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
            if (field[row][cell].NumberCell) return;
            if (field[row][cell].Merged) return;
            
            if (field[row][cell].MinedCell)
            {
                field[row][cell].Kaboom = true;
                field[row][cell].MinedCell = false;
                game.CurrentPlayer.Lifes -= 1;
                
                await Clients.Client(Context.ConnectionId).Mined();
                await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
                await Clients.Client(Context.ConnectionId).Status(game.CurrentPlayer);
                if (game.CurrentPlayer.Lifes == 0)
                {
                    await Clients.Client(Context.ConnectionId).Lose();
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
            
            if (game == null) return;
            if (!game.InProgress) return;
            if(game.Prepare) return;
            
            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
                return;
            }

            if (game.Player1 == game.CurrentPlayer)
            {
                if(game.Field2[row][cell].NumberCell) return;
                game.FirstPlayerCellClicked++;
                
                await CheckCell(row, cell, game.Field2);
                await Clients.Client(game.Player1.ConnectionId).EnemyField(game.OpenCell(row, cell, game.Field2));
                await Clients.Client(game.Player1.ConnectionId).EnemyField(game.CountMines(row, cell, game.Field2));
                await Clients.Client(game.Player2.ConnectionId).HintField(game.OpenCell(row, cell, game.Field2));

                if (game.IsWin(game.FirstPlayerCellClicked))
                {
                    Console.WriteLine("Game Over");
                    await Clients.Client(game.Player1.ConnectionId).GameOver(game.Player1, game.Player2);
                    GameRepository.Games.Remove(game.Id);
                }
            }

            else
            {
                if(game.Field1[row][cell].NumberCell) return;
                game.SecondPlayerCellClicked++;
                
                await CheckCell(row, cell, game.Field1);
                await Clients.Client(game.Player2.ConnectionId).EnemyField(game.OpenCell(row, cell, game.Field1));
                await Clients.Client(game.Player2.ConnectionId).EnemyField(game.CountMines(row, cell, game.Field1));
                await Clients.Client(game.Player1.ConnectionId).HintField(game.OpenCell(row, cell, game.Field1));

                if (game.IsWin(game.SecondPlayerCellClicked))
                {
                    Console.WriteLine("Game Over");
                    await Clients.Client(game.Player2.ConnectionId).GameOver(game.Player2, game.Player1);
                    GameRepository.Games.Remove(game.Id);
                }
            }

            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
            await Clients.Client(game.CurrentPlayer.ConnectionId).StopTimer();
            game.NextPlayer();
            await Clients.Client(game.CurrentPlayer.ConnectionId).TimeIsRacing();
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
        }

        public async Task PlaceFlag(int row, int cell)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            
            if (game == null) return;
            if (!game.InProgress) return;
            if(game.Prepare) return;

            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                await Clients.Client(Context.ConnectionId).NotYourTurn();
                Console.WriteLine($"Now is not your turn {Context.User.Identity.Name}");
                return;
            }

            if (game.Player1.ConnectionId == game.CurrentPlayer.ConnectionId)
            {
                if (game.Field1[row][cell].Merged) return;
                if (game.Field2[row][cell].NumberCell) return;
                if (game.FirstPlayerFlagCounter == (int)GameSettings.MinesCount) return;
                if (game.Field2[row][cell].MinedCell)
                    game.Field2[row][cell].MinedCell = false;
                
                game.FirstPlayerFlagCounter++;
                await Clients.Client(game.CurrentPlayer.ConnectionId).EnemyField(game.PlaceFlags(row, cell, game.Field2));
            }

            else
            {
                if (game.Field2[row][cell].Merged) return;
                if (game.Field1[row][cell].NumberCell) return;
                if (game.SecondPlayerFlagCounter == (int)GameSettings.MinesCount) return;
                if (game.Field1[row][cell].MinedCell)
                    game.Field1[row][cell].MinedCell = false;
                    
                game.SecondPlayerFlagCounter++;
                await Clients.Client(game.CurrentPlayer.ConnectionId).EnemyField(game.PlaceFlags(row, cell, game.Field1));
            }
            await Clients.Client(game.CurrentPlayer.ConnectionId).StopTimer();
            game.NextPlayer();
            await Clients.Client(game.CurrentPlayer.ConnectionId).TimeIsRacing();
            await Clients.Client(game.CurrentPlayer.ConnectionId).YourTurn();
            await Clients.Group(game.Id).PlayerTurn(game.CurrentPlayer);
        }
        public async Task SendMessage(string message)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayer(Context.ConnectionId)).Value;
            if (game != null)
                await Clients.Group(game.Id).ReceiveMessage(Context.User.Identity.Name, message);
        }

        public async Task Reconnect()
        {
            var query = _context.Users.Where(p => p.Login == Context.User.Identity.Name);
            foreach (var item in query)
                playerID = item.Id;

            Console.WriteLine(playerID);
            var existedGame = GameRepository.Games.FirstOrDefault(g => g.Value.HasPlayerId(playerID)).Value;
            
            if (existedGame != null && existedGame.Player1.PlayerId == playerID)
            {
                Console.WriteLine(existedGame.Player1.Name + "    " + existedGame.Player2.Name);
                await Clients.Client(Context.ConnectionId).Reconnect(existedGame.Player1, existedGame.Field1, existedGame.Field2);
                await Clients.Group(existedGame.Id).Points(existedGame.Player1.Points, existedGame.Player2.Points);
                await CheckTime();
            }
            else if (existedGame != null)
            {
                Console.WriteLine(existedGame.Player1.Name + "    " + existedGame.Player2.Name);
                await Clients.Client(Context.ConnectionId).Reconnect(existedGame.Player2, existedGame.Field2, existedGame.Field1);
                await Clients.Group(existedGame.Id).Points(existedGame.Player1.Points, existedGame.Player2.Points);
                await CheckTime();
            }
        }
        public override async Task OnConnectedAsync()
        {
            try
            {
                Console.WriteLine("GameHub hub connected");
                Console.WriteLine($"Players count {GameRepository.PlayersConnections.Count}");
                Console.WriteLine($"Player {Context.User.Identity.Name} connected");
                
                GameRepository.PlayersConnections.Add(Context.User.Identity.Name, Context.ConnectionId);
                int playersCount = GameRepository.PlayersConnections.Count;

                var game = new GameLogic
                {
                    Id = Guid.NewGuid().ToString(),
                    Player1 =
                    {
                        ConnectionId = GameRepository.PlayersConnections.ElementAt(playersCount - 2).Value,
                        PlayerId = playerID,
                        Name = GameRepository.PlayersConnections.ElementAt(playersCount - 2).Key,
                        Lifes = (int)GameSettings.Lifes,  Points = (int)GameSettings.Points
                    },
                    Player2 =
                    {
                        ConnectionId = GameRepository.PlayersConnections.ElementAt(playersCount - 1).Value,
                        PlayerId = playerID,
                        Name = GameRepository.PlayersConnections.ElementAt(playersCount - 1).Key,
                        Lifes = (int)GameSettings.Lifes,  Points = (int)GameSettings.Points
                    }
                };
                
                GameRepository.Games[game.Id] = game;
                game.InProgress = true;

                if (game.InProgress && GameRepository.PlayersConnections.Count >= 2)
                {
                    await Groups.AddToGroupAsync(game.Player1.ConnectionId, game.Id);
                    await Groups.AddToGroupAsync(game.Player2.ConnectionId, game.Id);
                    
                    await Clients.Group(game.Id).PrepareRound();
                    await Clients.Group(game.Id).Timeout();
                    await Clients.Client(game.Player1.ConnectionId).OwnField(game.Field1);
                    await Clients.Client(game.Player2.ConnectionId).OwnField(game.Field2);
                    await Clients.Group(game.Id).Players(game.Player1, game.Player2);
                    await Clients.Group(game.Id).Points(game.Player1.Points, game.Player2.Points);
                    game.Prepare = true;
                    Console.WriteLine("Prepare round: 20 sec");
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                await base.OnConnectedAsync();
            }
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var game = GameRepository.Games.FirstOrDefault(g => g.Value.Player1.ConnectionId == Context.ConnectionId || g.Value.Player2.ConnectionId == Context.ConnectionId).Value;

            if (game != null && game.Player1.Lifes == 0 || game != null && game.Player2.Lifes == 0)
            {
                Console.WriteLine("GameHub disconnected\n");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);
                GameRepository.Games.Remove(game.Id);
                GameRepository.PlayersConnections.Remove(game.Player1.Name);
                GameRepository.PlayersConnections.Remove(game.Player2.Name);
                await base.OnDisconnectedAsync(exception);
            }
        }
    }
}