using System;
using System.Windows;
using HabitTracker.Services;

namespace HabitTracker
{
    /// <summary>
    /// Interaction logic for AddHabitWindow.xaml
    /// </summary>
    public partial class AddHabitWindow : Window
    {
        private readonly HabitManager _habitManager;

        public AddHabitWindow(HabitManager habitManager)
        {
            try
            {
                InitializeComponent();
                _habitManager = habitManager ?? throw new ArgumentNullException(nameof(habitManager));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas inicjalizacji okna: {ex.Message}\n\n{ex}", 
                    "Błąd", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                throw;
            }
        }

        private void HabitTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Sprawdź, czy QuantitativePanel został już zainicjalizowany
            if (QuantitativePanel == null)
                return;

            if (HabitTypeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                bool isQuantitative = item.Tag?.ToString() == "Quantitative";
                QuantitativePanel.Visibility = isQuantitative ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = NameTextBox.Text?.Trim() ?? "";
                string description = DescriptionTextBox.Text?.Trim() ?? "";

                if (HabitTypeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                {
                    string habitType = selectedItem.Tag?.ToString() ?? "Boolean";

                    if (habitType == "Boolean")
                    {
                        // Walidacja nawyku Boolean
                        var (isValid, errorMessage) = InputValidator.ValidateBooleanHabit(name, description);
                        if (!isValid)
                        {
                            MessageBox.Show(errorMessage, "Błąd walidacji", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Utwórz nawyk Boolean
                        _habitManager.CreateHabit(name, description, isBoolean: true);
                    }
                    else // Quantitative
                    {
                        // Walidacja wartości docelowej
                        if (!double.TryParse(TargetValueTextBox.Text, out double targetValue))
                        {
                            MessageBox.Show("Wartość docelowa musi być liczbą.", "Błąd walidacji", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string unit = UnitTextBox.Text?.Trim() ?? "";

                        // Walidacja nawyku ilościowego
                        var (isValid, errorMessage) = InputValidator.ValidateQuantitativeHabit(name, targetValue, unit, description);
                        if (!isValid)
                        {
                            MessageBox.Show(errorMessage, "Błąd walidacji", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Utwórz nawyk ilościowy
                        _habitManager.CreateHabit(name, description, isBoolean: false, targetValue, unit);
                    }

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas tworzenia nawyku: {ex.Message}", 
                    "Błąd", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
