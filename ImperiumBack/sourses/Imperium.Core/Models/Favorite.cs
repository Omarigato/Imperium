using System;

namespace Imperium.Core.Models
{
    public class Favorite
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}