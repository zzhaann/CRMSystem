namespace CRMSystem.Models
{
    public class AnalyticsViewModel
    {
        public List<string> Labels { get; set; } = new(); //x osi boinsha saktaidt kun ushyn sagat 
        public List<int> OrderCounts { get; set; } = new();
        public List<decimal> TotalRevenue { get; set; } = new();

    
        public List<string> TopFlowers { get; set; } = new();
        public List<int> FlowerCounts { get; set; } = new();

  
        public List<string> TopFlorists { get; set; } = new();
        public List<int> FloristOrders { get; set; } = new();
    }
}
