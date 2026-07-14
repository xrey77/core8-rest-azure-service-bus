// Controllers/LoginController.cs
using  Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;


namespace core8_rest_azure_service_bus.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class LoginController: ControllerBase {

        private readonly IAuthService _authService;
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;  
        private readonly IMessagePublisher _publisher;

        public LoginController(
            IAuthService authService, 
            ILogger<LoginController> logger, 
            IConfiguration configuration,
            IMessagePublisher publisher
            ) {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> userLogin(LoginDto dto) {

            try {
                var user = await _authService.signin(dto);
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username!)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"]
                };
                
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                var userMessage = new {
                    firstname = user.Firstname,
                    lastname = user.Lastname,
                    email = user.Email,
                    mobile = user.Mobile,
                    username = user.Username,
                    userpic = user.Userpic,
                    isactivated = user.Isactivated,
                    isblocked = user.Isblocked,
                    mailtoken = user.Mailtoken,
                    qrcodeurl = user.Qrcodeurl,
                    token = tokenString,
                    message = "You have Logged-in successfully."
                };

                await _publisher.PublishAsync(userMessage, "UserLoggedIn");

                return Ok(userMessage);
                
            } catch(AppException ex) {
                _logger.LogWarning(ex, ex.Message);
                return StatusCode(500, new { message = ex.Message});
            }


        }
    }
}
