using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Admin.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ContractNumber { get; set; }

        [Required]
        public int Quantity { get; set; }

        public string? CustomerName { get; set; }

        [Required]
        public string CustomerPhone { get; set; }



        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int FloristId { get; set; }

        [ForeignKey("FloristId")]
        public Florist? Florist { get; set; }

        public int? FlowerId { get; set; }

        [ForeignKey("FlowerId")]
        public Flower? Flower { get; set; }

        [Required]
        public string Status { get; set; } = "Incoming";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "admin";
    }
}
