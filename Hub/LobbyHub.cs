using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MultiMinesweeper.Repository;

namespace MultiMinesweeper.Hub
{
    [Authorize]
    public class LobbyHub : Hub<ILobbyClient>
    {
        public string groupName = "Players";
        private readonly Random _random;
        public LobbyHub(Random random)
        {
            _random = random;
        }
        public async Task CheckPlayers()
        {
            var players = GameRepository.Players;
            await Clients.All.PlayersOnline(players);
        }

        public async Task MatchPlayers()
        {
            GameRepository.ConnectionId.Add(Context.ConnectionId);

            foreach (var item in GameRepository.ConnectionId)
            {
                Console.WriteLine(item);
            }

            if (GameRepository.ConnectionId.Count > 2)
            {
                while (GameRepository.ConnectionId.Count != 2)
                {
                    GameRepository.ConnectionId.RemoveAt(_random.Next(0, GameRepository.ConnectionId.Count));
                    await CheckPlayers();
                }
            } 
            
            else {
                if (GameRepository.ConnectionId.Count == 2)
                {
                    Console.WriteLine($"{GameRepository.ConnectionId.Count}");
                    await Clients.Clients(GameRepository.ConnectionId).ToTheGame();
                    await CheckPlayers();
                    GameRepository.ConnectionId = new List<string>();
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
            await base.OnDisconnectedAsync(exception);
        }
        
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Message Received by: {user}");
            await Clients.All.ReceiveMessage(user, message);
        }
    }
}