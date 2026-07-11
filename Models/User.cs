namespace core8_rest_azure_service_bus.Models
{

    public class User {
        public int Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Userpic { get; set; }
        public int Isactivated { get; set; }
        public int Isblocked { get; set; }
        public int Mailtoken { get; set; }
        public string? Secret { get; set; }
        public string? Qrcodeurl { get; set; }
    }
    
}