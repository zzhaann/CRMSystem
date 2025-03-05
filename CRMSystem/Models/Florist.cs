using System.ComponentModel.DataAnnotations;


namespace CPM.Models
{
    public class Florist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Phone { get; set; }
    }
}
