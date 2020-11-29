using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MultiMinesweeper.HubContract;
using MultiMinesweeper.Model;
using MultiMinesweeper.Repository;

namespace MultiMinesweeper.Hub
{
    [Authorize]
    public class LobbyHub : Hub<ILobbyClient>
    {
        private readonly RepositoryContext _context;
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

        public async Task MatchPlayers()
        {
            var query = _context.Users.Where(l => l.Login == Context.User.Identity.Name).Select(l => l.Points)
                .SingleOrDefault();
            LobbyRepository.ConnectedPlayersRating.Add(new Rating
            {
                ConnectionId = Context.ConnectionId, Login = Context.User.Identity.Name, Points = query
            });

            if (LobbyRepository.PlayersConnectionId.Contains(Context.ConnectionId)) return;
            LobbyRepository.PlayersConnectionId.Add(Context.ConnectionId);

            Console.WriteLine($"Player {Context.User.Identity.Name} added to ranking");
            Console.WriteLine($"Players in ranging: {LobbyRepository.ConnectedPlayersRating.Count}");

            if (LobbyRepository.PlayersConnectionId.Count > 2)
            {
                try
                {
                    int i = LobbyRepository.PlayersConnectionId.Count * 2;
                    while (i >= 0)
                    {
                        Console.WriteLine("Finding players");
                        foreach (var item in LobbyRepository.ConnectedPlayersRating)
                            Console.WriteLine($"Player: {Context.User.Identity.Name}, points: {item.Points}");
                        
                        if (LobbyRepository.PlayersConnectionId.Count == 2)
                        {
                            foreach (var item in LobbyRepository.ConnectedPlayersRating)
                                Console.WriteLine($"Player: {Context.User.Identity.Name}, points: {item.Points}");
                            Console.WriteLine("In game");
                            await Clients.Clients(LobbyRepository.PlayersConnectionId).ToTheGame();
                            LobbyRepository.PlayersConnectionId.Remove(Context.ConnectionId);

                            await CheckPlayers();
                        }

                        var player = LobbyRepository.ConnectedPlayersRating.FirstOrDefault(x => x.Points > query);

                        if (player != null)
                            LobbyRepository.PlayersConnectionId.Remove(player.ConnectionId);

                        await CheckPlayers();
                        Console.WriteLine($"Players in ranging {LobbyRepository.ConnectedPlayersRating.Count}");
                        i--;
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
                if (LobbyRepository.ConnectedPlayersRating.Count == 2 && LobbyRepository.PlayersConnectionId.Count == 2)
                {
                    try
                    {
                        var firstPlayer = LobbyRepository.ConnectedPlayersRating[0];
                        var secondPlayer = LobbyRepository.ConnectedPlayersRating[1];

                        if (firstPlayer.Points >= secondPlayer.Points * 0.1 * 8
                            || secondPlayer.Points >= firstPlayer.Points * 0.1 * 8)
                        {
                            Console.WriteLine($"Players connected: {LobbyRepository.ConnectedPlayersRating.Count}");
                            await Clients.Clients(LobbyRepository.PlayersConnectionId).ToTheGame();
                            await CheckPlayers();
                        }

                        await CheckPlayers();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            LobbyRepository.Players.Enqueue(Context.ConnectionId);
            foreach (var item in LobbyRepository.Players)
            {
                Console.WriteLine($"{item} in queue");
            }
            Console.WriteLine("ChatHub hub connected");
            await Clients.All.PlayersOnline(LobbyRepository.Players);
            await CheckPlayers();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            LobbyRepository.Players.Dequeue();
            foreach (var item in LobbyRepository.Players)
            {
                Console.WriteLine($"{item} dequeue");
            }
            Console.WriteLine("ChatHub hub disconnected");
            await Clients.All.PlayersOnline(LobbyRepository.Players); 
            LobbyRepository.PlayersConnectionId.Remove(Context.ConnectionId);
            LobbyRepository.ConnectedPlayersRating.RemoveAll(player => player.ConnectionId == Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}