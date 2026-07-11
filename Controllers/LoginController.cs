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
    [Route("[controller]")]
    public class LoginController: ControllerBase {

        private readonly IAuthService authService;
        private readonly ILogger<LoginController> logger;
        private readonly IConfiguration configuration;  


        public LoginController(IAuthService _authService, ILogger<LoginController> _logger, IConfiguration _configuration) {
            authService = _authService;
            logger = _logger;
            configuration = _configuration;
        }

        [HttpPost("/login")]
        public async Task<IActionResult> userLogin(LoginDto dto) {

            try {
                var user = await authService.signin(dto);


                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username!)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = configuration["Jwt:Issuer"],
                    Audience = configuration["Jwt:Audience"]
                };
                
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);



                return Ok( new {
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
                });
                
            } catch(AppException ex) {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(500, new { message = ex.Message});
            }


        }
    }
}
