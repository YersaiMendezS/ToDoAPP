using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                    AddButton.Content = "+";
                }
                else
                {
                    // 3. Add the task to the ListBox
                    TaskListView.Items.Add(newTask);
                }

                // 4. Clear the text box so they can type a new task
                TaskInput.Clear();
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
                    AddButton.Content = "Update Task";
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