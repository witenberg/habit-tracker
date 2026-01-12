namespace HabitTracker.Models
{
    /// <summary>
    /// Reprezentuje nawyk typu Tak/Nie (Boolean)
    /// </summary>
    public class BooleanHabit : Habit
    {
        /// <summary>
        /// Dla nawyku Boolean, wartość 1.0 oznacza "Tak" (ukończone), 0.0 oznacza "Nie"
        /// </summary>
        public override bool IsCompleted(double value)
        {
            return value >= 1.0;
        }
    }
}
