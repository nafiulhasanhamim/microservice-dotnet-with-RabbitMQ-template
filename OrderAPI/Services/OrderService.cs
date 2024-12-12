using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderAPI.data;
using OrderAPI.DTOs;
using OrderAPI.Interfaces;
using OrderAPI.Models;
using OrderAPI.RabbitMQ;

namespace OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly IRabbmitMQCartMessageSender _messageBus;
        private readonly Dictionary<Guid, object> _orders = new();



        public OrderService(AppDbContext context, IMapper mapper, IProductService productService, IRabbmitMQCartMessageSender messageBus)
        {
            _context = context;
            _mapper = mapper;
            _productService = productService;
            _messageBus = messageBus;
        }

        public async Task<OrderReadDto> CreateOrderAsync(OrderCreateDto orderDto)
        {
            // IEnumerable<ProductReadDto> products = await _productService.GetProducts();            
            // _messageBus.SendMessage(orderDto, "productchk");
            // _messageBus.SendMessage(orderDto, "productupd");
            // foreach (var product in products)
            // {
            //     Console.WriteLine($"ProductId: {product.ProductId}, Name: {product.Name}, Price: {product.Price}");
            // }
            var order = _mapper.Map<Order>(orderDto);
            order.UserValidationStatus = "pending";
            order.StockValidationStatus = "pending";
            order.OrderStatus = "pending";
            order.OrderDate = DateTime.UtcNow;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var message = new
            {
                OrderId = order.OrderId,
                ProductId = (orderDto.ProductId),
                UserId = orderDto.UserId,
                Quantity = orderDto.Quantity
            };
            _messageBus.SendMessage(new
            {
                OrderId = order.OrderId,
                ProductId = orderDto.ProductId,
                UserId = orderDto.UserId,
                Quantity = orderDto.Quantity
            }, "orderCreated");
            
            _messageBus.SendMessage(new
            {
                OrderId = order.OrderId,
                ProductId = orderDto.ProductId,
                UserId = orderDto.UserId,
                Quantity = orderDto.Quantity
            }, "orderCreateds");

            return _mapper.Map<OrderReadDto>(order);
        }

        public async Task<IEnumerable<OrderReadDto>> GetAllOrdersAsync()
        {
            var products = await _context.Orders.ToListAsync();
            return _mapper.Map<IEnumerable<OrderReadDto>>(products);
        }

        public async Task<OrderReadDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(p => p.OrderId == id);
            if (order == null) throw new KeyNotFoundException("order not found");
            return _mapper.Map<OrderReadDto>(order);
        }

        public async Task<bool> EventHandling(OrderReadDto eventMessage, string eventType)
        {
            if (eventMessage == null)
            {
                return false;
            }
            var order = await _context.Orders.FirstOrDefaultAsync(p => p.OrderId == eventMessage.OrderId);
            if (order != null)
            {
                switch (eventType)
                {
                    case "stockUpdated":
                        order.StockValidationStatus = "success";
                        break;

                    case "stockFailed":
                        order.StockValidationStatus = "failed";
                        break;
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                var isFinalized = await FinalizeOrder(eventMessage);
            }
            return true;
        }

        public async Task<bool> FinalizeOrder(OrderReadDto eventMessage)
        {
            if (eventMessage == null)
            {
                return false;
            }
            var order = await _context.Orders.FirstOrDefaultAsync(p => p.OrderId == eventMessage.OrderId);

            if (order == null)
            {
                return false;
            }

            if (order.StockValidationStatus == "success")
            {
                order.OrderStatus = "completed";
            }
            else 
            {
                order.OrderStatus = "cancelled";
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}