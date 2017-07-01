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
using Newtonsoft.Json;

namespace micro_email
{
    public class EmailMessage
    {
        public string EmailAddress { get;set; }
        public string Message { get;set; }
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

            var busConfig = new RawRabbitConfiguration
                {
                    Username = "guest",
                    Password = "guest",
                    Port = 5672,
                    VirtualHost = "/",
                    Hostnames = { "localhost" }
                };

            var client = BusClientFactory.CreateDefault(busConfig);

            client.SubscribeAsync<string>(async (json, context) =>
            {
                var msg = JsonConvert.DeserializeObject<EmailMessage>(json);

                logger.LogInformation($"Sending email - {msg.EmailAddress} with message {msg.Message}");
            }, (cfg) => cfg.WithExchange(
                  ex => ex.WithName("email_exchange")).WithQueue(
                  q => q.WithName("email_queue")).WithRoutingKey("email_command"));

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
