// Controllers/UpdateUserController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;

namespace core8_rest_azure_service_bus.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UpdateUserController: ControllerBase {

        private readonly IUserService userService;
        private readonly ILogger<UpdateUserController> logger;
        private readonly IMessagePublisher _publisher;

        public UpdateUserController(
            IUserService _userService,
            ILogger<UpdateUserController> _logger,
            IMessagePublisher publisher
            ) {
            userService = _userService;
            logger = _logger;
            _publisher = publisher;
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserDto dto) {

            try {
                var userData = await userService.UpdateUserProfile(id, dto);
                await _publisher.PublishAsync(userData, "UpdateProfile");

                return Ok(new {
                    message = "You have updated you profile successfully."
                });
            } catch(AppException ex) {
                logger.LogWarning(ex, ex.Message);
                return NotFound(new { message = ex.Message});
            }
        }
    }
}