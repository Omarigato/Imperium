namespace Imperium.Service.DTOs.Product
{
    public class ProductStatisticsDto
    {
        public int TotalProducts { get; set; }
        public int AvailableProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int ProductsCreatedThisMonth { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
}
