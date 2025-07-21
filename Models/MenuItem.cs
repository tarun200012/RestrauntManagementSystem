using RestaurantAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class MenuItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }

    public bool IsDeleted { get; set; }
}
