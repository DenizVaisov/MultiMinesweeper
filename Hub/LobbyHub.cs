using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MultiMinesweeper.Model;
using MultiMinesweeper.Repository;

namespace MultiMinesweeper.Hub
{
    [Authorize]
    public class LobbyHub : Hub<ILobbyClient>
    {
        private readonly Random _random;
        private readonly RepositoryContext _context;
        public LobbyHub(Random random, RepositoryContext context)
        {
            _random = random;
            _context = context;
        }
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Message Received by: {user}");
            await Clients.All.ReceiveMessage(user, message);
        }
        public async Task CheckPlayers()
        {
            var players = GameRepository.Players;
            await Clients.All.PlayersOnline(players);
        }

        public async Task MatchPlayers()
        {
            var query = _context.Users.Where(l => l.Login == Context.User.Identity.Name).Select(l => l.Points)
                .SingleOrDefault();
            GameRepository.ConnectedPlayersRating.Add(new Rating
            {
                ConnectionId = Context.ConnectionId,
                Login = Context.User.Identity.Name,
                Points = query
            });

            if (GameRepository.PlayersConnectionId.Contains(Context.ConnectionId)) return;
          
            GameRepository.PlayersConnectionId.Add(Context.ConnectionId);

            Console.WriteLine($"Player {Context.User.Identity.Name} added to ranking");
            Console.WriteLine($"Players in ranging: {GameRepository.ConnectedPlayersRating.Count}");

            if (GameRepository.PlayersConnectionId.Count > 2)
            {
                try
                {
                    int i = 8;
                    while (i >= 0)
                    {
                        i--;
                        Console.WriteLine("Finding players");
                        foreach (var item in GameRepository.ConnectedPlayersRating)
                            Console.WriteLine($"Player: {Context.User.Identity.Name}, points: {item.Points}");
                        
                        if (GameRepository.PlayersConnectionId.Count == 2)
                        {
                            foreach (var item in GameRepository.ConnectedPlayersRating)
                                Console.WriteLine($"Player: {Context.User.Identity.Name}, points: {item.Points}");
                            Console.WriteLine("In game");
                            await Clients.Clients(GameRepository.PlayersConnectionId).ToTheGame();
                            GameRepository.PlayersConnectionId.Remove(Context.ConnectionId);

                            await CheckPlayers();
                        }

                        var player = GameRepository.ConnectedPlayersRating.FirstOrDefault(x => x.Points > query);

                        Console.WriteLine(player?.Login + " " + player?.Points);
                        if (player != null)
                        {
                            GameRepository.PlayersConnectionId.Remove(player.ConnectionId);
                        }

                        await CheckPlayers();
                        Console.WriteLine($"Players in ranging {GameRepository.ConnectedPlayersRating.Count}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            else
            {
                if (GameRepository.ConnectedPlayersRating.Count == 2 && GameRepository.PlayersConnectionId.Count == 2)
                {
                    try
                    {
                        var firstPlayer = GameRepository.ConnectedPlayersRating[0];
                        var secondPlayer = GameRepository.ConnectedPlayersRating[1];

                        Console.WriteLine(firstPlayer);
                        Console.WriteLine(secondPlayer);

                        if (firstPlayer.Points >= secondPlayer.Points * 0.1 * _random.Next(8, 10)
                            && secondPlayer.Points >= firstPlayer.Points * 0.1 * _random.Next(8, 10))
                        {
                            Console.WriteLine($"Players connected: {GameRepository.ConnectedPlayersRating.Count}");
                            await Clients.Clients(GameRepository.PlayersConnectionId).ToTheGame();
                            await CheckPlayers();
                        }

                        await CheckPlayers();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            GameRepository.Players.Enqueue(Context.ConnectionId);
            foreach (var item in GameRepository.Players)
            {
                Console.WriteLine($"{item} in queue");
            }
            Console.WriteLine("ChatHub hub connected");
            await Clients.All.PlayersOnline(GameRepository.Players);
            await CheckPlayers();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            GameRepository.Players.Dequeue();
            foreach (var item in GameRepository.Players)
            {
                Console.WriteLine($"{item} dequeue");
            }
            Console.WriteLine("ChatHub hub disconnected");
            await Clients.All.PlayersOnline(GameRepository.Players); 
            GameRepository.PlayersConnectionId.Remove(Context.ConnectionId);
            GameRepository.ConnectedPlayersRating.RemoveAll(player => player.ConnectionId == Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}