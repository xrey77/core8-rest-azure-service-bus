// Controllers/ProductByCategoryController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;

namespace core8_rest_azure_service_bus.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ProductByCategoryController: ControllerBase {

        public readonly IProductService _productService;

        public ProductByCategoryController(IProductService productService) {
            _productService = productService;
        }

        [HttpGet("/getproductcategory")]
        public async Task<IActionResult> GetProductCategory() {
            try {
                var products = await _productService.GetProductsByCategoryAsync();
                return Ok(products);
            }
            catch(AppException ex) {
                return Conflict(new {message = ex.Message});
            }
        }
    }    
}