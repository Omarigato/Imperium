using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Core.Models
{
    public class Review
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string? Comment { get; set; }
        
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}