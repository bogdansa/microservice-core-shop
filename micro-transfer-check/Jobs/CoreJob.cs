using System;
using System.Linq;
using NLog;
using Quartz;
using System.Threading.Tasks;
using RawRabbit;
using Newtonsoft.Json;

namespace micro_transfer_check.Jobs
{
    public class OrderTransferReceived
    {
        public Guid Id {get;set;}
    }

    public class CoreJob : IJob
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        // private IBusClient _client;

        // public CoreJob(IBusClient client)
        // {
        //     _client = client;
        // }
        public Task Execute(IJobExecutionContext jobContext)
        {
            _logger.Info("CoreJob Executing");
            try
            {
                _logger.Info("CoreJob doing its magic");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error when executing job");
                return Task.FromResult(false);
            }
        }

        // public Task Execute(IJobExecutionContext jobContext)       
        // {
        //     _logger.Info("CoreJob Executing");
        //     try
        //     {
        //         return Task.Run(() => {
        //             using (var dbContext = new TransferJobDBContext())
        //             {
        //                 var orders = dbContext.OrdersAwaitingTransfer.ToList();
        //                 foreach(var order in orders)
        //                 {
        //                     _logger.Info($"Performing check on order - {order.Id}");
        //                     if(Check(order.Id))
        //                     {
        //                         _logger.Info($"Transfer received for order - {order.Id}");
        //                         dbContext.OrdersAwaitingTransfer.Remove(order);
        //                         dbContext.SaveChanges();

        //                         var jsonData = new OrderTransferReceived { Id = order.Id };

        //                         _client.PublishAsync(jsonData, default(Guid),
        //                         cfg => cfg.WithExchange(ex => ex.WithName("order_exchange")).WithRoutingKey("order_transfer_received"));
        //                         _logger.Info($"Send event order transfer received for - {order.Id}");
        //                     }
        //                     else
        //                     {
        //                         _logger.Info($"Awaiting transfer for order - {order.Id}");
        //                     }
        //                 }

        //             }
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.Error(ex, "Error when executing job");
        //         return Task.FromResult(false);
        //     }
        // }

        private bool Check(Guid orderId)
        {
            var rand = new Random();
            return rand.NextDouble() <= 0.8;
        }
    }
}