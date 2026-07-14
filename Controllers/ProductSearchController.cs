// Controllers/ProductSearchController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Models;

namespace core8_rest_azure_service_bus.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductSearchController : ControllerBase 
    {
        private readonly IProductService _productService;
        private readonly IMessagePublisher _publisher;

        public ProductSearchController(
            IProductService productService,
            IMessagePublisher publisher
            ) 
        {
            _productService = productService;
            _publisher = publisher;
        }

        [HttpGet("{page}/{keyword}")]
        public async Task<IActionResult> ProductList(int page, string keyword) 
        {
            try 
            {
                if (page < 1) page = 1; 

                var productsData = await _productService.GetProductSearchAsync(page, keyword);
                await _publisher.PublishAsync(productsData, "ProductsSearch");

                return Ok(productsData);
            }
            catch (AppException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
