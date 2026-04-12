using System.Collections.Generic;

namespace ToDoAPP.Models
{
    public class TodoData
    {
        public List<TaskItem> ActiveTasks { get; set; } = new();
        public List<TaskItem> CompletedTasks { get; set; } = new();
        public List<PriorityItem> CustomPriorities { get; set; } = new();
    }
}
