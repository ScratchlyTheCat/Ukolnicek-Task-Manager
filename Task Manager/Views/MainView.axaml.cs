using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Task_Manager.AppData;
using Task_Manager.Models;

namespace Task_Manager.Views;

public partial class MainView : UserControl
{
    private ObservableCollection<StackPanel> _renderedTasks = new ObservableCollection<StackPanel>();
    private DispatcherTimer _autosaveTimer;
    private DispatcherTimer _toastDismissTimer;

    public MainView()
    {
        InitializeComponent();
        
        TasksListBox.ItemsSource = _renderedTasks;
        HistoryListBox.ItemsSource = TaskManagerData.ActivityHistory;
        NotificationsListBox.ItemsSource = TaskManagerData.Notifications;
        
        PriorityComboBox.ItemsSource = Enum.GetValues(typeof(TaskPriority));
        PriorityComboBox.SelectedIndex = 1;
        FilterComboBox.SelectedIndex = 0;
        
        UpdateInfoTextVisibility();

        _autosaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        _autosaveTimer.Tick += (s, e) => { TaskManagerData.ExecuteSave(isAutosave: true); };
        _autosaveTimer.Start();

        _toastDismissTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _toastDismissTimer.Tick += (s, e) => { NotificationToast.IsVisible = false; _toastDismissTimer.Stop(); };
    }

    private void TriggerToastNotification(string text)
    {
        NotificationToastText.Text = text;
        NotificationToast.IsVisible = true;
        _toastDismissTimer.Stop();
        _toastDismissTimer.Start();
    }

    private void DismissNotification_OnClick(object? sender, RoutedEventArgs e)
    {
        NotificationToast.IsVisible = false;
        _toastDismissTimer.Stop();
    }

    private void VytvoritUkol_OnClick(object? sender, RoutedEventArgs e) //tvoříme úkol
    {
        string taskName;

        if (TasksNameBox.Text != null)//získáme info z textboxu a přidáme ho následně sem
        {
            taskName = TasksNameBox.Text;
        }
        else
        {
            taskName = "";
        }
        
        if (string.IsNullOrWhiteSpace(taskName)) 
        {
            return;
        }

        string selectedColor = "Black";
        if (ColorComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null) //kontroluje jestli je vybraná barva, jinak je černá
        {
            if (selectedItem.Content != null)
            {
                selectedColor = selectedItem.Content.ToString();
            }
            else
            {
                selectedColor = "Black";
            }
        }

        TaskPriority priority = TaskPriority.Střední;
        if (PriorityComboBox.SelectedItem is TaskPriority selectedPriority) // kontroluje vybranou prioritu
        {
            priority = selectedPriority;
        }

        string dueDateText;

        if (DueDatePicker.SelectedDate.HasValue)
        {
            dueDateText = DueDatePicker.SelectedDate.Value.ToString("dd.MM.yyyy");
        }
        else
        {
            dueDateText = "Bez termínu";
        }

        var newTask = new TaskItem //vytváří úkol s těmito parametry
        {
            Name = taskName,
            DueDate = dueDateText,
            Color = selectedColor,
            Priority = priority,
            IsCompleted = false
        };

        TaskManagerData.AllTasks.Add(newTask);
        
        string inlineAlert = TaskManagerData.GenerateDueDateNotification(taskName, DueDatePicker.SelectedDate, dueDateText);
        TriggerToastNotification(inlineAlert); 

        ApplyFilter();
        TasksNameBox.Text = string.Empty;
    }

    private void RenderTaskList(List<TaskItem> tasksToRender) //vykreslujeme list úloh
    {
        _renderedTasks.Clear();

        foreach (var task in tasksToRender) //vybereme barvu
        {
            IImmutableSolidColorBrush brush;

            switch (task.Color)
            {
                case "Blue":
                    brush = Brushes.Blue;
                    break;
        
                case "Red":
                    brush = Brushes.Red;
                    break;
        
                case "Green":
                    brush = Brushes.Green;
                    break;
        
                case "Yellow":
                    brush = Brushes.Yellow;
                    break;
        
                default:
                    brush = Brushes.Black;
                    break;
            }

            if (task.IsCompleted) brush = Brushes.Gray;

            string statusPrefix;

            if (task.IsCompleted == true)
            {
                statusPrefix = "✓ ";
            }
            else
            {
                statusPrefix = "";
            }
            TextBlock taskText = new TextBlock //vykreslíme podle následujících parametrů
            {
                Text = $"{statusPrefix}{task.Name} | {task.DueDate} [{task.Priority}]",
                Foreground = brush,
                FontSize = 18,
                Width = 280,
                TextWrapping = TextWrapping.Wrap
            };

            Button doneButton = new Button { Content = "Hotovo", Width = 80, IsEnabled = !task.IsCompleted };

            var currentTask = task;
            doneButton.Click += (s, ev) =>
            {
                currentTask.IsCompleted = true;
                string finishMsg = $"Úkol '{currentTask.Name}' byl dokončen.";
                TaskManagerData.AddNotification(finishMsg);
                TriggerToastNotification(finishMsg);
                ApplyFilter();
            };

            StackPanel taskRow = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10, Margin = new Thickness(0, 2)
            };
            taskRow.Children.Add(taskText);
            taskRow.Children.Add(doneButton);
            _renderedTasks.Add(taskRow);
        }
        UpdateInfoTextVisibility();
    }

    private void FilterComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (FilterComboBox == null)
        {
            return;
        }

        List<TaskItem> filtered;

        switch (FilterComboBox.SelectedIndex)
        {
            case 1:
                //ukaž nehotové úkoly
                filtered = TaskManagerData.AllTasks.Where(t => !t.IsCompleted).ToList();
                break;

            case 2:
                //ukaž hotové úkol
                filtered = TaskManagerData.AllTasks.Where(t => t.IsCompleted).ToList();
                break;

            default:
                //ukaž všechny úkoly
                filtered = TaskManagerData.AllTasks.ToList();
                break;
        }

        RenderTaskList(filtered);
    }
    
    private void ExportToTxt_OnClick(object? sender, RoutedEventArgs e) //po kliknutí exportuj data
    {
        string resultMessage = TaskManagerData.ExecuteSave(isAutosave: false);
        TriggerToastNotification("Export byl dokončen.");
        TasksInfoText.Text = resultMessage;
        TasksInfoText.IsVisible = true;
    }

    private void LoadFromTxt_OnClick(object? sender, RoutedEventArgs e) //po kliknutí načti data
    {
        string resultMessage = TaskManagerData.LoadFromTxt();
        ApplyFilter();
        TriggerToastNotification("Záloha úkolů byla načtena!");
        TasksInfoText.Text = resultMessage;
        TasksInfoText.IsVisible = true;
    }
    
    private void ToggleTheme_OnClick(object? sender, RoutedEventArgs e) // po kliknutí změň barvu okna
    {
        if (Application.Current != null)
        {
            bool isDarkNow = Application.Current.RequestedThemeVariant == ThemeVariant.Dark;
            if (isDarkNow == true)
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                TaskManagerData.LogActivity("Vzhled přepnut na Světlý režim.");
            }
            else
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                TaskManagerData.LogActivity("Vzhled přepnut na Temný režim.");
            }
        }
    }

    private void UpdateInfoTextVisibility() // aktualizuj viditelnost textu
    {
        if (TasksInfoText != null)
        {
            TasksInfoText.IsVisible = _renderedTasks.Count == 0;
        }
    }
}