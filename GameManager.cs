using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MultiMinesweeper
{
    public class GameManager
    {
        private static GameManager instance;
        private static readonly object padLock = new object();
        public ConcurrentDictionary<string, Player> Players { get; set; }
        public Timer Timer;

        public static GameManager Instance
        {
            get
            {
                lock (padLock)
                {
                    return instance ?? (instance = new GameManager());
                }
            }
        }
    }
}