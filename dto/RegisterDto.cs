namespace core8_rest_azure_service_bus.dto
{
    public class RegisterDto {
       public int Id { get; set; }
       public string? Firstname { get; set; } 
       public int RoleId { get; set; }
       public string? Lastname { get; set; } = String.Empty;
       public string? Email { get; set; } = String.Empty;
       public string? Mobile { get; set; } = String.Empty;
       public string? Username { get; set; } = String.Empty;
       public string? Password { get; set; } = String.Empty;
    }    
}