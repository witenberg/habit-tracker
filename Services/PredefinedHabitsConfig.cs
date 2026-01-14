using System.Collections.Generic;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    /// <summary>
    /// Konfiguracja predefiniowanych nawyków dostępnych dla nowych użytkowników
    /// </summary>
    public static class PredefinedHabitsConfig
    {
        /// <summary>
        /// Pobiera listę wszystkich predefiniowanych nawyków
        /// </summary>
        public static List<PredefinedHabit> GetAllPredefinedHabits()
        {
            return new List<PredefinedHabit>
            {
                // Boolean habits
                new PredefinedHabit
                {
                    Name = "Picie wody",
                    Description = "Pamiętaj o regularnym piciu wody w ciągu dnia",
                    IsBoolean = true
                },
                new PredefinedHabit
                {
                    Name = "Ćwiczenia fizyczne",
                    Description = "Wykonaj trening lub aktywność fizyczną",
                    IsBoolean = true
                },
                new PredefinedHabit
                {
                    Name = "Czytanie książki",
                    Description = "Przeczytaj przynajmniej kilka stron książki",
                    IsBoolean = true
                },
                new PredefinedHabit
                {
                    Name = "Medytacja",
                    Description = "Poświęć czas na relaks i medytację",
                    IsBoolean = true
                },
                new PredefinedHabit
                {
                    Name = "Zdrowy sen",
                    Description = "Upewnij się, że śpisz odpowiednią ilość godzin",
                    IsBoolean = true
                },
                new PredefinedHabit
                {
                    Name = "Spacer na świeżym powietrzu",
                    Description = "Wyjdź na spacer i dotleń się świeżym powietrzem",
                    IsBoolean = true
                },
                
                // Quantitative habits
                new PredefinedHabit
                {
                    Name = "Ilość wypitej wody",
                    Description = "Śledź ile szklanek wody wypijasz dziennie",
                    IsBoolean = false,
                    TargetValue = 8,
                    Unit = "szklanki"
                },
                new PredefinedHabit
                {
                    Name = "Kroki dzienne",
                    Description = "Zliczaj liczbę kroków wykonanych w ciągu dnia",
                    IsBoolean = false,
                    TargetValue = 10000,
                    Unit = "kroków"
                },
                new PredefinedHabit
                {
                    Name = "Czas czytania",
                    Description = "Śledź ile minut dziennie poświęcasz na czytanie",
                    IsBoolean = false,
                    TargetValue = 30,
                    Unit = "minut"
                },
                new PredefinedHabit
                {
                    Name = "Czas ćwiczeń",
                    Description = "Zapisuj ile minut dziennie ćwiczyłeś",
                    IsBoolean = false,
                    TargetValue = 30,
                    Unit = "minut"
                },
                new PredefinedHabit
                {
                    Name = "Porcje warzyw i owoców",
                    Description = "Śledź ile porcji warzyw i owoców zjadasz dziennie",
                    IsBoolean = false,
                    TargetValue = 5,
                    Unit = "porcji"
                },
                new PredefinedHabit
                {
                    Name = "Czas spędzony na świeżym powietrzu",
                    Description = "Zapisuj ile minut dziennie spędzasz na zewnątrz",
                    IsBoolean = false,
                    TargetValue = 60,
                    Unit = "minut"
                }
            };
        }
    }

    /// <summary>
    /// Reprezentuje predefiniowany nawyk (szablon)
    /// </summary>
    public class PredefinedHabit
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsBoolean { get; set; }
        public double TargetValue { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
