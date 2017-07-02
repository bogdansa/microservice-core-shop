using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Quartz.Simpl;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.vNext;
using System;
using Newtonsoft.Json;
using NLog;

namespace micro_transfer_check 
{

    public class SomeService 
    {
        NLog.ILogger logger = LogManager.GetCurrentClassLogger();

        public SomeService(IBusClient client )
        {
            client.SubscribeAsync<string>(async (json, context) =>
            {
                var orderAwaitingTransfer = JsonConvert.DeserializeObject<OrderAwaitingTransfer>(json);

                logger.Info($"Received Order Awaiting Transfer - {orderAwaitingTransfer.Id}");

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

    public static class AppExtension
    {
        public static void InitializeContainer(this IApplicationBuilder app, Container container, IHostingEnvironment env)
        {
            container.RegisterMvcControllers(app);

            container.RegisterSingleton<IBusClient>(() => {
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

            container.RegisterSingleton<IScheduler>(() =>
            {
                var sched = new StdSchedulerFactory().GetScheduler().Result;
                sched.JobFactory = new SimpleInjectiorJobFactory(container);
                return sched;
            });

            container.RegisterSingleton<SomeService>();
            container.Verify();
        }
    }

    public class SimpleInjectiorJobFactory : SimpleJobFactory
    {
        private readonly Container _container;

        public SimpleInjectiorJobFactory(Container container)
        {
            _container = container;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                return (IJob)_container.GetInstance(bundle.JobDetail.JobType);
            }
            catch (Exception ex)
            {
                throw new SchedulerException($"Problem while instantiating job '{bundle.JobDetail.Key}' from the NinjectJobFactory.", ex);
            }
        }
    }
}