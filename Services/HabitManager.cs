using System;
using System.Collections.Generic;
using System.Linq;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    /// <summary>
    /// Klasa zarządzająca nawykami użytkownika
    /// </summary>
    public class HabitManager
    {
        private readonly IDataService _dataService;
        private List<User> _users;
        private User? _currentUser;
        private int _nextHabitId = 1;
        private int _nextEntryId = 1;

        public HabitManager(IDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _users = _dataService.Load();
        }

        /// <summary>
        /// Loguje użytkownika na podstawie nazwy użytkownika i hasła
        /// </summary>
        /// <param name="username">Nazwa użytkownika</param>
        /// <param name="password">Hasło</param>
        /// <returns>True, jeśli logowanie się powiodło</returns>
        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                u.Password == password);

            if (user != null)
            {
                _currentUser = user;
                InitializeIds();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Rejestruje nowego użytkownika
        /// </summary>
        /// <param name="username">Nazwa użytkownika</param>
        /// <param name="password">Hasło</param>
        /// <returns>True, jeśli rejestracja się powiodła</returns>
        public bool Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            // Sprawdź, czy użytkownik już istnieje
            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Znajdź następne dostępne ID użytkownika
            var nextUserId = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;

            var newUser = new User
            {
                Id = nextUserId,
                Username = username,
                Password = password,
                Habits = new List<Habit>()
            };

            _users.Add(newUser);
            SaveData();

            return true;
        }

        /// <summary>
        /// Wylogowuje bieżącego użytkownika
        /// </summary>
        public void Logout()
        {
            _currentUser = null;
            _nextHabitId = 1;
            _nextEntryId = 1;
        }

        /// <summary>
        /// Pobiera bieżącego zalogowanego użytkownika
        /// </summary>
        public User? GetCurrentUser()
        {
            return _currentUser;
        }

        /// <summary>
        /// Sprawdza, czy użytkownik jest zalogowany
        /// </summary>
        public bool IsLoggedIn()
        {
            return _currentUser != null;
        }

        /// <summary>
        /// Inicjalizuje następne ID dla nawyków i wpisów na podstawie bieżącego użytkownika
        /// </summary>
        private void InitializeIds()
        {
            if (_currentUser == null)
            {
                _nextHabitId = 1;
                _nextEntryId = 1;
                return;
            }

            // Ustawiamy następne ID dla nawyków
            if (_currentUser.Habits.Any())
            {
                _nextHabitId = _currentUser.Habits.Max(h => h.Id) + 1;
            }
            else
            {
                _nextHabitId = 1;
            }

            // Ustawiamy następne ID dla wpisów
            var allEntries = _currentUser.Habits.SelectMany(h => h.History).ToList();
            if (allEntries.Any())
            {
                _nextEntryId = allEntries.Max(e => e.Id) + 1;
            }
            else
            {
                _nextEntryId = 1;
            }
        }

        /// <summary>
        /// Factory method do tworzenia nawyków
        /// </summary>
        /// <param name="name">Nazwa nawyku</param>
        /// <param name="description">Opis nawyku</param>
        /// <param name="isBoolean">True dla BooleanHabit, False dla QuantitativeHabit</param>
        /// <param name="targetValue">Wartość docelowa (tylko dla QuantitativeHabit)</param>
        /// <param name="unit">Jednostka (tylko dla QuantitativeHabit)</param>
        /// <returns>Utworzony nawyk</returns>
        public Habit CreateHabit(string name, string description, bool isBoolean, double targetValue = 0, string unit = "")
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Brak zalogowanego użytkownika");

            Habit habit;

            if (isBoolean)
            {
                habit = new BooleanHabit
                {
                    Id = _nextHabitId++,
                    Name = name,
                    Description = description,
                    CreatedDate = DateTime.Now,
                    History = new List<HabitEntry>()
                };
            }
            else
            {
                habit = new QuantitativeHabit
                {
                    Id = _nextHabitId++,
                    Name = name,
                    Description = description,
                    CreatedDate = DateTime.Now,
                    History = new List<HabitEntry>(),
                    TargetValue = targetValue,
                    Unit = unit
                };
            }

            _currentUser.Habits.Add(habit);
            SaveData();

            return habit;
        }

        /// <summary>
        /// Usuwa nawyk z listy użytkownika
        /// </summary>
        /// <param name="habitId">ID nawyku do usunięcia</param>
        /// <returns>True, jeśli nawyk został usunięty</returns>
        public bool DeleteHabit(int habitId)
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Brak zalogowanego użytkownika");

            var habit = _currentUser.Habits.FirstOrDefault(h => h.Id == habitId);
            if (habit == null)
                return false;

            _currentUser.Habits.Remove(habit);
            SaveData();

            return true;
        }

        /// <summary>
        /// Dodaje wpis do historii nawyku (logowanie postępu)
        /// </summary>
        /// <param name="habitId">ID nawyku</param>
        /// <param name="date">Data wpisu</param>
        /// <param name="value">Wartość wpisu</param>
        /// <param name="note">Notatka (opcjonalna)</param>
        /// <returns>Utworzony wpis lub null, jeśli nawyk nie został znaleziony</returns>
        public HabitEntry? LogProgress(int habitId, DateTime date, double value, string note = "")
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Brak zalogowanego użytkownika");

            var habit = _currentUser.Habits.FirstOrDefault(h => h.Id == habitId);
            if (habit == null)
                return null;

            // Sprawdź, czy już istnieje wpis dla tego dnia
            var existingEntry = habit.History.FirstOrDefault(e => e.Date.Date == date.Date);
            if (existingEntry != null)
            {
                // Aktualizuj istniejący wpis
                existingEntry.Value = value;
                existingEntry.Note = note;
                existingEntry.IsTargetMet = habit.IsCompleted(value);
                SaveData();
                return existingEntry;
            }

            // Utwórz nowy wpis
            var entry = new HabitEntry
            {
                Id = _nextEntryId++,
                HabitId = habitId,
                Date = date,
                Value = value,
                Note = note,
                IsTargetMet = habit.IsCompleted(value)
            };

            habit.History.Add(entry);
            SaveData();

            return entry;
        }

        /// <summary>
        /// Pobiera nawyki, które mają wpisy dla danego dnia
        /// </summary>
        /// <param name="date">Data do sprawdzenia</param>
        /// <returns>Lista nawyków z wpisami dla danego dnia</returns>
        public List<Habit> GetDailyHabits(DateTime date)
        {
            if (_currentUser == null)
                return new List<Habit>();

            return _currentUser.Habits
                .Where(h => h.History.Any(e => e.Date.Date == date.Date))
                .ToList();
        }

        /// <summary>
        /// Pobiera wszystkie nawyki bieżącego użytkownika
        /// </summary>
        public List<Habit> GetAllHabits()
        {
            if (_currentUser == null)
                return new List<Habit>();

            return _currentUser.Habits.ToList();
        }

        /// <summary>
        /// Pobiera nawyk po ID
        /// </summary>
        public Habit? GetHabitById(int habitId)
        {
            if (_currentUser == null)
                return null;

            return _currentUser.Habits.FirstOrDefault(h => h.Id == habitId);
        }

        /// <summary>
        /// Zapisuje dane do magazynu
        /// </summary>
        private void SaveData()
        {
            _dataService.Save(_users);
        }
    }
}
