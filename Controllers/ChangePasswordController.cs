using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;

namespace core8_rest_azure_service_bus.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChangePasswordController: ControllerBase {

        private readonly IMessagePublisher _publisher;
        private readonly IUserService userService;

        public ChangePasswordController(
            IUserService _userService,
            IMessagePublisher publisher
        ) {
            userService = _userService;
            _publisher = publisher;
            
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> changePassword(int id, ChangePasswordDto dto) {

            try {
                var userData = await userService.ChangeUserPassword(id, dto);

                await _publisher.PublishAsync(userData, "ChangePassword");
                return Ok(
                    new { message = "You have changed you passord successfully." }
                );

            } catch(AppException ex) {
                return NotFound(new { message = ex.Message});
            }
        }
    }
}