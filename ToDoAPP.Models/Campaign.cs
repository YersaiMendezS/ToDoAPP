namespace ToDoAPP.Models
{
    // The overarching goal (Azure Epic)
    public class Campaign
    {
        public string Title { get; set; } = string.Empty;
        public List<Mission> Missions { get; set; } = new();
    }//class Campaign
}//namespace ToDoAPP.Models
