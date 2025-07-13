namespace Imperium.Service.DTOs.Order
{
    /// <summary>
    /// DTO для админского управления заказом
    /// </summary>
    public class AdminOrderActionDto
    {
        public string? Notes { get; set; }
        public string? Reason { get; set; }
    }
}
