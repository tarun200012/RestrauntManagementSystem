using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
