using System;

namespace ToDoAPP.Models
{
    public class TaskItem
    {
        public int TaskNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PriorityName { get; set; } = string.Empty;
        public string PriorityColor { get; set; } = "Gray";
        public DateTime? DueDate { get; set; }
    }
}
