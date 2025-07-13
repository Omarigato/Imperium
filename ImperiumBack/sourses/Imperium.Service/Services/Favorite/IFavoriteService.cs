using Imperium.Service.DTOs.Favorite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Favorite
{
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteDto>> GetClientFavoritesAsync(Guid clientId);
        Task<FavoriteDto?> AddToFavoriteAsync(Guid clientId, AddToFavoriteDto addDto);
        Task<bool> RemoveFromFavoriteAsync(Guid clientId, Guid productId);
        Task<bool> IsProductInFavoritesAsync(Guid clientId, Guid productId);
        Task<int> GetFavoriteCountAsync(Guid clientId);
        Task<bool> ClearFavoritesAsync(Guid clientId);
    }
}
