using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core8_rest_azure_service_bus.dto
{
    public class UploadPicDto {
        public int Id { get; set; }
        public required IFormFile Userpic { get; set; }
    }        
}