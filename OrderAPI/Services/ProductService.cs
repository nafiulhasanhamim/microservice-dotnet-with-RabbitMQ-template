using OrderAPI.DTOs;
using OrderAPI.Interfaces;
using Newtonsoft.Json;

namespace OrderAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductService(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }
        public async Task<IEnumerable<ProductReadDto>> GetProducts()
        {
            var client = _httpClientFactory.CreateClient("Product");
            var response = await client.GetAsync($"/api/Product");
            Console.WriteLine("response");

            // if (response.IsSuccessStatusCode)
            // {
                var apiContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("apicontent", apiContent);
                return JsonConvert.DeserializeObject<IEnumerable<ProductReadDto>>(apiContent);
            // }

            // return new List<ProductReadDto>();
        }

    }
}