using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.dto;

namespace core8_rest_azure_service_bus.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VerifyTotpController : ControllerBase {

        private readonly IMfaService _mfaService;
        private readonly IMessagePublisher _publisher;

        public VerifyTotpController(
            IMfaService mfaService,
            IMessagePublisher publisher
            ) {
            _mfaService = mfaService;
            _publisher = publisher;
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> VerifyOtpCode(int id, VerifyOtpDto dto) {
            try {
                var userData = await _mfaService.VerifyOtp(id, dto);
                await _publisher.PublishAsync(userData, "VerifyTotp");
                return Ok(new {
                    username = userData.Username,
                    message = "OTP code has been verified successfully."});
            } catch(AppException ex) {                
                return BadRequest(new { message = ex.Message});
            }
        }

    }
    
}