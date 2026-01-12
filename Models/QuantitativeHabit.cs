namespace HabitTracker.Models
{
    /// <summary>
    /// Reprezentuje nawyk ilościowy z wartością docelową
    /// </summary>
    public class QuantitativeHabit : Habit
    {
        public double TargetValue { get; set; }
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Sprawdza, czy wartość osiągnęła lub przekroczyła wartość docelową
        /// </summary>
        public override bool IsCompleted(double value)
        {
            return value >= TargetValue;
        }
    }
}
