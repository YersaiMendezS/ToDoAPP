using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Text.Json;

namespace ToDoAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _editingIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            LoadTasks();
        }

        private const string SaveFilePath = "tasks.json";

        public class TodoData
        {
            public List<string> ActiveTasks { get; set; } = new();
            public List<string> CompletedTasks { get; set; } = new();
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var data = JsonSerializer.Deserialize<TodoData>(json);

                    if (data != null)
                    {
                        foreach (var task in data.ActiveTasks)
                            TaskListView.Items.Add(task);

                        foreach (var task in data.CompletedTasks)
                            CompletedTaskListView.Items.Add(task);
                    }
                }
            }
            catch
            {
                // Ignore load errors
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SaveTasks();
        }

        private void SaveTasks()
        {
            try
            {
                var data = new TodoData();
                foreach (string item in TaskListView.Items)
                    data.ActiveTasks.Add(item);

                foreach (string item in CompletedTaskListView.Items)
                    data.CompletedTasks.Add(item);

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SaveFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        private void ShowTaskPopup_Click(object sender, RoutedEventArgs e)
        {
            _editingIndex = -1;
            PopupTitle.Text = "Create Task";
            AddButton.Content = "Save Task";
            TaskInput.Clear();
            TaskPopup.Visibility = Visibility.Visible;
            TaskInput.Focus();
        }

        private void CloseTaskPopup_Click(object sender, RoutedEventArgs e)
        {
            TaskPopup.Visibility = Visibility.Collapsed;
        }

        private void TaskInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTask_Click(sender, e);
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get the text the user typed
            string newTask = TaskInput.Text;

            // 2. Make sure it's not empty before adding it
            if (!string.IsNullOrWhiteSpace(newTask))
            {
                if (_editingIndex >= 0)
                {
                    // Update existing task
                    TaskListView.Items[_editingIndex] = newTask;
                    _editingIndex = -1;
                }
                else
                {
                    // 3. Add the task to the ListBox
                    TaskListView.Items.Add(newTask);
                }

                // 4. Clear the text box so they can type a new task
                TaskInput.Clear();
                TaskPopup.Visibility = Visibility.Collapsed;
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var task = button.Tag as string;
                _editingIndex = TaskListView.Items.IndexOf(task);

                if (_editingIndex >= 0)
                {
                    TaskInput.Text = task;
                    PopupTitle.Text = "Edit Task";
                    AddButton.Content = "Update Task";
                    TaskPopup.Visibility = Visibility.Visible;
                    TaskInput.Focus();
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string task)
            {
                int index = TaskListView.Items.IndexOf(task);
                if (index >= 0)
                {
                    TaskListView.Items.RemoveAt(index);

                    if (_editingIndex == index)
                    {
                        _editingIndex = -1;
                        TaskInput.Clear();
                        AddButton.Content = "Add Task";
                    }
                    else if (_editingIndex > index)
                    {
                        _editingIndex--;
                    }
                }
                else
                {
                    int completedIndex = CompletedTaskListView.Items.IndexOf(task);
                    if (completedIndex >= 0)
                    {
                        CompletedTaskListView.Items.RemoveAt(completedIndex);
                    }
                }
            }
        }

        private void TaskCompleted_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is string task)
            {
                int index = TaskListView.Items.IndexOf(task);
                if (index >= 0)
                {
                    TaskListView.Items.RemoveAt(index);
                    CompletedTaskListView.Items.Add(task);

                    if (_editingIndex == index)
                    {
                        _editingIndex = -1;
                        TaskInput.Clear();
                        AddButton.Content = "Add Task";
                    }
                    else if (_editingIndex > index)
                    {
                        _editingIndex--;
                    }
                }
            }
        }

        private void TaskCompleted_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is string task)
            {
                int index = CompletedTaskListView.Items.IndexOf(task);
                if (index >= 0)
                {
                    CompletedTaskListView.Items.RemoveAt(index);
                    TaskListView.Items.Add(task);
                }
            }
        }
    }
}