using RestaurantAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class MenuItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }

    public bool IsDeleted { get; set; }

    public decimal Cost { get; set; } = 0;  // default value for new rows

}
