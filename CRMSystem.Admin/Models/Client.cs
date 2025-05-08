using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Admin.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Required]
        public string Phone { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "admin";
    }
}
