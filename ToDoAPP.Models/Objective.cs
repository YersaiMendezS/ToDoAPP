namespace ToDoAPP.Models
{
    // The granular task (Azure Task)
    public class Objective
    {
        public string Description { get; set; } = string.Empty;
        public string PriorityName { get; set; } = string.Empty;
        public string PriorityColor { get; set; } = "Gray";
        public DateTime? DueDate { get; set; }

        // Gamified Status: 0 = To Do, 1 = In Progress, 2 = Completed
        public int Status { get; set; } = 0;
    }//class Objective
}//namespace ToDoAPP.Models
