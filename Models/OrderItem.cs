using System.ComponentModel.DataAnnotations;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    public Order? Order { get; set; }

    [Required]
    public int MenuItemId { get; set; }

    public MenuItem? MenuItem { get; set; }

    public int Quantity { get; set; }
}
