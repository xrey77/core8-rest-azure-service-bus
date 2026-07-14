// Controllers/ProductListController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Models;

namespace core8_rest_azure_service_bus.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductListController : ControllerBase 
    {
        private readonly IProductService _productService;
        private readonly IMessagePublisher _publisher;

        public ProductListController(
            IProductService productService,
            IMessagePublisher publisher            
            ) 
        {
            _productService = productService;
            _publisher = publisher;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> ProductList(int page) 
        {
            try 
            {
                if (page < 1) page = 1; 

                var productsData = await _productService.GetProductListAsync(page);
                await _publisher.PublishAsync(productsData, "ProductList");

                return Ok(productsData);
            }
            catch (AppException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
