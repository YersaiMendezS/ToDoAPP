using System.Text;
using System.Windows;
using System.Windows.Controls;

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
                    AddButton.Content = "Add Task";
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
    }
}