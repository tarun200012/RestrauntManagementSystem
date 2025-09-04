using RestaurantAPI.Models;
using System.ComponentModel.DataAnnotations;

public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string? Password { get; set; }

    public string? Phone { get; set; }

    public bool IsDeleted { get; set; }

    [Required]
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();


    // Navigation property for Coupons
    public ICollection<CouponCustomer> CouponCustomers { get; set; } = new List<CouponCustomer>();
}
