using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using core8_rest_azure_service_bus.dto;
using core8_rest_azure_service_bus.Helpers;
using core8_rest_azure_service_bus.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace core8_rest_azure_service_bus.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UploadProfilePicController : ControllerBase
    {

        private readonly IWebHostEnvironment _env;
        private readonly IUserService _userService;
        private readonly IMessagePublisher _publisher;

        public UploadProfilePicController(
            IUserService userService,
            IWebHostEnvironment env,
            IMessagePublisher publisher
            ) {
            _userService = userService;
            _env = env;
            _publisher = publisher;
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> uploadPicture(int id, UploadPicDto dto) {

            if (dto?.Userpic == null || dto.Userpic.Length == 0)
            {
                return BadRequest(new { message = "Profile Picture not found or file is empty." });
            }

            try
            {
                var folderName = Path.Combine(_env.WebRootPath, "users");
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                // Standardized filename (e.g., 005.jpg)
                var filename = $"00{id}.jpg";
                var fullSavePath = Path.Combine(folderName, filename);

                // 3. Process and Save Image asynchronously
                using (var stream = dto.Userpic.OpenReadStream())
                using (var image = await Image.LoadAsync(stream))
                {
                    image.Mutate(x => x.Resize(100, 100));
                    await image.SaveAsync(fullSavePath);
                }

                // 4. Generate dynamic URL (Avoids hardcoding localhost)
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var fileUrl = $"{baseUrl}/users/{filename}";

                var userData = await _userService.UploadPicture(id, filename);
                await _publisher.PublishAsync(userData, "UploadProfilePic");

                return Ok(new { 
                    userpic = filename,
                    message = "You have changed your profile picture successfully."});
            } catch(AppException ex) {
                return BadRequest(new { message = ex.Message});
            }
        }

    }
}