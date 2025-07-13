using System;

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

        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;
        
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeleteDate { get; set; }
    }
}