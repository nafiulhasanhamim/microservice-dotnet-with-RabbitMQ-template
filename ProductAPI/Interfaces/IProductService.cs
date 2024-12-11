
using ProductAPI.DTO;
using ProductAPI.DTOs;

namespace ProductAPI.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductReadDto>> GetAllAsync();
        Task<ProductReadDto> GetByIdAsync(Guid id);
        Task<ProductReadDto> CreateAsync(ProductCreateDto productDto);
        Task<ProductReadDto> UpdateAsync(Guid id, ProductUpdateDto productDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> UpdateStockAsync(OrderDTO eventMessage);
    }
}