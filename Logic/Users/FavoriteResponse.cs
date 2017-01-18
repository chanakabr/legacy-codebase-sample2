
namespace Core.Users
{
    public class FavoriteResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public FavoritObject[] Favorites { get; set; }

    }
}
