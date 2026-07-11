using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;

namespace core8_rest_azure_service_bus.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MfaActivationController : ControllerBase {

        public readonly IMfaService mfaService;

        public MfaActivationController(IMfaService _mfaService) {
            mfaService = _mfaService;
        }

        [HttpPatch("/activatemfa/{id}")]
        public async Task<IActionResult> MfaActivation(int id, MfaActivateDto dto) {
            try {
                var user = await mfaService.ActivateMfa(id, dto);
                return Ok(new {
                    qrcodeurl = user.Qrcodeurl,
                    message = "Multi-Factor Authenticator has been enabled."});
            } catch(AppException ex) {
                return NotFound(new {mesage = ex.Message});
            }
        }

    }    
}