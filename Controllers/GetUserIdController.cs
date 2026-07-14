// Controllers/GetUseridController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Models;
using core8_rest_azure_service_bus.Helpers;

namespace core8_rest_azure_service_bus.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GetUserIdController: ControllerBase {

        private readonly IMessagePublisher _publisher;
        private readonly IUserService userService;
        private readonly ILogger<GetUserIdController> logger;
        
        public GetUserIdController(
            IUserService _userService,
            ILogger<GetUserIdController> _logger,
            IMessagePublisher publisher
            ) {
            userService = _userService;
            logger = _logger;
            _publisher =publisher;

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserId(int id) {
            try {
                var userData =  await userService.GetUserId(id);

                await _publisher.PublishAsync(userData, "GetUserid");

                return Ok(new {
                    id = userData.Id,
                    firstname = userData.Firstname,
                    lastname = userData.Lastname,
                    email = userData.Email,
                    mobile = userData.Mobile,
                    username = userData.Username,
                    userpic = userData.Userpic,
                    isactivated = userData.Isactivated,
                    isblocked = userData.Isblocked,
                    mailtoken = userData.Mailtoken,
                    qrcodeurl = userData.Qrcodeurl
                });
            } catch(AppException ex) {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(500, new { message = ex.Message});
            }
        }
    }
    
}