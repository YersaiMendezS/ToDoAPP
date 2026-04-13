using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.IO;
using System.Text.Json;
using System.Linq;
using ToDoAPP.Models;
using ToDoAPP.Application.Interfaces;
using ToDoAPP.Application.Services;
using ToDoAPP.Infrastructure.Data;
using ToDoAPP.Infrastructure.Theme;

namespace ToDoAPP
{
    public partial class MainWindow : Window
    {
        private bool _isDarkMode = false;

        private readonly IDataStore _dataStore;
        private readonly IThemeManager _themeManager;
        private QuestManager _questManager;

        private List<PriorityItem> _priorities = new();

        // State tracking
        private int _nameEntryMode = 0; // 1 = Campaign, 2 = Mission
        private Campaign _selectedCampaignForMission = null;

        public MainWindow()
        {
            InitializeComponent();
            _dataStore = new FileJsonDataStore();
            _themeManager = new WpfThemeManager();
            ApplyTheme();
            InitializePriorities();
            LoadData();
        }

        private void InitializePriorities()
        {
            _priorities.Add(new PriorityItem { Name = "High", Color = "Red" });
            _priorities.Add(new PriorityItem { Name = "Medium", Color = "Goldenrod" });
            _priorities.Add(new PriorityItem { Name = "Low", Color = "Green" });
            PriorityComboBox.ItemsSource = _priorities;
            PriorityComboBox.SelectedIndex = 1;
        }

        // --- THEME ---
        private void ApplyTheme()
        {
            if (_themeManager == null) return;
            _themeManager.ApplyTheme(this, ThemeComboBox, _isDarkMode);
        }

