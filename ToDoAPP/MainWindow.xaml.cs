using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.Json;
using System.Linq;
using ToDoAPP.Models;

namespace ToDoAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _editingIndex = -1;
        private bool _isDarkMode = false;

        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme();
            InitializePriorities();
            LoadTasks();
        }

        private const string SaveFilePath = "tasks.json";

        public class TodoDataLocal
        {
            public List<TaskItem> ActiveTasks { get; set; } = new();
            public List<TaskItem> CompletedTasks { get; set; } = new();
            public List<PriorityItem> CustomPriorities { get; set; } = new();
            public bool IsDarkMode { get; set; } = false;
            public int LastTaskNumber { get; set; } = 0;
        }

        private Random _rnd = new Random();
        private List<PriorityItem> _priorities = new();
        private int _lastTaskNumber = 0;

        private void InitializePriorities()
        {
            _priorities.Add(new PriorityItem { Name = "High", Color = "Red" });
            _priorities.Add(new PriorityItem { Name = "Medium", Color = "Goldenrod" });
            _priorities.Add(new PriorityItem { Name = "Low", Color = "Green" });
            UpdatePrioritiesList();
        }

        private void UpdatePrioritiesList()
        {
            PriorityComboBox.ItemsSource = null;
            PriorityComboBox.ItemsSource = _priorities;
            if (PriorityComboBox.Items.Count > 0)
                PriorityComboBox.SelectedIndex = PriorityComboBox.Items.Count > 0 ? 1 : 0;
        }

        private void ApplyTheme()
        {
            if (_isDarkMode)
            {
                this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                this.Foreground = Brushes.White;
                Application.Current.Resources["PopupBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
                Application.Current.Resources["TextBrush"] = Brushes.White;
                Application.Current.Resources["ControlBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42"));

                // Override global system colors to deeply re-theme complicated native controls (like DatePicker calendar and ComboBox dropdowns)
                Application.Current.Resources[SystemColors.WindowBrushKey] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
                Application.Current.Resources[SystemColors.WindowTextBrushKey] = Brushes.White;
                Application.Current.Resources[SystemColors.ControlBrushKey] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42"));
                Application.Current.Resources[SystemColors.ControlTextBrushKey] = Brushes.White;
                Application.Current.Resources[SystemColors.ControlLightBrushKey] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
            }
            else
            {
                this.Background = Brushes.White;
                this.Foreground = Brushes.Black;
                Application.Current.Resources["PopupBackgroundBrush"] = Brushes.White;
                Application.Current.Resources["TextBrush"] = Brushes.Black;
                Application.Current.Resources["ControlBackgroundBrush"] = Brushes.White;

                Application.Current.Resources[SystemColors.WindowBrushKey] = SystemColors.WindowBrush;
                Application.Current.Resources[SystemColors.WindowTextBrushKey] = SystemColors.WindowTextBrush;
                Application.Current.Resources[SystemColors.ControlBrushKey] = SystemColors.ControlBrush;
                Application.Current.Resources[SystemColors.ControlTextBrushKey] = SystemColors.ControlTextBrush;
                Application.Current.Resources[SystemColors.ControlLightBrushKey] = SystemColors.ControlLightBrush;
            }

            if (ThemeComboBox != null)
            {
                ThemeComboBox.SelectedIndex = _isDarkMode ? 1 : 0;
            }
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var data = JsonSerializer.Deserialize<TodoDataLocal>(json);

                    if (data != null)
                    {
                        if (data.CustomPriorities != null)
                        {
                            foreach (var p in data.CustomPriorities)
                                _priorities.Add(p);
                            UpdatePrioritiesList();
                        }

                        foreach (var task in data.ActiveTasks)
                            TaskListView.Items.Add(task);

                        foreach (var task in data.CompletedTasks)
                            CompletedTaskListView.Items.Add(task);

                        _isDarkMode = data.IsDarkMode;
                        _lastTaskNumber = data.LastTaskNumber;
                        ApplyTheme();
                    }
                }
            }
            catch
            {
                // Ignore load errors, maybe fallback on old json later if required
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
                var data = new TodoDataLocal();

                // Exclude defaults from saved custom priorities
                var customPrios = _priorities.Skip(3).ToList();
                data.CustomPriorities.AddRange(customPrios);

                foreach (TaskItem item in TaskListView.Items)
                    data.ActiveTasks.Add(item);

                foreach (TaskItem item in CompletedTaskListView.Items)
                    data.CompletedTasks.Add(item);

                data.IsDarkMode = _isDarkMode;
                data.LastTaskNumber = _lastTaskNumber;

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SaveFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        private void ShowSettingsPopup_Click(object sender, RoutedEventArgs e)
        {
            ThemeComboBox.SelectedIndex = _isDarkMode ? 1 : 0;
            SettingsPopup.Visibility = Visibility.Visible;
        }

        private void CloseSettingsPopup_Click(object sender, RoutedEventArgs e)
        {
            SettingsPopup.Visibility = Visibility.Collapsed;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox != null && ThemeComboBox.SelectedIndex >= 0)
            {
                _isDarkMode = ThemeComboBox.SelectedIndex == 1;
                ApplyTheme();
            }
        }

        private void ShowTaskPopup_Click(object sender, RoutedEventArgs e)
        {
            _editingIndex = -1;
            PopupTitle.Text = "Create Task";
            AddButton.Content = "Save Task";
            TaskInput.Clear();
            TaskDatePicker.SelectedDate = null;
            PriorityComboBox.SelectedIndex = 1;
            TaskPopup.Visibility = Visibility.Visible;
            NewPriorityPanel.Visibility = Visibility.Collapsed;
            TaskInput.Focus();
        }

        private void CloseTaskPopup_Click(object sender, RoutedEventArgs e)
        {
            TaskPopup.Visibility = Visibility.Collapsed;
            NewPriorityPanel.Visibility = Visibility.Collapsed;
        }

        private void AddPriority_Click(object sender, RoutedEventArgs e)
        {
            NewPriorityPanel.Visibility = NewPriorityPanel.Visibility == Visibility.Collapsed 
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveNewPriority_Click(object sender, RoutedEventArgs e)
        {
            string name = NewPriorityInput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                string randomColor = $"#{_rnd.Next(0x1000000):X6}";
                var newPriority = new PriorityItem { Name = name, Color = randomColor };
                _priorities.Add(newPriority);
                UpdatePrioritiesList();
                PriorityComboBox.SelectedItem = newPriority;
                NewPriorityInput.Clear();
                NewPriorityPanel.Visibility = Visibility.Collapsed;
            }
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
            string newTaskDescription = TaskInput.Text;
            var selectedPriority = PriorityComboBox.SelectedItem as PriorityItem;

            if (!string.IsNullOrWhiteSpace(newTaskDescription) && selectedPriority != null)
            {
                var taskItem = new TaskItem
                {
                    Description = newTaskDescription,
                    PriorityName = selectedPriority.Name,
                    PriorityColor = selectedPriority.Color,
                    DueDate = TaskDatePicker.SelectedDate
                };

                if (_editingIndex >= 0)
                {
                    var existingTask = TaskListView.Items[_editingIndex] as TaskItem;
                    if (existingTask != null) taskItem.TaskNumber = existingTask.TaskNumber;

                    TaskListView.Items[_editingIndex] = taskItem;
                    _editingIndex = -1;
                }
                else
                {
                    _lastTaskNumber++;
                    taskItem.TaskNumber = _lastTaskNumber;
                    TaskListView.Items.Add(taskItem);
                }

                TaskInput.Clear();
                TaskPopup.Visibility = Visibility.Collapsed;
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TaskItem task)
            {
                _editingIndex = TaskListView.Items.IndexOf(task);
                if (_editingIndex >= 0)
                {
                    TaskInput.Text = task.Description;
                    TaskDatePicker.SelectedDate = task.DueDate;

                    var matchingPriority = _priorities.FirstOrDefault(p => p.Name == task.PriorityName);
                    if (matchingPriority != null)
                    {
                        PriorityComboBox.SelectedItem = matchingPriority;
                    }
                    else if (_priorities.Count > 0)
                    {
                        PriorityComboBox.SelectedIndex = 1;
                    }

                    PopupTitle.Text = "Edit Task";
                    AddButton.Content = "Update Task";
                    TaskPopup.Visibility = Visibility.Visible;
                    NewPriorityPanel.Visibility = Visibility.Collapsed;
                    TaskInput.Focus();
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TaskItem task)
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
            if (sender is CheckBox checkBox && checkBox.Tag is TaskItem task)
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
            if (sender is CheckBox checkBox && checkBox.Tag is TaskItem task)
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