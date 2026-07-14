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
    [Route("api/[controller]")]
    public class SalesdataController : ControllerBase 
    {
        private readonly ISaleService _saleService;
        private readonly IMessagePublisher _publisher;

        public SalesdataController(
            ISaleService saleService,
            IMessagePublisher publisher
            ) {
            _saleService = saleService;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> SalesList() {
            try {
                var salesData = await _saleService.SalesList();
                await _publisher.PublishAsync(salesData, "SalesList");

                return Ok(salesData);
            } catch(AppException ex) {
                return NotFound(new { message = ex.Message});
            }
        }
    }
}
