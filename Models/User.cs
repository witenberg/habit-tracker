using System.Collections.Generic;

namespace HabitTracker.Models
{
    /// <summary>
    /// Reprezentuje użytkownika aplikacji z listą jego nawyków
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<Habit> Habits { get; set; } = new List<Habit>();
    }
}
