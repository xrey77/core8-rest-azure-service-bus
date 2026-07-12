namespace core8_rest_azure_service_bus.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Descriptions { get; set; }
        public int Qty { get; set; }
        public string? Unit { get; set; }
        public decimal Costprice { get; set; }
        public decimal Sellprice { get; set; }
        public decimal Saleprice { get; set; }
        public string? Productpicture { get; set; }
        public int Alertstocks { get; set; }
        public int Criticalstocks { get; set; }
    }
}