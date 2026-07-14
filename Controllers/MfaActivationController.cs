using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;

namespace core8_rest_azure_service_bus.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class MfaActivationController : ControllerBase {

        private readonly IMfaService _mfaService;
        private readonly IMessagePublisher _publisher;

        public MfaActivationController(
            IMfaService mfaService,
            IMessagePublisher publisher
            ) {
            _mfaService = mfaService;
            _publisher = publisher;
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> MfaActivation(int id, MfaActivateDto dto) {
            try {
                var userData = await _mfaService.ActivateMfa(id, dto);
                await _publisher.PublishAsync(userData, "MfaActivation");

                return Ok(new {
                    qrcodeurl = userData.Qrcodeurl,
                    message = "Multi-Factor Authenticator has been enabled."});
            } catch(AppException ex) {
                return NotFound(new {mesage = ex.Message});
            }
        }

    }    
}