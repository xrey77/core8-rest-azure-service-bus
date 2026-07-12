// Controllers/ProductListController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Models;

namespace core8_rest_azure_service_bus.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class ProductListController : ControllerBase 
    {
        private readonly IProductService _productService;

        public ProductListController(IProductService productService) 
        {
            _productService = productService;
        }

        [HttpGet("/productlist/{page}")]
        public async Task<IActionResult> ProductList(int page) 
        {
            try 
            {
                // Ensure page is never 0 or negative
                if (page < 1) page = 1; 

                var products = await _productService.GetProductListAsync(page);
                return Ok(products);
            }
            catch (AppException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
