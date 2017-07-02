using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading;
using NLog.Extensions.Logging;
using NLog.Web;
using NLog;
using RawRabbit;
using RawRabbit.vNext;
using RawRabbit.Configuration;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;

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
        private readonly Container _container = new Container();

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
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IControllerActivator>((x) => {
                return new SimpleInjectorControllerActivator(_container);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();

            app.UseDeveloperExceptionPage();

            app.UseMvc();

            app.InitializeContainer(_container, env);

            MainScheduler.Start(_container);
        }
    }
}
