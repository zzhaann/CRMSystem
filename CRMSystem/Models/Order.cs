using CPM.Models;
using System;
using System.ComponentModel.DataAnnotations; //рекуайред, кей ушын колданылып тур
using System.ComponentModel.DataAnnotations.Schema; //Форейн кей, калам

namespace CRMSystem.Models
{
    public class Order
    {
        [Key] //primary 
        public int Id { get; set; }

        [Required]
        public string ContractNumber { get; set; }

        [Required] //mindetti turde toltyru ushin 
        public string FlowerName { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? CustomerName { get; set; }

        [Required]
        public string CustomerPhone { get; set; }

        [Required]
        public int FloristId { get; set; }

        [ForeignKey("FloristId")]
        public Florist Florist { get; set; }

        [Required]
        public string Status { get; set; } = "Incoming";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
