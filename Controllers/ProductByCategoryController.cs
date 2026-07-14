// Controllers/ProductByCategoryController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;

namespace core8_rest_azure_service_bus.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductByCategoryController: ControllerBase {

        private readonly IProductService _productService;
        private readonly IMessagePublisher _publisher;

        public ProductByCategoryController(
            IProductService productService,
            IMessagePublisher publisher            
            ) {
            _productService = productService;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductCategory() {
            try {
                var productsData = await _productService.GetProductsByCategoryAsync();
                await _publisher.PublishAsync(productsData, "ProductByCategory");

                return Ok(productsData);
            }
            catch(AppException ex) {
                return Conflict(new {message = ex.Message});
            }
        }
    }    
}