namespace RestaurantAPI.Models
{
    public class CouponCustomer
    {
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
