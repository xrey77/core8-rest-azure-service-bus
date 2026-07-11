// Controllers/RegistrationController.cs
using Microsoft.AspNetCore.Mvc;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Services;
using core8_rest_azure_service_bus.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient; 
using System;
using System.Threading.Tasks;

namespace core8_rest_azure_service_bus.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationController : ControllerBase 
    {
        private readonly IAuthService authService;
        private readonly ILogger<RegistrationController> logger;

        public RegistrationController(IAuthService _authService, ILogger<RegistrationController> _logger) 
        {
            authService = _authService;
            logger = _logger;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> UserRegistration(RegisterDto dto) 
        {
            try
            {
                await authService.signup(dto);

                return Ok(new { 
                    firstname = dto.Firstname,
                    lastname = dto.Lastname,
                    email = dto.Email,
                    mobile = dto.Mobile,
                    username = dto.Username,
                    password = dto.Password,
                    message = "You have registered successfully, please login."
                });
            }
            catch (AppException ex) 
            {
                // Handle duplicate unique key error (e.g., Username or Email already exists)
                logger.LogWarning(ex, "Registration failed due to duplicate constraints.");
                return StatusCode(500, new {message = ex.Message });
                // return Conflict(new { message = ex.message });
            }
            // catch (SqlException ex)
            // {
            //     // Handle general MS SQL / Dapper connectivity or syntax errors
            //     logger.LogError(ex, "Database error occurred during user registration.");
            //     return StatusCode(500, new { message = "An internal database error occurred. Please try again later." });
            // }
            // catch (Exception ex)
            // {
            //     // Catch-all for any other unexpected application errors
            //     logger.LogError(ex, "An unexpected error occurred during user registration.");
            //     return StatusCode(500, new { message = "An unexpected error occurred." });
            // }
        }
    }
}
