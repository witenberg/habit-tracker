using System;
using System.Linq;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    /// <summary>
    /// Klasa do obliczania statystyk nawyków
    /// </summary>
    public class StatsEngine
    {
        /// <summary>
        /// Oblicza obecną passę (streak) dla nawyku
        /// </summary>
        /// <param name="habit">Nawyk do analizy</param>
        /// <returns>Liczba dni w obecnej serii</returns>
        public int GetCurrentStreak(Habit habit)
        {
            if (habit == null)
                throw new ArgumentNullException(nameof(habit));

            return habit.GetStreak();
        }

        /// <summary>
        /// Oblicza procent wykonania nawyku w zadanym okresie
        /// </summary>
        /// <param name="habit">Nawyk do analizy</param>
        /// <param name="startDate">Data początkowa okresu</param>
        /// <param name="endDate">Data końcowa okresu</param>
        /// <returns>Procent wykonania (0-100)</returns>
        public double GetCompletionPercentage(Habit habit, DateTime startDate, DateTime endDate)
        {
            if (habit == null)
                throw new ArgumentNullException(nameof(habit));

            if (startDate > endDate)
                throw new ArgumentException("Data początkowa nie może być późniejsza niż data końcowa");

            // Oblicz liczbę dni w okresie
            var totalDays = (endDate.Date - startDate.Date).Days + 1;

            if (totalDays <= 0)
                return 0;

            // Policz dni, w których cel został osiągnięty
            var completedDays = habit.History
                .Where(e => e.Date.Date >= startDate.Date && e.Date.Date <= endDate.Date)
                .Count(e => e.IsTargetMet);

            return (double)completedDays / totalDays * 100.0;
        }

        /// <summary>
        /// Oblicza procent wykonania nawyku w ostatnich N dniach
        /// </summary>
        /// <param name="habit">Nawyk do analizy</param>
        /// <param name="days">Liczba ostatnich dni</param>
        /// <returns>Procent wykonania (0-100)</returns>
        public double GetCompletionPercentageLastDays(Habit habit, int days)
        {
            if (days <= 0)
                throw new ArgumentException("Liczba dni musi być większa od zera");

            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-(days - 1));

            return GetCompletionPercentage(habit, startDate, endDate);
        }

        /// <summary>
        /// Oblicza procent wykonania nawyku w bieżącym miesiącu
        /// </summary>
        /// <param name="habit">Nawyk do analizy</param>
        /// <returns>Procent wykonania (0-100)</returns>
        public double GetCompletionPercentageThisMonth(Habit habit)
        {
            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = today;

            return GetCompletionPercentage(habit, startDate, endDate);
        }

        /// <summary>
        /// Oblicza procent wykonania nawyku w bieżącym tygodniu
        /// </summary>
        /// <param name="habit">Nawyk do analizy</param>
        /// <returns>Procent wykonania (0-100)</returns>
        public double GetCompletionPercentageThisWeek(Habit habit)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endDate = today;

            return GetCompletionPercentage(habit, startOfWeek, endDate);
        }

        /// <summary>
        /// Oblicza średnią wartość dla nawyku ilościowego w zadanym okresie
        /// </summary>
        /// <param name="habit">Nawyk do analizy (musi być QuantitativeHabit)</param>
        /// <param name="startDate">Data początkowa okresu</param>
        /// <param name="endDate">Data końcowa okresu</param>
        /// <returns>Średnia wartość lub null, jeśli nie ma danych</returns>
        public double? GetAverageValue(Habit habit, DateTime startDate, DateTime endDate)
        {
            if (habit == null)
                throw new ArgumentNullException(nameof(habit));

            if (habit is not QuantitativeHabit)
                throw new ArgumentException("Nawyk musi być typu QuantitativeHabit");

            if (startDate > endDate)
                throw new ArgumentException("Data początkowa nie może być późniejsza niż data końcowa");

            var entries = habit.History
                .Where(e => e.Date.Date >= startDate.Date && e.Date.Date <= endDate.Date)
                .ToList();

            if (entries.Count == 0)
                return null;

            return entries.Average(e => e.Value);
        }

        /// <summary>
        /// Oblicza całkowitą liczbę dni z ukończonym nawykiem
        /// </summary>
        /// <param name="habit">Nawyk do analizy</param>
        /// <returns>Całkowita liczba dni z ukończonym nawykiem</returns>
        public int GetTotalCompletedDays(Habit habit)
        {
            if (habit == null)
                throw new ArgumentNullException(nameof(habit));

            return habit.History.Count(e => e.IsTargetMet);
        }

        /// <summary>
        /// Oblicza najdłuższą passę (streak) w historii nawyku
        /// </summary>
        /// <param name="habit">Nawyk do analizy</param>
        /// <returns>Najdłuższa seria dni z ukończonym nawykiem</returns>
        public int GetLongestStreak(Habit habit)
        {
            if (habit == null)
                throw new ArgumentNullException(nameof(habit));

            if (habit.History == null || habit.History.Count == 0)
                return 0;

            // Sortuj wpisy po dacie (od najstarszych)
            var sortedEntries = habit.History
                .Where(e => e.IsTargetMet)
                .OrderBy(e => e.Date)
                .ToList();

            if (sortedEntries.Count == 0)
                return 0;

            int longestStreak = 1;
            int currentStreak = 1;
            DateTime lastDate = sortedEntries[0].Date.Date;

            for (int i = 1; i < sortedEntries.Count; i++)
            {
                var daysDiff = (sortedEntries[i].Date.Date - lastDate).Days;
                
                if (daysDiff == 1)
                {
                    // Kolejny dzień - zwiększ serię
                    currentStreak++;
                }
                else if (daysDiff > 1)
                {
                    // Przerwa w serii - zaktualizuj najdłuższą serię i zacznij nową
                    longestStreak = Math.Max(longestStreak, currentStreak);
                    currentStreak = 1;
                }
                // Jeśli daysDiff == 0, to ten sam dzień - pomiń

                lastDate = sortedEntries[i].Date.Date;
            }

            // Sprawdź ostatnią serię
            longestStreak = Math.Max(longestStreak, currentStreak);

            return longestStreak;
        }
    }
}
