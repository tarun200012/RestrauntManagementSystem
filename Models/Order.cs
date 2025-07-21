using RestaurantAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [JsonIgnore]
    public Customer? Customer { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }

    public DateTime ScheduledAt { get; set; }

    public bool IsConfirmed { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
