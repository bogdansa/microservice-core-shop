﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace micro_transfer_check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var portNumber =5003;

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls($"http://*:{portNumber}")
                .Build();

            host.Run();
        }
    }
}
