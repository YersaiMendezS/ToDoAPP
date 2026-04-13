namespace ToDoAPP.Models
{
    // The major deliverable (Azure Feature)
    public class Mission
    {
        public string Title { get; set; } = string.Empty;
        public List<Objective> Objectives { get; set; } = new();
    }//class Mission
}//namespace ToDoAPP.Models
