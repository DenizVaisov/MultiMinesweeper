using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using MultiMinesweeper.Game;
using MultiMinesweeper.HubContract;
using MultiMinesweeper.Model;
using MultiMinesweeper.Repository;

namespace MultiMinesweeper.Hub
{
    [Authorize]
    public class LobbyHub : Hub<ILobbyClient>
    {
        private readonly RepositoryContext _context;
        private double ratingCoeficent = (double)GameSettings.Rating;
        public LobbyHub(RepositoryContext context)
        {
            _context = context;
        }

        public async Task SendAllMessages()
        {
            Console.WriteLine($"Messages count: {LobbyRepository.LobbyChat.Count}");
            await Clients.Client(Context.ConnectionId).ReceiveAllMessages(LobbyRepository.LobbyChat);
        }

        public async Task DeleteAllMessages()
        {
            LobbyRepository.LobbyChat.Clear();
            await Clients.All.ReceiveMessage("", "All messages will be deleted every 24 hours", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
        }
        
        public async Task SendMessage(string user, string message, string time)
        {
            Console.WriteLine($"Message Received by: {user}");
            LobbyRepository.LobbyChat.Add(new Chat {Name = user, Message = message, Time = time});
            await Clients.All.ReceiveMessage(user, message, time);
        }
        public async Task CheckPlayers()
        {
            var players = LobbyRepository.Players;
            await Clients.All.PlayersOnline(players);
        }
        
        public async Task RunPeriodicallyAsync(
            Func<Task> func,
            int interval,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(interval, cancellationToken);
                    await func();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public async Task PlayersRanking()
        {
         try
         {
             while (LobbyRepository.ConnectedPlayersRating.Count > 0)
             {
                 int playersCount = LobbyRepository.ConnectedPlayersRating.Count;
                 List<string> playersConnectionId = LobbyRepository.ConnectedPlayersRating.Select(p => p.ConnectionId).ToList();
                 await Clients.Clients(playersConnectionId).PlayersInRanking(playersCount);
					
                 Console.WriteLine("Finding players");
                 Console.WriteLine($"Players in rating: {LobbyRepository.ConnectedPlayersRating.Count}");
                    
                 if (LobbyRepository.ConnectedPlayersRating.Count == 1 || 
                     LobbyRepository.ConnectedPlayersRating.Count == 0) 
                     return; 
                    
                 for (int i = 0; i < playersCount - 1; i++)
                 {
                     for (int j = i + 1; j < playersCount; j++)
                     {
                         double firstPlayerPoints = LobbyRepository.ConnectedPlayersRating[i].Points;
                         double secondPlayerPoints = LobbyRepository.ConnectedPlayersRating[j].Points;
                         if (firstPlayerPoints >= secondPlayerPoints * 0.1 * ratingCoeficent
                             && secondPlayerPoints >= firstPlayerPoints * 0.1 * ratingCoeficent
                             || firstPlayerPoints <= 100 && secondPlayerPoints <= 100)
                         {
                             string firstPlayerConnectionId = LobbyRepository.ConnectedPlayersRating[j].ConnectionId;
                             string firstPlayerName = LobbyRepository.ConnectedPlayersRating[j].Login;
                             string secondPlayerConnectionId = LobbyRepository.ConnectedPlayersRating[i].ConnectionId;
                             string secondPlayerName = LobbyRepository.ConnectedPlayersRating[i].Login;
                                
                             LobbyRepository.PlayersToGame.Add(firstPlayerName, firstPlayerConnectionId);
                             LobbyRepository.PlayersToGame.Add(secondPlayerName, secondPlayerConnectionId);

                             var playersToGameConnectionId = LobbyRepository.PlayersToGame.Select(p => p.Value).ToList();
                             await Clients.Clients(playersToGameConnectionId).ToTheGame();
								
                             LobbyRepository.ConnectedPlayersRating.RemoveAll(p => p.ConnectionId == firstPlayerConnectionId);
                             LobbyRepository.ConnectedPlayersRating.RemoveAll(p => p.ConnectionId == secondPlayerConnectionId);
                             LobbyRepository.Players.Remove(firstPlayerConnectionId);
                             LobbyRepository.Players.Remove(secondPlayerConnectionId);
                                
                             playersCount = LobbyRepository.ConnectedPlayersRating.Count;
                             playersConnectionId = LobbyRepository.ConnectedPlayersRating.Select(p => p.ConnectionId).ToList();
                             await Clients.Clients(playersConnectionId).PlayersInRanking(playersCount);
                             await Clients.All.PlayersOnline(LobbyRepository.Players);
                             ratingCoeficent = (double)GameSettings.Rating;
                         }
                     }
                 }

                 if (ratingCoeficent > 5 && LobbyRepository.ConnectedPlayersRating.Count >= 2)
                 {
                     ratingCoeficent -= 0.1;
                     Console.WriteLine(ratingCoeficent);
                 }

                 Thread.Sleep(1000);
             }
         }
         catch (Exception e)
         {
             Console.WriteLine(e);
             throw;
         }
        }

        public async Task MatchPlayers()
        {
            var points = _context.Users
                .Where(p => p.Login == Context.User.Identity.Name)
                .Select(p => p.Points)
                .SingleOrDefault();
            
            LobbyRepository.ConnectedPlayersRating.Add(new Rating
            {
                ConnectionId = Context.ConnectionId, Login = Context.User.Identity.Name, Points = points
            });

            if (LobbyRepository.ConnectedPlayersRating.Count < 2)
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;
                Console.WriteLine("Start ranking");
                await RunPeriodicallyAsync(PlayersRanking, 2000, token);
            }
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("ChatHub hub connected");
            LobbyRepository.Players.Add(Context.ConnectionId);
            await Clients.All.PlayersOnline(LobbyRepository.Players);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("ChatHub hub disconnected");
            LobbyRepository.ConnectedPlayersRating.RemoveAll(player => player.ConnectionId == Context.ConnectionId);
            LobbyRepository.Players.Remove(Context.ConnectionId);
            await Clients.All.PlayersOnline(LobbyRepository.Players); 
            await base.OnDisconnectedAsync(exception);
        }
    }
}