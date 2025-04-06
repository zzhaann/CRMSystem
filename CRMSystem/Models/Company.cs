using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Address { get; set; }

        public string? ContactPhone { get; set; }

        public ICollection<Flower> Flowers { get; set; }
    }
}
