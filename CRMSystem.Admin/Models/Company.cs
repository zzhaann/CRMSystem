﻿using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Admin.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Address { get; set; }

        public string? ContactPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "admin";

        //public ICollection<Flower> Flowers { get; set; }

    }
}
