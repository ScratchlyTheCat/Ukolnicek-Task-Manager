using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Collections.ObjectModel;

namespace Task_Manager.Views;

public partial class MainView : UserControl 
{

    private ObservableCollection<TextBlock> _tasks = new ObservableCollection<TextBlock>();

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

        TextBlock newTask = new TextBlock
        {
            Text = $"{taskName} {_tasks.Count + 1}",
            Foreground = brush,
            FontSize = 18
        };

        _tasks.Add(newTask);

        UpdateInfoTextVisibility();
    }

    private void UpdateInfoTextVisibility()
    {
        TasksInfoText.IsVisible = _tasks.Count == 0;
    }
}
