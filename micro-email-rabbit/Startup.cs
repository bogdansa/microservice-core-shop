using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using RawRabbit;
using RawRabbit.vNext;
using RawRabbit.Configuration;

namespace micro_email
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            env.ConfigureNLog("NLog.config");

            logger.LogInformation("Service started");

            var busConfig = new RawRabbitConfiguration
                {
                    Username = "guest",
                    Password = "guest",
                    Port = 5672,
                    VirtualHost = "/",
                    Hostnames = { "localhost" }
                };

            var client = BusClientFactory.CreateDefault(busConfig);

            client.SubscribeAsync<Tuple<string, string>>(async (msg, context) =>
            {
                logger.LogInformation($"Sending email - {msg.Item1} with message {msg.Item2}")
                Console.WriteLine($"Recieved: {msg.Item1} {msg.Item2}.");
            }, (cfg) => cfg.WithExchange(
                  ex => ex.WithName("email_exchange")).WithQueue(
                  q => q.WithName("email_queue")));

        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();

            app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }
}
