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

        private readonly IUserService userService;
        private readonly ILogger<GetUserIdController> logger;
        
        public GetUserIdController(IUserService _userService, ILogger<GetUserIdController> _logger) {
            userService = _userService;
            logger = _logger;

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserId(int id) {
            try {
                var user =  await userService.GetUserId(id);
                return Ok(new {
                    id = user.Id,
                    firstname = user.Firstname,
                    lastname = user.Lastname,
                    email = user.Email,
                    mobile = user.Mobile,
                    username = user.Username,
                    userpic = user.Userpic,
                    isactivated = user.Isactivated,
                    isblocked = user.Isblocked,
                    mailtoken = user.Mailtoken,
                    qrcodeurl = user.Qrcodeurl
                });
            } catch(AppException ex) {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(500, new { message = ex.Message});
            }
        }
    }
    
}