using System;
using System.Collections.Generic;
using System.Linq;

namespace HabitTracker.Models
{
    public abstract class Habit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public List<HabitEntry> History { get; set; } = new List<HabitEntry>();

        /// <summary>
        /// Sprawdza, czy nawyk został ukończony na podstawie wartości
        /// </summary>
        /// <param name="value">Wartość do sprawdzenia</param>
        /// <returns>True, jeśli nawyk jest ukończony</returns>
        public abstract bool IsCompleted(double value);

        /// <summary>
        /// Oblicza aktualną serię (streak) - liczbę kolejnych dni z ukończonym nawykiem
        /// </summary>
        /// <returns>Liczba dni w serii</returns>
        public virtual int GetStreak()
        {
            if (History == null || History.Count == 0)
                return 0;

            // Sortuj wpisy po dacie (od najnowszych)
            var sortedEntries = History
                .Where(e => e.IsTargetMet)
                .OrderByDescending(e => e.Date)
                .ToList();

            if (sortedEntries.Count == 0)
                return 0;

            int streak = 0;
            DateTime? lastDate = null;

            foreach (var entry in sortedEntries)
            {
                if (lastDate == null)
                {
                    // Pierwszy wpis - sprawdź czy jest z dzisiaj lub wczoraj
                    var daysDiff = (DateTime.Today - entry.Date.Date).Days;
                    if (daysDiff <= 1)
                    {
                        streak = 1;
                        lastDate = entry.Date.Date;
                    }
                    else
                    {
                        // Jeśli pierwszy wpis jest starszy niż wczoraj, nie ma serii
                        break;
                    }
                }
                else
                {
                    // Sprawdź czy wpisy są kolejne (różnica 1 dzień)
                    var daysDiff = (lastDate.Value - entry.Date.Date).Days;
                    if (daysDiff == 1)
                    {
                        streak++;
                        lastDate = entry.Date.Date;
                    }
                    else
                    {
                        // Przerwa w serii
                        break;
                    }
                }
            }

            return streak;
        }
    }
}
