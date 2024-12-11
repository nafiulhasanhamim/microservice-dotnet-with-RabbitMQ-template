using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderAPI.DTOs;

namespace OrderAPI.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductReadDto>> GetProducts();
    }
}