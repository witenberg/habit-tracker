using System;

namespace HabitTracker.Models
{
    /// <summary>
    /// Reprezentuje pojedynczy wpis w historii nawyku
    /// </summary>
    public class HabitEntry
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool IsTargetMet { get; set; }
    }
}
