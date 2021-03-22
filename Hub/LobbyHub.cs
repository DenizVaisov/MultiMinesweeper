using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Authorization;
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
        private bool isRankingStart;
        public LobbyHub(RepositoryContext context)
        {
            _context = context;
        }
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Message Received by: {user}");
            await Clients.All.ReceiveMessage(user, message);
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
                Console.WriteLine("Start ranking");
                await Task.Delay(interval, cancellationToken);
                await func();
            }
        }

        public async Task PlayersRanking()
        {
            if (LobbyRepository.ConnectedPlayersRating.Count >= 2)
            {
                try
                {
                    int playersCount = LobbyRepository.ConnectedPlayersRating.Count;
                    await Clients.Clients(LobbyRepository.Players).PlayersInRanking(playersCount);

                    while (true)
                    {
                        Console.WriteLine("Finding players");
                        Console.WriteLine($"Players in rating: {LobbyRepository.ConnectedPlayersRating.Count}");
                        
                        if (LobbyRepository.ConnectedPlayersRating.Count == 1 || 
                            LobbyRepository.ConnectedPlayersRating.Count == 0) 
                            break; 
                        
                        for (int i = 0; i < playersCount - 1; i++)
                        {
                            for (int j = i + 1; j < playersCount; j++)
                            {
                                if (LobbyRepository.ConnectedPlayersRating.Count >= 2
                                    && LobbyRepository.ConnectedPlayersRating[i].Points >=
                                    LobbyRepository.ConnectedPlayersRating[j].Points * 0.1 * ratingCoeficent
                                    && LobbyRepository.ConnectedPlayersRating[j].Points >=
                                    LobbyRepository.ConnectedPlayersRating[i].Points * 0.1 * ratingCoeficent
                                )
                                {
                                    string firstPlayerConnectionId = LobbyRepository.ConnectedPlayersRating[j].ConnectionId;
                                    string secondPlayerConnectionId = LobbyRepository.ConnectedPlayersRating[i].ConnectionId;
                                    
                                    LobbyRepository.PlayersToGame.Add(firstPlayerConnectionId);
                                    LobbyRepository.PlayersToGame.Add(secondPlayerConnectionId);

                                    ratingCoeficent = (double)GameSettings.Rating;
                                    await Clients.Clients(LobbyRepository.PlayersToGame).ToTheGame();
                                    
                                    LobbyRepository.ConnectedPlayersRating.RemoveAll(player => player.ConnectionId == firstPlayerConnectionId);
                                    LobbyRepository.ConnectedPlayersRating.RemoveAll(player => player.ConnectionId == secondPlayerConnectionId);
                                    LobbyRepository.Players.Remove(firstPlayerConnectionId);
                                    LobbyRepository.Players.Remove(secondPlayerConnectionId);
                                    
                                    playersCount = LobbyRepository.ConnectedPlayersRating.Count;
                                    await Clients.Clients(LobbyRepository.Players).PlayersInRanking(playersCount);
                                    await Clients.All.PlayersOnline(LobbyRepository.Players);
                                    break;
                                }
                            }
                        }
                        
                        if (ratingCoeficent > 5 && LobbyRepository.ConnectedPlayersRating.Count >= 2)
                            ratingCoeficent -= 0.1;
                        Thread.Sleep(3000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public async Task MatchPlayers()
        {
            var points = _context.Users
                .Where(p => p.Login == Context.User.Identity.Name)
                .Select(p => p.Points)
                .SingleOrDefault();
            
//            if(LobbyRepository.ConnectedPlayersRating.Exists(player => player.Login == Context.User.Identity.Name)) return;
            LobbyRepository.ConnectedPlayersRating.Add(new Rating
            {
                ConnectionId = Context.ConnectionId, Login = Context.User.Identity.Name, Points = points
            });
            
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            if(!isRankingStart)
                await RunPeriodicallyAsync(PlayersRanking, 2000, token);
//            await PlayersRanking();
            isRankingStart = true;
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