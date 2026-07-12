// Controllers/ProductSearchController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Models;

namespace core8_rest_azure_service_bus.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class ProductSearchController : ControllerBase 
    {
        private readonly IProductService _productService;

        public ProductSearchController(IProductService productService) 
        {
            _productService = productService;
        }

        [HttpGet("/productsearch/{page}/{keyword}")]
        public async Task<IActionResult> ProductList(int page, string keyword) 
        {
            try 
            {
                // Ensure page is never 0 or negative
                if (page < 1) page = 1; 

                var products = await _productService.GetProductSearchAsync(page, keyword);
                return Ok(products);
            }
            catch (AppException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
