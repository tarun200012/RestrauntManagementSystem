namespace RestaurantAPI.DTOs
{
    public class CreateCouponRequestDto
    {
        public string Name { get; set; } = string.Empty;

        // Client will send "Flat"/"flat"/"FLAT" or "Percentage"
        public string DiscountType { get; set; } = string.Empty;

        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal MinOrderAmount { get; set; }

        public bool IsActive { get; set; } = true;

        // Optional: list of restaurant IDs, null/empty means "all restaurants"
        public List<int>? RestaurantIds { get; set; }

        // Optional: list of customer IDs, null/empty means "all customers"
        public List<int>? CustomerIds { get; set; }
    }

}
