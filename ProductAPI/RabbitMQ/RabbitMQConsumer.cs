using System.Text;
using System.Text.Json;
using ProductAPI.DTO;
using ProductAPI.DTOs;
using ProductAPI.Interfaces;
using ProductAPI.Services;
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

            // Declare queues for multiple events
            _channel.QueueDeclare("orderCreated", false, false, false, null);  // Queue for product check events
            // _channel.QueueDeclare("productchk", false, false, false, null);  // Queue for product check events
            // _channel.QueueDeclare("productupd", false, false, false, null); // Queue for product update events
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            // Create and configure consumers for each queue
            // CreateConsumer("productchk", async (message) =>
            // {
            //     var eventMessage = JsonSerializer.Deserialize<ProductReadDto>(message);
            //     using (var scope = _serviceProvider.CreateScope())
            //     {
            //         var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            //         Console.WriteLine($"Processing ProductDeleted event for Product ID: {eventMessage.ProductId}");
            //         await ProcessDeleteEvent(eventMessage, productService);
            //     }
            // });

            CreateConsumer("orderCreated", async (message) =>
            {
                var eventMessage = JsonSerializer.Deserialize<OrderDTO>(message);
                // Console.WriteLine("orderCreated event is listened from products");
                // Console.WriteLine(eventMessage.ProductId);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                    await ProcessUpdateStockEvent(eventMessage, productService);
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

                // Console.WriteLine($"Received event from queue {queueName}: {message}");
                await processMessage(message);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        // private async Task ProcessDeleteEvent(ProductReadDto eventMessage, IProductService productService)
        // {
        //     // Handle delete event
        //     Console.WriteLine($"Deleting product with ID: {eventMessage.ProductId}");
        //     // await productService.DeleteAsync(eventMessage.ProductId);
        // }
        private async Task ProcessUpdateStockEvent(OrderDTO eventMessage, IProductService productService)
        {
            Console.WriteLine("ProcessUpdateStockEvents");
            Console.WriteLine(eventMessage.ProductId);
            await productService.UpdateStockAsync(eventMessage);
        }

        // private async Task ProcessUpdateEvent(ProductReadDto eventMessage, ProductService productService)
        // {
        //     // Handle update event
        //     Console.WriteLine($"Updating product with ID: {eventMessage.ProductId}");
        //     productService.UpdateStockAsync(eventMessage);
        // }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
