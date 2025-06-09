namespace Imperium.Core.Models
{
    public class ProductSize
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        public Guid SizeId { get; set; }
        public virtual Dictionary Size { get; set; } = null!;
        
        public bool IsAvailable { get; set; } = true;
    }
}