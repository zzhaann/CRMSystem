namespace CRMSystem.Admin.Models
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public List<Flower> Flowers { get; set; }
        public List<Florist> Florists { get; set; }
        public List<Client> Clients { get; set; }
    }
}
