namespace Imperium.Core.Models
{
    public class ProductColor
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        public Guid ColorId { get; set; }
        public virtual Dictionary Color { get; set; } = null!;
        
        public bool IsAvailable { get; set; } = true;
    }
}