        private void ShowSettingsPopup_Click(object sender, RoutedEventArgs e) => SettingsPopup.Visibility = Visibility.Visible;
        private void CloseSettingsPopup_Click(object sender, RoutedEventArgs e) => SettingsPopup.Visibility = Visibility.Collapsed;
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox != null && ThemeComboBox.SelectedIndex >= 0)
            {
                _isDarkMode = ThemeComboBox.SelectedIndex == 1;
                ApplyTheme();
            }
        }

        // --- DATA SAVING & LOADING ---
        private void LoadData()
        {
            var data = _dataStore.LoadData();
            _questManager = new QuestManager(data.Campaigns);
            _isDarkMode = data.IsDarkMode;
            ApplyTheme();

            RefreshJournalTree();
            RefreshMissionDropdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SaveData();
        }

        private void SaveData()
        {
            _dataStore.SaveData(_questManager.Campaigns, _isDarkMode);
        }

        // --- UI UPDATERS ---
        private void RefreshJournalTree()
        {
            JournalTreeView.ItemsSource = null;
            JournalTreeView.ItemsSource = _questManager.Campaigns;
        }

        private void RefreshMissionDropdown()
        {
            var allMissions = _questManager.GetAllMissions();

            BoardFocusComboBox.ItemsSource = null;
            BoardFocusComboBox.ItemsSource = allMissions;
            if (allMissions.Count > 0) BoardFocusComboBox.SelectedIndex = 0;
        }

        private void RefreshKanbanBoard()
        {
            BoardToDoList.Items.Clear();
            BoardDoingList.Items.Clear();
            BoardDoneList.Items.Clear();

            if (BoardFocusComboBox.SelectedItem is Mission currentMission)
            {
                foreach (var obj in currentMission.Objectives)
                {
                    if (obj.Status == 0) BoardToDoList.Items.Add(obj);
                    else if (obj.Status == 1) BoardDoingList.Items.Add(obj);
                    else if (obj.Status == 2) BoardDoneList.Items.Add(obj);
                }
            }
        }

        private void BoardFocusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshKanbanBoard();
        }

        // --- ADDING CAMPAIGNS & MISSIONS ---
        private void AddCampaign_Click(object sender, RoutedEventArgs e)
        {
            _nameEntryMode = 1;
            NamePopupTitle.Text = "Enter Campaign Name:";
            NameEntryInput.Clear();
            NameEntryPopup.Visibility = Visibility.Visible;
        }

        private void AddMission_Click(object sender, RoutedEventArgs e)
        {
            if (JournalTreeView.SelectedItem is Campaign selectedCamp)
            {
                _selectedCampaignForMission = selectedCamp;
                _nameEntryMode = 2;
                NamePopupTitle.Text = $"New Mission for '{selectedCamp.Title}':";
                NameEntryInput.Clear();
                NameEntryPopup.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Please select a Campaign in the Tree View first.", "Planning Phase");
            }
        }

        private void CloseNamePopup_Click(object sender, RoutedEventArgs e) => NameEntryPopup.Visibility = Visibility.Collapsed;

        private void SaveName_Click(object sender, RoutedEventArgs e)
        {
            string name = NameEntryInput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (_nameEntryMode == 1) // Campaign
                {
                    _questManager.AddCampaign(name);
                }
                else if (_nameEntryMode == 2 && _selectedCampaignForMission != null) // Mission
                {
                    _questManager.AddMission(_selectedCampaignForMission, name);
                    RefreshMissionDropdown();
                }
                RefreshJournalTree();
                NameEntryPopup.Visibility = Visibility.Collapsed;
            }
            else
            {
                string entityType = _nameEntryMode == 1 ? "Campaign" : "Mission";
                MessageBox.Show($"{entityType} name cannot be empty.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NameEntryInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveName_Click(sender, new RoutedEventArgs());
            }
        }

        // --- KANBAN BOARD INTERACTIONS ---
        private void ShowTaskPopup_Click(object sender, RoutedEventArgs e)
        {
            if (BoardFocusComboBox.SelectedItem is Mission)
            {
                TaskInput.Clear();
                TaskDatePicker.SelectedDate = null;
                TaskPopup.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Please create and select a Mission from the dropdown before adding Objectives.", "Execution Phase");
            }
        }

        private void CloseTaskPopup_Click(object sender, RoutedEventArgs e) => TaskPopup.Visibility = Visibility.Collapsed;

        private void AddObjective_Click(object sender, RoutedEventArgs e)
        {
            if (BoardFocusComboBox.SelectedItem is Mission currentMission && PriorityComboBox.SelectedItem is PriorityItem selectedPriority)
            {
                if (!string.IsNullOrWhiteSpace(TaskInput.Text))
                {
                    _questManager.AddObjective(currentMission, TaskInput.Text, selectedPriority, TaskDatePicker.SelectedDate);

                    RefreshKanbanBoard();
                    TaskPopup.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Objective description cannot be empty.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void TaskInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddObjective_Click(sender, new RoutedEventArgs());
            }
        }

        // Move Card: To Do -> Doing
        private void StartQuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Objective obj)
            {
                _questManager.UpdateObjectiveStatus(obj, 1);
                RefreshKanbanBoard();
            }
        }

        // Move Card: Doing -> Done
        private void CompleteQuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Objective obj)
            {
                _questManager.UpdateObjectiveStatus(obj, 2);
                RefreshKanbanBoard();
            }
        }

        // Undo Card: Doing -> To Do
        private void UndoActiveQuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Objective obj)
            {
                _questManager.UpdateObjectiveStatus(obj, 0);
                RefreshKanbanBoard();
            }
        }

        // Undo Card: Done -> Doing
        private void UndoDoneQuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Objective obj)
            {
                _questManager.UpdateObjectiveStatus(obj, 1);
                RefreshKanbanBoard();
            }
        }

        // Delete Card from Done
        private void DeleteObjective_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Objective obj && BoardFocusComboBox.SelectedItem is Mission currentMission)
            {
                _questManager.DeleteObjective(currentMission, obj);
                RefreshKanbanBoard();
            }
        }
    }
}