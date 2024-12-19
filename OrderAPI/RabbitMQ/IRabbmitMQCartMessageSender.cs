using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderAPI.DTOs;

namespace OrderAPI.RabbitMQ
{
    public interface IRabbmitMQCartMessageSender
    {
        void SendMessage(object message, string name, string type);
    }
}