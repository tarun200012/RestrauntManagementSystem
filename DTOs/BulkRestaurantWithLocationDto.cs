using RestaurantAPI.Dtos;

namespace RestaurantAPI.DTOs
{
    public class BulkRestaurantWithLocationDto
    {
        public List<RestaurantWithLocationDto> Restaurants { get; set; }
    }
}
