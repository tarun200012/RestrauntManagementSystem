namespace RestaurantAPI.DTOs
{
    namespace RestaurantAPI.DTOs.Coupon
    {
        public class CouponResponseDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
            public decimal MinOrderAmount { get; set; }
            public decimal DiscountValue { get; set; }
            public string DiscountType { get; set; } = string.Empty; // e.g., "Flat", "Percentage"
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsActive { get; set; }

            // Linked Info (flattened to avoid cycles)
            public List<int> CustomerIds { get; set; } = new();
            public List<int> RestaurantIds { get; set; } = new();
        }
    }

}
