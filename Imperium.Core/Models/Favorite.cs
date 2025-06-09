namespace Imperium.Core.Models
{
    public class Favorite
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}