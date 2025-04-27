using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMSystem.Models
{
    public class Flower
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int InitialQuantity { get; set; } // начальное количество

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ClientPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "admin";


        [Required]
        public int CompanyId { get; set; }



        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}