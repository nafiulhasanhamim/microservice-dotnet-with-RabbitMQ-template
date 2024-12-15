using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductAPI.DTO;
using ProductAPI.DTOs;
using ProductAPI.Interfaces;
using ProductAPI.Models;
using ProductAPI.RabbitMQ;

namespace ProductAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRabbmitMQCartMessageSender _messageBus;


        public ProductService(AppDbContext context, IMapper mapper, IRabbmitMQCartMessageSender messageBus)
        {
            _context = context;
            _mapper = mapper;
            _messageBus = messageBus;
        }

        public async Task<IEnumerable<ProductReadDto>> GetAllAsync()
        {
            var products = await _context.Products.ToListAsync();
            return _mapper.Map<IEnumerable<ProductReadDto>>(products);
        }

        public async Task<ProductReadDto> GetByIdAsync(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) throw new KeyNotFoundException("Product not found");
            return _mapper.Map<ProductReadDto>(product);
        }

        public async Task<ProductReadDto> CreateAsync(ProductCreateDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductReadDto>(product);
        }

        public async Task<ProductReadDto> UpdateAsync(Guid id, ProductUpdateDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new KeyNotFoundException("Product not found");

            _mapper.Map(productDto, product);
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductReadDto>(product);

        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new KeyNotFoundException("Product not found");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStockAsync(OrderDTO eventMessage)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == Guid.Parse(eventMessage.ProductId));
            if (product == null) _messageBus.SendMessage(eventMessage, "stockFailed");
            else if (product.Quantity >= eventMessage.Quantity)
            {
                product.Quantity -= eventMessage.Quantity;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                _messageBus.SendMessage(eventMessage, "stockUpdated");
            }
            else if (product.Quantity < eventMessage.Quantity)
            {
                _messageBus.SendMessage(eventMessage, "stockFailed");
            }
            return true;
        }
    }
}
