﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MultiMinesweeper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("PORT");
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://*:{port}")
                .UseStartup<Startup>();
        }
    }
}