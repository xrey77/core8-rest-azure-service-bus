using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;

namespace core8_rest_azure_service_bus.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SaleController : ControllerBase 
    {
        private readonly ISaleService _saleService; // Changed to private

        public SaleController(ISaleService saleService) {
            _saleService = saleService;
        }

        [HttpGet("/getsales")]
        public async Task<IActionResult> SalesList() {
            try {
                var sales = await _saleService.SalesList();
                return Ok(sales);
            } catch(AppException ex) {
                return NotFound(new { message = ex.Message});
            }
        }
    }
}
