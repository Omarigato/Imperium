namespace Imperium.Core.Models
{
    public class ProductFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public Guid FileId { get; set; }
        public virtual File File { get; set; } = null!;
        
        public bool IsAddition { get; set; } = false;
    }
}