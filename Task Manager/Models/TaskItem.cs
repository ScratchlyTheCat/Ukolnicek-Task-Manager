namespace Task_Manager.Models;

public enum TaskPriority //priorita úkolu
{
    Nízká,
    Střední,
    Vysoká
}

public class TaskItem //definujeme úkol a jeho parametry
{
    public string Name { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string Color { get; set; } = "Black";
    public bool IsCompleted { get; set; } = false;
    public TaskPriority Priority { get; set; } = TaskPriority.Střední;
}