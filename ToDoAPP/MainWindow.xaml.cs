using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.ComponentModel;
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

        private PriorityItem _editingPriority = null;

        private void ShowTaskPopup_Click(object sender, RoutedEventArgs e)
        {
            _editingIndex = -1;
            PopupTitle.Text = "Create Task";
            AddButton.Content = "Save Task";
            TaskInput.Clear();
            TaskDatePicker.SelectedDate = null;
            if (PriorityComboBox.Items.Count > 0) PriorityComboBox.SelectedIndex = 1;
            TaskPopup.Visibility = Visibility.Visible;
            PriorityEditorPanel.Visibility = Visibility.Collapsed;
            TaskInput.Focus();
        }

        private void CloseTaskPopup_Click(object sender, RoutedEventArgs e)
        {
            TaskPopup.Visibility = Visibility.Collapsed;
            PriorityEditorPanel.Visibility = Visibility.Collapsed;
        }

        private void AddPriority_Click(object sender, RoutedEventArgs e)
        {
            _editingPriority = null;
            PriorityNameInput.Clear();
            PriorityColorComboBox.SelectedIndex = 0;
            PriorityEditorPanel.Visibility = Visibility.Visible;
        }

        private void EditPriority_Click(object sender, RoutedEventArgs e)
        {
            if (PriorityComboBox.SelectedItem is PriorityItem selected)
            {
                _editingPriority = selected;
                PriorityNameInput.Text = selected.Name;

                bool colorFound = false;
                foreach (ComboBoxItem item in PriorityColorComboBox.Items)
                {
                    if (item.Content.ToString().Equals(selected.Color, StringComparison.OrdinalIgnoreCase))
                    {
                        PriorityColorComboBox.SelectedItem = item;
                        colorFound = true;
                        break;
                    }
                }
                if (!colorFound && PriorityColorComboBox.Items.Count > 0)
                    PriorityColorComboBox.SelectedIndex = 0;

                PriorityEditorPanel.Visibility = Visibility.Visible;
            }
        }

        private void DeletePriority_Click(object sender, RoutedEventArgs e)
        {
            if (PriorityComboBox.SelectedItem is PriorityItem selected)
            {
                if (_priorities.Count > 1)
                {
                    _priorities.Remove(selected);
                    UpdatePrioritiesList();
                    PriorityEditorPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Cannot delete the last priority.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void CancelPriorityEdit_Click(object sender, RoutedEventArgs e)
        {
            PriorityEditorPanel.Visibility = Visibility.Collapsed;
        }

        private void SavePriority_Click(object sender, RoutedEventArgs e)
        {
            string name = PriorityNameInput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                string color = "Gray";
                if (PriorityColorComboBox.SelectedItem is ComboBoxItem colorItem)
                {
                    color = colorItem.Content.ToString();
                }

                if (_editingPriority != null)
                {
                    string oldName = _editingPriority.Name;
                    _editingPriority.Name = name;
                    _editingPriority.Color = color;

                    foreach (var item in TaskListView.Items.OfType<TaskItem>())
                    {
                        if (item.PriorityName == oldName)
                        {
                            item.PriorityName = name;
                            item.PriorityColor = color;
                        }
                    }
                    foreach (var item in CompletedTaskListView.Items.OfType<TaskItem>())
                    {
                        if (item.PriorityName == oldName)
                        {
                            item.PriorityName = name;
                            item.PriorityColor = color;
                        }
                    }

                    TaskListView.Items.Refresh();
                    CompletedTaskListView.Items.Refresh();
                }
                else
                {
                    var newPriority = new PriorityItem { Name = name, Color = color };
                    _priorities.Add(newPriority);
                    _editingPriority = newPriority;
                }

                UpdatePrioritiesList();
                PriorityComboBox.SelectedItem = _editingPriority;
                PriorityEditorPanel.Visibility = Visibility.Collapsed;
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
                    PriorityEditorPanel.Visibility = Visibility.Collapsed;
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

        private void ShowFilterPopup_Click(object sender, RoutedEventArgs e)
        {
            var filterPriorities = new List<PriorityItem> { new PriorityItem { Name = "All", Color = "Transparent" } };
            filterPriorities.AddRange(_priorities);
            FilterPriorityComboBox.ItemsSource = filterPriorities;

            if (FilterPriorityComboBox.SelectedItem == null)
            {
                FilterPriorityComboBox.SelectedIndex = 0;
            }

            FilterPopup.Visibility = Visibility.Visible;
        }

        private void CloseFilterPopup_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.Visibility = Visibility.Collapsed;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var selectedPriority = FilterPriorityComboBox.SelectedItem as PriorityItem;
            string priorityName = selectedPriority?.Name == "All" ? null : selectedPriority?.Name;
            DateTime? filterDate = FilterDatePicker.SelectedDate;

            Predicate<object> filter = obj =>
            {
                if (obj is TaskItem task)
                {
                    bool matchPriority = string.IsNullOrEmpty(priorityName) || task.PriorityName == priorityName;
                    bool matchDate = !filterDate.HasValue || (task.DueDate.HasValue && task.DueDate.Value.Date == filterDate.Value.Date);
                    return matchPriority && matchDate;
                }
                return false;
            };

            TaskListView.Items.Filter = filter;
            CompletedTaskListView.Items.Filter = filter;

            ApplySorting();

            FilterPopup.Visibility = Visibility.Collapsed;
        }

        private void ApplySorting()
        {
            TaskListView.Items.SortDescriptions.Clear();
            CompletedTaskListView.Items.SortDescriptions.Clear();

            int sortByIndex = SortByComboBox.SelectedIndex;
            if (sortByIndex > 0)
            {
                string propertyName = sortByIndex == 1 ? "DueDate" : "PriorityName";
                var direction = SortDirectionComboBox.SelectedIndex == 0 
                    ? ListSortDirection.Ascending 
                    : ListSortDirection.Descending;

                TaskListView.Items.SortDescriptions.Add(new SortDescription(propertyName, direction));
                CompletedTaskListView.Items.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterPriorityComboBox.SelectedIndex = 0;
            FilterDatePicker.SelectedDate = null;
            SortByComboBox.SelectedIndex = 0;
            SortDirectionComboBox.SelectedIndex = 0;

            TaskListView.Items.Filter = null;
            CompletedTaskListView.Items.Filter = null;

            TaskListView.Items.SortDescriptions.Clear();
            CompletedTaskListView.Items.SortDescriptions.Clear();

            FilterPopup.Visibility = Visibility.Collapsed;
        }
    }
}