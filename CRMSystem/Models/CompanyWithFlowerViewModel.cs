using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class CompanyWithFlowerViewModel
    {
        // Company fields
        [Required]
        public string Name { get; set; }

        public string? Address { get; set; }

        [Required]
        public string ContactPhone { get; set; }

        // Flower fields
        [Required]
        public string FlowerName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Укажите количество")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Укажите цену")]
        public decimal Price { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Укажите цену продажи")]
        public decimal ClientPrice { get; set; }

        public int InitialQuantity { get; set; } // начальное количество
       

    }
}
