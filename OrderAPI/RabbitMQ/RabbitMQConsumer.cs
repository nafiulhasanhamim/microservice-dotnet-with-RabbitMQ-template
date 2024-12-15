// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Text.Json;
// using System.Threading.Tasks;
// using OrderAPI.DTOs;
// using OrderAPI.Interfaces;
// using OrderAPI.Services;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;

// namespace OrderAPI.RabbitMQ
// {
//     public class RabbitMQConsumer : BackgroundService
//     {
//         private readonly IServiceProvider _serviceProvider;
//         private IConnection _connection;
//         private IModel _channel;

//         public RabbitMQConsumer(IServiceProvider serviceProvider)
//         {
//             _serviceProvider = serviceProvider;

//             var factory = new ConnectionFactory
//             {
//                 HostName = "localhost",
//                 UserName = "guest",
//                 Password = "guest"
//             };

//             _connection = factory.CreateConnection();
//             _channel = _connection.CreateModel();
//             _channel.QueueDeclare("stockUpdated", false, false, false, null); // Queue for product update events
//             _channel.QueueDeclare("stockFailed", false, false, false, null); // Queue for product update events
//         }

//         protected override Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             stoppingToken.ThrowIfCancellationRequested();

//             CreateConsumer("stockUpdated", async (message) =>
//             {
//                 var eventMessage = JsonSerializer.Deserialize<OrderReadDto>(message);
                // using (var scope = _serviceProvider.CreateScope())
                // {
                //     var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                //     if (eventMessage == null)
                //     {
                //         return;
                //     }
                //     await orderService.EventHandling(eventMessage, "stockUpdated");
                // }
//             });
//             CreateConsumer("stockFailed", async (message) =>
//             {
                // var eventMessage = JsonSerializer.Deserialize<OrderReadDto>(message);
                // using (var scope = _serviceProvider.CreateScope())
                // {
                //     var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                //     if (eventMessage == null)
                //     {
                //         return;
                //     }
                //     await orderService.EventHandling(eventMessage, "stockFailed");
                // }
//             });

//             return Task.CompletedTask;
//         }

//         private void CreateConsumer(string queueName, Func<string, Task> processMessage)
//         {
//             var consumer = new EventingBasicConsumer(_channel);

//             consumer.Received += async (ch, ea) =>
//             {
//                 var body = ea.Body.ToArray();
//                 var message = Encoding.UTF8.GetString(body);

//                 Console.WriteLine($"Received event from queue {queueName}: {message}");
//                 await processMessage(message);

//                 _channel.BasicAck(ea.DeliveryTag, false);
//             };

//             _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
//         }
//         public override void Dispose()
//         {
//             _channel.Close();
//             _connection.Close();
//             base.Dispose();
//         }
//     }
// }




using System.Text;
using System.Text.Json;
using OrderAPI.DTOs;
using OrderAPI.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductAPI.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the fanout exchange
            _channel.ExchangeDeclare(exchange: "stockUpdated", type: ExchangeType.Fanout);
            _channel.ExchangeDeclare(exchange: "stockFailed", type: ExchangeType.Fanout);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var stockUpdatedQueue = _channel.QueueDeclare().QueueName;
            var stockFailedQueue = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: stockUpdatedQueue, exchange: "stockUpdated", routingKey: "");
            _channel.QueueBind(queue: stockFailedQueue, exchange: "stockFailed", routingKey: "");

            CreateConsumer(stockUpdatedQueue, async (message) =>
            {
                var eventMessage = JsonSerializer.Deserialize<OrderReadDto>(message);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    if (eventMessage == null)
                    {
                        return;
                    }
                    await orderService.EventHandling(eventMessage, "stockUpdated");
                }
            });
            CreateConsumer(stockFailedQueue, async (message) =>
            {
                var eventMessage = JsonSerializer.Deserialize<OrderReadDto>(message);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    if (eventMessage == null)
                    {
                        return;
                    }
                    await orderService.EventHandling(eventMessage, "stockFailed");
                }
            });

            return Task.CompletedTask;
        }

        private void CreateConsumer(string queueName, Func<string, Task> processMessage)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Received event from queue {queueName}: {message}");
                await processMessage(message);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
