// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Newtonsoft.Json;
// using ProductAPI.DTO;
// using RabbitMQ.Client;

// namespace ProductAPI.RabbitMQ
// {
//     public class RabbmitMQCartMessageSender : IRabbmitMQCartMessageSender
//     {

//         private readonly string _hostName;
//         private readonly string _username;
//         private readonly string _password;
//         private IConnection _connection;

//         public RabbmitMQCartMessageSender()
//         {
//             _hostName = "localhost";
//             _password = "guest";
//             _username = "guest";
//         }

//         public void SendMessage(object message, string queueName)
//         {
//             if (ConnectionExists())
//             {
//                 using var channel = _connection.CreateModel();
//                 channel.QueueDeclare(queueName, false, false, false, null);
//                 var json = JsonConvert.SerializeObject(message);
//                 var body = Encoding.UTF8.GetBytes(json);
//                 channel.BasicPublish(exchange: "", routingKey: queueName, null, body: body);
//             }

//         }

//         private void CreateConnection()
//         {
//             try
//             {
//                 var factory = new ConnectionFactory
//                 {
//                     HostName = _hostName,
//                     Password = _password,
//                     UserName = _username
//                 };

//                 _connection = factory.CreateConnection();
//             }
//             catch (Exception ex)
//             {

//             }
//         }

//         private bool ConnectionExists()
//         {
//             if (_connection != null)
//             {
//                 return true;
//             }
//             CreateConnection();
//             return true;
//         }
//     }
// }


using System.Text;
using Newtonsoft.Json;
using ProductAPI.RabbitMQ;
using RabbitMQ.Client;

namespace ProductAPI.RabbitMQ
{
    public class RabbmitMQCartMessageSender : IRabbmitMQCartMessageSender
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;

        public RabbmitMQCartMessageSender()
        {
            _hostName = "localhost";
            _password = "guest";
            _username = "guest";
        }

        public void SendMessage(object message, string exchangeName = "DefaultExchange")
        {
            if (string.IsNullOrEmpty(exchangeName))
            {
                throw new ArgumentException("Exchange name cannot be null or empty.", nameof(exchangeName));
            }

            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();

                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: "", // Routing key is ignored for fanout exchanges
                    basicProperties: null,
                    body: body
                );

                Console.WriteLine($"Message sent to exchange {exchangeName}: {json}");
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _username
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create connection: {ex.Message}");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }

            CreateConnection();
            return _connection != null;
        }
    }
}
