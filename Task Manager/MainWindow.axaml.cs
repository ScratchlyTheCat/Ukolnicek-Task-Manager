using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;
using System;

namespace Task_Manager.Views;

public partial class MainView : UserControl 
{

    private ObservableCollection<StackPanel> _tasks = new ObservableCollection<StackPanel>();
    private List<string> _notifications = new List<string>();

    public MainView()
    {
        InitializeComponent();

        TasksListBox.ItemsSource = _tasks;
        
        UpdateInfoTextVisibility();
    }

    private void VytvoritUkol_OnClick(object? sender, RoutedEventArgs e)
    {
        string taskName = TasksNameBox.Text;

        string selectedColor = "Black";

        if (ColorComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            selectedColor = selectedItem.Content.ToString();
        }

        IImmutableSolidColorBrush brush = selectedColor switch
        {
            "Blue" => Brushes.Blue,
            "Red" => Brushes.Red,
            "Green" => Brushes.Green,
            "Yellow" => Brushes.Yellow,
            _ => Brushes.Black
        };

        string dueDateText = "";

        if (DueDatePicker.SelectedDate.HasValue)
        {
            dueDateText = DueDatePicker.SelectedDate.Value.ToString("dd.MM.yyyy");
        }

        TextBlock taskText = new TextBlock
        {
            Text = $"{taskName} | {dueDateText}",
            Foreground = brush,
            FontSize = 18,
            Width = 250
        };

        Button doneButton = new Button
        {
            Content = "Hotovo",
            Width = 100
        };

        doneButton.Click += (s, e) =>
        {
            taskText.Text = "✓ " + taskText.Text;
            taskText.Foreground = Brushes.Gray;
            doneButton.IsEnabled = false;
        };

        StackPanel taskRow = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 10
        };

        taskRow.Children.Add(taskText);
        taskRow.Children.Add(doneButton);

        _tasks.Add(taskRow);

        _notifications.Add($"Úkol ˇ{taskName}ˇ má být hotový do {dueDateText}");

        UpdateInfoTextVisibility();

        
    }
    
    private void ExportToTxt_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            string desktopPath =
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string filePath =
                Path.Combine(desktopPath, "tasks_export.txt");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var task in _tasks)
                {
                    if (task.Children[0] is TextBlock text)
                    {
                        writer.WriteLine(text.Text);
                    }
                }
            }

            TasksInfoText.Text = $"Export hotov: {filePath}";
            TasksInfoText.IsVisible = true;
        }
        catch (Exception ex)
        {
            TasksInfoText.Text = "Chyba exportu: " + ex.Message;
            TasksInfoText.IsVisible = true;
        }
    }
    
    
    
    private void ToggleTheme_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current.RequestedThemeVariant == ThemeVariant.Dark)
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        }
        else
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
        }
    }
    
    private void ShowNotifications_OnClick(object? sender, RoutedEventArgs e)
    {
        string message;

        if (_notifications.Count == 0)
        {
            message = "Žádné notifikace.";
        }
        else
        {
            message = string.Join("\n", _notifications);
        }

        Window notificationWindow = new Window
        {
            Title = "Notifikace",
            Width = 400,
            Height = 300,
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = message,
                    Margin = new Avalonia.Thickness(20),
                    FontSize = 18,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }
        };

        notificationWindow.Show();
    }
    private void UpdateInfoTextVisibility()
    {
        TasksInfoText.IsVisible = _tasks.Count == 0;
    }
}
