﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAPI.Models
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        // Navigation property for one-to-one
        public Location ? Location { get; set; } = null!;

        // Foreign key
        public int ? LocationId { get; set; }

        public bool IsDeleted { get; set; }
    }
}
