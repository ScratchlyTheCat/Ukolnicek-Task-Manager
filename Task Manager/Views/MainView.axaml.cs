using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;

namespace Task_Manager.Views;

public partial class MainView : UserControl 
{

    private ObservableCollection<string> _tasks = new ObservableCollection<string>();

    public MainView()
    {
        InitializeComponent();

        TasksListBox.ItemsSource = _tasks;
        
        UpdateInfoTextVisibility();
    }

    private void VytvoritUkol_OnClick(object? sender, RoutedEventArgs e)
    {
        string taskName = TasksNameBox.Text;
        _tasks.Add($"{taskName} {_tasks.Count + 1}");
        
        UpdateInfoTextVisibility();
    }

    private void UpdateInfoTextVisibility()
    {
        TasksInfoText.IsVisible = _tasks.Count == 0;
    }
}