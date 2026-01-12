using System;

namespace HabitTracker.Services
{
    /// <summary>
    /// Klasa do walidacji danych wejściowych
    /// </summary>
    public static class InputValidator
    {
        /// <summary>
        /// Sprawdza, czy nazwa nawyku jest poprawna (nie jest pusta ani null)
        /// </summary>
        /// <param name="name">Nazwa nawyku do sprawdzenia</param>
        /// <returns>True, jeśli nazwa jest poprawna</returns>
        public static bool IsValidHabitName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        /// <summary>
        /// Sprawdza, czy wartość liczbowa jest dodatnia
        /// </summary>
        /// <param name="value">Wartość do sprawdzenia</param>
        /// <returns>True, jeśli wartość jest dodatnia</returns>
        public static bool IsPositiveValue(double value)
        {
            return value > 0;
        }

        /// <summary>
        /// Sprawdza, czy wartość liczbowa jest nieujemna (>= 0)
        /// </summary>
        /// <param name="value">Wartość do sprawdzenia</param>
        /// <returns>True, jeśli wartość jest nieujemna</returns>
        public static bool IsNonNegativeValue(double value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Waliduje dane nawyku Boolean
        /// </summary>
        /// <param name="name">Nazwa nawyku</param>
        /// <param name="description">Opis nawyku (opcjonalny)</param>
        /// <returns>Krotka (isValid, errorMessage)</returns>
        public static (bool isValid, string? errorMessage) ValidateBooleanHabit(string? name, string? description = null)
        {
            if (!IsValidHabitName(name))
            {
                return (false, "Nazwa nawyku nie może być pusta.");
            }

            return (true, null);
        }

        /// <summary>
        /// Waliduje dane nawyku ilościowego
        /// </summary>
        /// <param name="name">Nazwa nawyku</param>
        /// <param name="targetValue">Wartość docelowa</param>
        /// <param name="unit">Jednostka (opcjonalna)</param>
        /// <param name="description">Opis nawyku (opcjonalny)</param>
        /// <returns>Krotka (isValid, errorMessage)</returns>
        public static (bool isValid, string? errorMessage) ValidateQuantitativeHabit(
            string? name, 
            double targetValue, 
            string? unit = null, 
            string? description = null)
        {
            if (!IsValidHabitName(name))
            {
                return (false, "Nazwa nawyku nie może być pusta.");
            }

            if (!IsPositiveValue(targetValue))
            {
                return (false, "Wartość docelowa musi być liczbą dodatnią.");
            }

            return (true, null);
        }

        /// <summary>
        /// Waliduje wartość wpisu do historii nawyku
        /// </summary>
        /// <param name="value">Wartość wpisu</param>
        /// <returns>Krotka (isValid, errorMessage)</returns>
        public static (bool isValid, string? errorMessage) ValidateEntryValue(double value)
        {
            if (!IsNonNegativeValue(value))
            {
                return (false, "Wartość wpisu nie może być ujemna.");
            }

            return (true, null);
        }

        /// <summary>
        /// Waliduje datę wpisu (nie może być z przyszłości)
        /// </summary>
        /// <param name="date">Data do sprawdzenia</param>
        /// <returns>Krotka (isValid, errorMessage)</returns>
        public static (bool isValid, string? errorMessage) ValidateEntryDate(DateTime date)
        {
            if (date.Date > DateTime.Today)
            {
                return (false, "Data wpisu nie może być z przyszłości.");
            }

            return (true, null);
        }
    }
}
