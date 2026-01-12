using System;
using System.Windows;
using System.Windows.Controls;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker
{
    /// <summary>
    /// Interaction logic for StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        private readonly Habit _habit;
        private readonly StatsEngine _statsEngine;

        public StatsWindow(Habit habit)
        {
            InitializeComponent();
            _habit = habit ?? throw new ArgumentNullException(nameof(habit));
            _statsEngine = new StatsEngine();
            
            LoadStats();
        }

        private void LoadStats()
        {
            // Ustaw nazwę i opis nawyku
            HabitNameTextBlock.Text = _habit.Name;
            HabitDescriptionTextBlock.Text = string.IsNullOrWhiteSpace(_habit.Description) 
                ? "(Brak opisu)" 
                : _habit.Description;

            // Wyczyść panel statystyk
            StatsPanel.Children.Clear();

            // Obecna passa
            int currentStreak = _statsEngine.GetCurrentStreak(_habit);
            AddStatistic("Obecna passa", $"{currentStreak} dni");

            // Najdłuższa passa
            int longestStreak = _statsEngine.GetLongestStreak(_habit);
            AddStatistic("Najdłuższa passa", $"{longestStreak} dni");

            // Całkowita liczba dni z ukończonym nawykiem
            int totalCompleted = _statsEngine.GetTotalCompletedDays(_habit);
            AddStatistic("Całkowita liczba dni z ukończonym nawykiem", $"{totalCompleted} dni");

            // Procent wykonania w tym tygodniu
            double weekPercentage = _statsEngine.GetCompletionPercentageThisWeek(_habit);
            AddStatistic("Wykonanie w tym tygodniu", $"{weekPercentage:F1}%");

            // Procent wykonania w tym miesiącu
            double monthPercentage = _statsEngine.GetCompletionPercentageThisMonth(_habit);
            AddStatistic("Wykonanie w tym miesiącu", $"{monthPercentage:F1}%");

            // Procent wykonania w ostatnich 30 dniach
            double last30Days = _statsEngine.GetCompletionPercentageLastDays(_habit, 30);
            AddStatistic("Wykonanie w ostatnich 30 dniach", $"{last30Days:F1}%");

            // Dodatkowe statystyki dla QuantitativeHabit
            if (_habit is QuantitativeHabit quantitativeHabit)
            {
                AddStatistic("Wartość docelowa", $"{quantitativeHabit.TargetValue} {quantitativeHabit.Unit}");

                // Średnia wartość w ostatnich 30 dniach
                var avgValue = _statsEngine.GetAverageValue(_habit, DateTime.Today.AddDays(-30), DateTime.Today);
                if (avgValue.HasValue)
                {
                    AddStatistic("Średnia wartość (ostatnie 30 dni)", $"{avgValue.Value:F2} {quantitativeHabit.Unit}");
                }
            }

            // Data utworzenia
            AddStatistic("Data utworzenia", _habit.CreatedDate.ToString("dd.MM.yyyy"));

            // Liczba wszystkich wpisów
            AddStatistic("Liczba wpisów w historii", _habit.History.Count.ToString());
        }

        private void AddStatistic(string label, string value)
        {
            var border = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 15),
                Background = System.Windows.Media.Brushes.WhiteSmoke
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var labelText = new TextBlock
            {
                Text = label + ":",
                FontWeight = FontWeights.SemiBold,
                Width = 250,
                VerticalAlignment = VerticalAlignment.Center
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(labelText);
            stackPanel.Children.Add(valueText);

            border.Child = stackPanel;
            StatsPanel.Children.Add(border);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
