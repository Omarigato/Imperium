using System;
using System.ComponentModel.DataAnnotations;

namespace Imperium.Service.DTOs.Favorite
{
    /// <summary>
    /// DTO для добавления товара в избранное
    /// </summary>
    public class AddToFavoriteDto
    {
        [Required]
        public Guid ProductId { get; set; }
    }
}
