using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderAPI.DTOs;

namespace OrderAPI.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderReadDto>> GetAllOrdersAsync();
        Task<OrderReadDto?> GetOrderByIdAsync(Guid id);
        Task<OrderReadDto> CreateOrderAsync(OrderCreateDto orderDto);
        Task<bool> EventHandling(OrderReadDto orderData, string eventType);
        Task<bool> FinalizeOrder(OrderReadDto orderId);

    }
}