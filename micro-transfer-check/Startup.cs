using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading;
using NLog.Extensions.Logging;
using NLog.Web;
using RawRabbit;
using RawRabbit.vNext;
using RawRabbit.Configuration;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace micro_transfer_check
{
    //TODO add to services and refactor this messy mess - this is needed to dupise the client 

    public class OrderAwaitingTransfer
    {
        public Guid Id { get; set; }
    }

    class TransferJobDBContext: DbContext
    {
        public DbSet<OrderAwaitingTransfer> OrdersAwaitingTransfer{ get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=transfer-check-job.db");
        }
    }

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

            MainScheduler.Start();

        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<SomeService>();

            services.AddSingleton<IBusClient>((x) => {
                var busConfig = new RawRabbitConfiguration
                    {
                        Username = "guest",
                        Password = "guest",
                        Port = 5672,
                        VirtualHost = "/",
                        Hostnames = { "localhost" }
                    };

                return BusClientFactory.CreateDefault(busConfig);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();

            app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }

    public class SomeService 
    {
        public SomeService(IBusClient client, ILogger<SomeService> logger)
        {
            client.SubscribeAsync<string>(async (json, context) =>
            {
                var orderAwaitingTransfer = JsonConvert.DeserializeObject<OrderAwaitingTransfer>(json);

                logger.LogInformation($"Received Order Awaiting Transfer - {orderAwaitingTransfer.Id}");

                using (var dbContext = new TransferJobDBContext())
                {
                    dbContext.OrdersAwaitingTransfer.Add(orderAwaitingTransfer);
                    dbContext.SaveChanges();
                }

            }, (cfg) => cfg.WithExchange(
                  ex => ex.WithName("order_exchange")).WithQueue(
                  q => q.WithName("order_queue")).WithRoutingKey("order_awaiting_transfer"));
        }
    }
}
