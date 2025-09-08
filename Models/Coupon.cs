using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public enum DiscountType
    {
        Flat,
        Percent,
        BOGO
    }

    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DiscountType DiscountType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinOrderAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Soft delete flag
        public bool IsDeleted { get; set; } = false;
        public ICollection<CouponRestaurant> CouponRestaurants { get; set; } = new List<CouponRestaurant>();
        public ICollection<CouponCustomer> CouponCustomers { get; set; } = new List<CouponCustomer>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
