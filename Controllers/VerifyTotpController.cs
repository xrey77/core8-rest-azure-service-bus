using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.dto;

namespace core8_rest_azure_service_bus.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VerifyTotpController : ControllerBase {

        public readonly IMfaService mfaService;

        public VerifyTotpController(IMfaService _mfaService) {
            mfaService = _mfaService;
        }

        [HttpPatch("/verifyotp/{id}")]
        public async Task<IActionResult> VerifyOtpCode(int id, VerifyOtpDto dto) {
            try {
                await mfaService.VerifyOtp(id, dto);
                return Ok(new {message = "OTP code has been verified successfully."});
            } catch(AppException ex) {
                return BadRequest(new { message = ex.Message});
            }
        }

    }
    
}