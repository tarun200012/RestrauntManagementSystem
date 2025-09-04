namespace RestaurantAPI.Models
{
    public class CouponRestaurant
    {
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
