namespace RestaurantAPI.Dtos
{
    public class RestaurantWithLocationDto
    {
        public int? Id { get; set; }  // Optional for creation, required for update
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }

        public LocationDto Location { get; set; }  // Embedded one-to-one location
    }

    public class LocationDto
    {
        public int? Id { get; set; }  // Optional for creation, required for update
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
    }
}
