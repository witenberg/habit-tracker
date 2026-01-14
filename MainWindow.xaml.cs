using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly HabitManager _habitManager;
        private DateTime _selectedDate = DateTime.Today;
        private ObservableCollection<Habit> _dailyHabits = new ObservableCollection<Habit>();

        public MainWindow(HabitManager habitManager)
        {
            InitializeComponent();
            _habitManager = habitManager ?? throw new ArgumentNullException(nameof(habitManager));
            
            DataContext = this;
            SelectedDate = DateTime.Today;
            DatePicker.SelectedDate = SelectedDate;
            UpdateCurrentUserDisplay();
            LoadDailyHabits();
            
            // Sprawdź czy użytkownik ma 0 nawyków i pokaż okno z predefiniowanymi nawykami
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Sprawdź czy użytkownik ma 0 nawyków
            var allHabits = _habitManager.GetAllHabits();
            if (allHabits.Count == 0)
            {
                ShowPredefinedHabitsWindow();
            }
        }

        private void ShowPredefinedHabitsWindow()
        {
            try
            {
                var predefinedHabitsWindow = new PredefinedHabitsWindow(_habitManager)
                {
                    Owner = this
                };
                
                if (predefinedHabitsWindow.ShowDialog() == true)
                {
                    // Odśwież listę nawyków po dodaniu predefiniowanych
                    LoadDailyHabits();
                    UpdateStatus("Dodano wybrane predefiniowane nawyki.");
                }
                else
                {
                    UpdateStatus("Pominięto wybór predefiniowanych nawyków.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas otwierania okna predefiniowanych nawyków: {ex.Message}\n\nSzczegóły: {ex}", 
                    "Błąd", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void UpdateCurrentUserDisplay()
        {
            var currentUser = _habitManager.GetCurrentUser();
            if (currentUser != null)
            {
                CurrentUserTextBlock.Text = $"Zalogowany jako: {currentUser.Username}";
            }
            else
            {
                CurrentUserTextBlock.Text = "";
            }
        }

        private void DatePicker_SelectedDateChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DatePicker.SelectedDate.HasValue)
            {
                SelectedDate = DatePicker.SelectedDate.Value;
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                LoadDailyHabits();
            }
        }

        public ObservableCollection<Habit> DailyHabits
        {
            get => _dailyHabits;
            set
            {
                _dailyHabits = value;
                OnPropertyChanged(nameof(DailyHabits));
            }
        }

        private void LoadDailyHabits()
        {
            // Pobierz wszystkie nawyki użytkownika
            var allHabits = _habitManager.GetAllHabits();
            DailyHabits.Clear();
            
            // Filtruj nawyki - pokazuj tylko te, które zostały utworzone w dniu wybranym lub wcześniej
            var selectedDateOnly = SelectedDate.Date;
            foreach (var habit in allHabits)
            {
                // Jeśli data utworzenia nawyku jest wcześniejsza lub równa wybranej dacie, pokaż nawyk
                if (habit.CreatedDate.Date <= selectedDateOnly)
                {
                    DailyHabits.Add(habit);
                }
            }

            // Odśwież ListView, aby zaktualizować wyświetlane wartości
            HabitsListView.Items.Refresh();

            UpdateStatus($"Wyświetlanie {DailyHabits.Count} nawyków dla {SelectedDate:dd.MM.yyyy}");
        }

        private void AddHabitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addHabitWindow = new AddHabitWindow(_habitManager)
                {
                    Owner = this
                };
                if (addHabitWindow.ShowDialog() == true)
                {
                    LoadDailyHabits();
                    UpdateStatus("Nawyk został dodany pomyślnie.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas otwierania okna: {ex.Message}\n\nSzczegóły: {ex}", 
                    "Błąd", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedHabit = HabitsListView.SelectedItem as Habit;
            if (selectedHabit == null)
            {
                MessageBox.Show("Proszę wybrać nawyk z listy, aby zobaczyć statystyki.", 
                    "Brak wybranego nawyku", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                return;
            }

            var statsWindow = new StatsWindow(selectedHabit);
            statsWindow.ShowDialog();
        }

        private void BooleanHabitButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BooleanHabit habit)
            {
                try
                {
                    // Sprawdź, czy nawyk jest już wykonany w wybranym dniu
                    var existingEntry = habit.History.FirstOrDefault(h => h.Date.Date == SelectedDate.Date);
                    
                    if (existingEntry != null && existingEntry.Value >= 1.0)
                    {
                        // Cofnij wykonanie - zapisz wartość 0
                        _habitManager.LogProgress(habit.Id, SelectedDate, 0.0, "");
                        button.Content = "✓ Zrobione";
                        button.Background = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#4CAF50"));
                        UpdateStatus($"Cofnięto wykonanie nawyku '{habit.Name}'.");
                        
                        // Znajdź Border i zmień tło na domyślne
                        var border = FindVisualParent<Border>(button);
                        if (border != null)
                        {
                            border.Background = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#F5F5F5"));
                        }
                    }
                    else
                    {
                        // Oznacz jako wykonane
                        var entry = _habitManager.LogProgress(habit.Id, SelectedDate, 1.0, "Zrobione");
                        if (entry != null)
                        {
                            button.Content = "↶ Cofnij";
                            button.Background = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#FF9800"));
                            UpdateStatus($"Nawyk '{habit.Name}' oznaczony jako wykonany.");
                            
                            // Znajdź Border i zmień tło na zielone
                            var border = FindVisualParent<Border>(button);
                            if (border != null)
                            {
                                border.Background = new SolidColorBrush(
                                    (Color)ColorConverter.ConvertFromString("#C8E6C9"));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zapisywania: {ex.Message}", 
                        "Błąd", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
        }

        private void BooleanHabitButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BooleanHabit habit)
            {
                var entry = habit.History.FirstOrDefault(h => h.Date.Date == SelectedDate.Date);
                if (entry != null && entry.Value >= 1.0)
                {
                    button.Content = "↶ Cofnij";
                    button.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FF9800"));
                }
                else
                {
                    button.Content = "✓ Zrobione";
                    button.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#4CAF50"));
                }
            }
        }

        private void BooleanHabitBorder_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is BooleanHabit habit)
            {
                var entry = habit.History.FirstOrDefault(h => h.Date.Date == SelectedDate.Date);
                if (entry != null && entry.Value >= 1.0)
                {
                    border.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#C8E6C9"));
                }
                else
                {
                    border.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#F5F5F5"));
                }
            }
        }

        private void QuantitativeHabitButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TextBox textBox)
            {
                if (textBox.Tag is QuantitativeHabit habit)
                {
                    // Sprawdź, czy jesteśmy w trybie edycji czy zapisu
                    if (button.Content.ToString() == "Edytuj")
                    {
                        // Włącz tryb edycji
                        textBox.IsReadOnly = false;
                        button.Content = "Zapisz";
                        button.Background = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#4CAF50"));
                        UpdateStatus($"Tryb edycji dla '{habit.Name}'.");
                        return;
                    }

                    // Tryb zapisu
                    if (double.TryParse(textBox.Text, out double value))
                    {
                        var (isValid, errorMessage) = InputValidator.ValidateEntryValue(value);
                        if (!isValid)
                        {
                            MessageBox.Show(errorMessage, "Błąd walidacji", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        try
                        {
                            var entry = _habitManager.LogProgress(habit.Id, SelectedDate, value, "");
                            if (entry != null)
                            {
                                textBox.IsReadOnly = true;
                                button.Content = "Edytuj";
                                button.Background = new SolidColorBrush(
                                    (Color)ColorConverter.ConvertFromString("#2196F3"));
                                UpdateStatus($"Wartość {value} {habit.Unit} zapisana dla '{habit.Name}'.");
                                
                                // Sprawdź, czy cel został osiągnięty i zmień tło
                                var border = FindVisualParent<Border>(button);
                                if (border != null)
                                {
                                    if (value >= habit.TargetValue)
                                    {
                                        border.Background = new SolidColorBrush(
                                            (Color)ColorConverter.ConvertFromString("#C8E6C9"));
                                    }
                                    else
                                    {
                                        border.Background = new SolidColorBrush(
                                            (Color)ColorConverter.ConvertFromString("#F5F5F5"));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Błąd podczas zapisywania: {ex.Message}", 
                                "Błąd", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Proszę wprowadzić poprawną wartość liczbową.", 
                            "Błąd", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void QuantitativeHabitButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TextBox textBox)
            {
                if (textBox.Tag is QuantitativeHabit habit)
                {
                    var entry = habit.History.FirstOrDefault(h => h.Date.Date == SelectedDate.Date);
                    if (entry != null)
                    {
                        // Istnieje wpis - tryb edycji
                        textBox.IsReadOnly = true;
                        button.Content = "Edytuj";
                        button.Background = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#2196F3"));
                    }
                    else
                    {
                        // Brak wpisu - tryb zapisu
                        textBox.IsReadOnly = false;
                        button.Content = "Zapisz";
                        button.Background = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#4CAF50"));
                    }
                }
            }
        }

        private void QuantitativeHabitBorder_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is QuantitativeHabit habit)
            {
                var entry = habit.History.FirstOrDefault(h => h.Date.Date == SelectedDate.Date);
                if (entry != null && entry.Value >= habit.TargetValue)
                {
                    border.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#C8E6C9"));
                }
                else
                {
                    border.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#F5F5F5"));
                }
            }
        }

        private void ValueTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is QuantitativeHabit habit)
            {
                // Znajdź wpis dla wybranego dnia
                var entry = habit.History.FirstOrDefault(h => h.Date.Date == SelectedDate.Date);
                if (entry != null)
                {
                    textBox.Text = entry.Value.ToString("F2");
                }
                else
                {
                    textBox.Text = "0";
                }
            }
        }

        private void UpdateStatus(string message)
        {
            StatusTextBlock.Text = message;
        }

        private T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;

            return FindVisualParent<T>(parentObject);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Czy na pewno chcesz się wylogować?",
                "Wylogowanie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _habitManager.Logout();
                this.Close();
            }
        }
    }
}
