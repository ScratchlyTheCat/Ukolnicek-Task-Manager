using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Task_Manager.Models;

namespace Task_Manager.AppData;

public static class TaskManagerData
{
    public static List<TaskItem> AllTasks { get; } = new List<TaskItem>();
    public static ObservableCollection<string> ActivityHistory { get; } = new ObservableCollection<string>();
    public static ObservableCollection<string> Notifications { get; } = new ObservableCollection<string>();

    private static readonly string ExportFilePath = Path.Combine( // exportuje vše do txt souboru
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
        "tasks_export.txt"
    );

    static TaskManagerData()
    { //automaticky jsem přidal spuštění aplikace jako první záznam do historie
        LogActivity("Aplikace byla spuštěna.");
    }

    public static void LogActivity(string message) //přidáva co se děje do historie
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        ActivityHistory.Insert(0, $"[{timestamp}] {message}"); 
    }

    public static void AddNotification(string message) //přidává notifikaci
    {
        string timestamp = DateTime.Now.ToString("HH:mm");
        Notifications.Insert(0, $"[{timestamp}] {message}");
        LogActivity($"Notifikace: {message}");
    }

    
    public static string GenerateDueDateNotification(string taskName, DateTimeOffset? selectedDate, string dueDateText)
    {
        string notificationText;

        if (selectedDate.HasValue)
        {
            // zjistí jak blízko je úkol k termínu a přidá notifikaci
            int daysLeft = (selectedDate.Value.Date - DateTime.Today).Days;

            if (daysLeft > 0)
            {
                notificationText = $"Úkol '{taskName}' má být hotový do {dueDateText} (zbývá {daysLeft} dní)";
            }
            else if (daysLeft == 0)
            {
                notificationText = $"Úkol '{taskName}' má být hotový do {dueDateText} (termín je dnes)";
            }
            else
            {
                notificationText = $"Úkol '{taskName}' měl být hotový do {dueDateText} (po termínu o {Math.Abs(daysLeft)} dní)";
            }
        }
        else
        {
            notificationText = $"Úkol '{taskName}' nemá nastavený termín.";
        }

        AddNotification(notificationText);
        return notificationText;
    }

    public static string ExecuteSave(bool isAutosave)  //píše data úkolu do txt souboru
    {
        using (StreamWriter writer = new StreamWriter(ExportFilePath))
        {
            foreach (var task in AllTasks)
            {
                writer.WriteLine($"{task.Name};{task.DueDate};{task.Color};{task.IsCompleted};{task.Priority}");
            }
        }

        if (isAutosave)
        {
            LogActivity("Provedeno automatické uložení dat.");
            return "Autosave done";
        }
        
        AddNotification("Export byl dokončen.");
        return $"Export hotov: {ExportFilePath}";
    }

    public static string LoadFromTxt() //načítá z txt souboru
    {
        if (!File.Exists(ExportFilePath))
        {
            return "Exportovaný soubor na ploše nebyl nalezen.";
        }

        AllTasks.Clear();
        string[] lines = File.ReadAllLines(ExportFilePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            string[] parts = line.Split(';');
            if (parts.Length >= 5)
            {
                AllTasks.Add(new TaskItem
                {
                    Name = parts[0],
                    DueDate = parts[1],
                    Color = parts[2],
                    IsCompleted = bool.Parse(parts[3]),
                    Priority = Enum.Parse<TaskPriority>(parts[4])
                });
            }
        }

        LogActivity($"Načteno {lines.Length} záznamů ze záložního exportu.");
        return "Úkoly byly úspěšně načteny!";

    }
}