using System;

namespace Imperium.Core.Models
{
    public class ProductFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public Guid FileId { get; set; }
        public virtual File File { get; set; } = null!;

        public bool IsAddition { get; set; } = false;

        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeleteDate { get; set; }
    }
}