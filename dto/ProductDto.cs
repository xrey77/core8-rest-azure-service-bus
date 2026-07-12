namespace core8_rest_azure_service_bus.dto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Descriptions { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
    }        
}