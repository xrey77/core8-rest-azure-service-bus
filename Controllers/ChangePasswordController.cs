using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;

namespace core8_rest_azure_service_bus.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChangePasswordController: ControllerBase {


        private readonly IUserService userService;

        public ChangePasswordController(IUserService _userService) {
            userService = _userService;
        }

        [HttpPatch("/changepassword/{id}")]
        public async Task<IActionResult> changePassword(int id, ChangePasswordDto dto) {

            try {
                await userService.ChangeUserPassword(id, dto);
                return Ok(
                    new { message = "You have changed you passord successfully." }
                );

            } catch(AppException ex) {
                return NotFound(new { message = ex.Message});
            }
        }
    }
}