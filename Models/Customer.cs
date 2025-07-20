using System.ComponentModel.DataAnnotations;

public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
