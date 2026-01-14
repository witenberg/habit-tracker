using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker
{
    /// <summary>
    /// Interaction logic for PredefinedHabitsWindow.xaml
    /// </summary>
    public partial class PredefinedHabitsWindow : Window
    {
        private readonly HabitManager _habitManager;
        private readonly ObservableCollection<PredefinedHabitViewModel> _habits;

        public PredefinedHabitsWindow(HabitManager habitManager)
        {
            InitializeComponent();
            _habitManager = habitManager ?? throw new ArgumentNullException(nameof(habitManager));
            _habits = new ObservableCollection<PredefinedHabitViewModel>();

            LoadPredefinedHabits();
            HabitsItemsControl.ItemsSource = _habits;
        }

        private void LoadPredefinedHabits()
        {
            var predefinedHabits = PredefinedHabitsConfig.GetAllPredefinedHabits();
            foreach (var habit in predefinedHabits)
            {
                _habits.Add(new PredefinedHabitViewModel
                {
                    Name = habit.Name,
                    Description = habit.Description,
                    IsBoolean = habit.IsBoolean,
                    TargetValue = habit.TargetValue,
                    Unit = habit.Unit,
                    IsSelected = false,
                    TypeLabel = habit.IsBoolean ? "Tak/Nie" : "Ilościowy",
                    TargetLabel = habit.IsBoolean ? "-" : $"{habit.TargetValue} {habit.Unit}",
                    ShowDetails = habit.IsBoolean ? Visibility.Collapsed : Visibility.Visible
                });
            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var habit in _habits)
            {
                habit.IsSelected = true;
            }
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var habit in _habits)
            {
                habit.IsSelected = false;
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedHabits = _habits.Where(h => h.IsSelected).ToList();
            
            if (selectedHabits.Count == 0)
            {
                MessageBox.Show("Proszę wybrać przynajmniej jeden nawyk.", 
                    "Brak wyboru", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                foreach (var habitViewModel in selectedHabits)
                {
                    _habitManager.CreateHabit(
                        habitViewModel.Name,
                        habitViewModel.Description,
                        habitViewModel.IsBoolean,
                        habitViewModel.TargetValue,
                        habitViewModel.Unit
                    );
                }

                MessageBox.Show($"Pomyślnie dodano {selectedHabits.Count} nawyk(ów).", 
                    "Sukces", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania nawyków: {ex.Message}", 
                    "Błąd", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// ViewModel dla predefiniowanego nawyku w oknie wyboru
    /// </summary>
    public class PredefinedHabitViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsBoolean { get; set; }
        public double TargetValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string TypeLabel { get; set; } = string.Empty;
        public string TargetLabel { get; set; } = string.Empty;
        public Visibility ShowDetails { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
