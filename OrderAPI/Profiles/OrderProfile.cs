using AutoMapper;
using OrderAPI.DTOs;
using OrderAPI.Models;
namespace OrderAPI.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderReadDto>();
            CreateMap<OrderCreateDto, Order>();
        }
    }
}