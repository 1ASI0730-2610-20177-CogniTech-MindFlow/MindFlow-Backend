namespace Mindflow_backend.WellnessEngine.Application.Dtos;

public class StressCheckResultDto
{
    public string StressLevel { get; set; } = string.Empty;
    public int Score { get; set; }
    public int AnalyzedEntries { get; set; }
    public List<HabitAdjustmentDto> PausedHabits { get; set; } = [];
    public List<HabitAdjustmentDto> ResumedHabits { get; set; } = [];
    public string? Advice { get; set; }
}

public class HabitAdjustmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
