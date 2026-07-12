// Controllers/RegistrationController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace core8_rest_azure_service_bus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase 
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegistrationController> _logger;
        private readonly IMessagePublisher _publisher;

        public RegistrationController(
            IAuthService authService, 
            ILogger<RegistrationController> logger,
            IMessagePublisher publisher) 
        {
            _authService = authService;
            _logger = logger;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> UserRegistration([FromBody] RegisterDto dto) 
        {
            try
            {
                await _authService.signup(dto);
                
                var userNotification = new 
                {
                    firstname = dto.Firstname,
                    lastname = dto.Lastname,
                    email = dto.Email,
                    mobile = dto.Mobile,
                    username = dto.Username,
                    message = "You have registered successfully, please login."                    
                };

                // Publishes the anonymous object to Azure Service Bus
                await _publisher.PublishAsync(userNotification, "UserRegistered");
                
                return Ok(userNotification);
            }
            catch (AppException ex) 
            {
                _logger.LogWarning(ex, "Registration failed due to application business rules.");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
