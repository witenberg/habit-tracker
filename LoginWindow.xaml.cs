using System;
using System.Windows;
using System.Windows.Input;
using HabitTracker.Services;

namespace HabitTracker
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly HabitManager _habitManager;

        public bool IsLoggedIn { get; private set; } = false;

        public LoginWindow(HabitManager habitManager)
        {
            InitializeComponent();
            _habitManager = habitManager ?? throw new ArgumentNullException(nameof(habitManager));
            UsernameTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptRegister();
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            SwitchToRegisterMode();
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            SwitchToLoginMode();
        }

        private void SwitchToRegisterMode()
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            RegisterUsernameTextBox.Focus();
            HideError();
        }

        private void SwitchToLoginMode()
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            UsernameTextBox.Focus();
            HideError();
        }

        private void AttemptLogin()
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Proszę wprowadzić nazwę użytkownika.");
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Proszę wprowadzić hasło.");
                PasswordBox.Focus();
                return;
            }

            try
            {
                if (_habitManager.Login(username, password))
                {
                    IsLoggedIn = true;
                    DialogResult = true;
                    // DialogResult automatycznie zamknie okno dialogowe
                }
                else
                {
                    ShowError("Nieprawidłowa nazwa użytkownika lub hasło.");
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Błąd podczas logowania: {ex.Message}");
            }
        }

        private void AttemptRegister()
        {
            var username = RegisterUsernameTextBox.Text.Trim();
            var password = RegisterPasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Proszę wprowadzić nazwę użytkownika.");
                RegisterUsernameTextBox.Focus();
                return;
            }

            if (username.Length < 3)
            {
                ShowError("Nazwa użytkownika musi mieć co najmniej 3 znaki.");
                RegisterUsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Proszę wprowadzić hasło.");
                RegisterPasswordBox.Focus();
                return;
            }

            if (password.Length < 4)
            {
                ShowError("Hasło musi mieć co najmniej 4 znaki.");
                RegisterPasswordBox.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Hasła nie są identyczne.");
                ConfirmPasswordBox.Clear();
                ConfirmPasswordBox.Focus();
                return;
            }

            try
            {
                if (_habitManager.Register(username, password))
                {
                    // Automatyczne logowanie po rejestracji
                    if (_habitManager.Login(username, password))
                    {
                        // Ustaw flagi - DialogResult automatycznie zamknie okno dialogowe
                        IsLoggedIn = true;
                        DialogResult = true;
                    }
                    else
                    {
                        ShowError("Rejestracja się powiodła, ale logowanie nie powiodło się. Spróbuj zalogować się ręcznie.");
                        SwitchToLoginMode();
                        UsernameTextBox.Text = username;
                        PasswordBox.Focus();
                    }
                }
                else
                {
                    ShowError("Użytkownik o tej nazwie już istnieje.");
                    RegisterUsernameTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Błąd podczas rejestracji: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            ErrorMessageTextBlock.Text = string.Empty;
        }

        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptLogin();
            }
        }

        private void RegisterUsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterPasswordBox.Focus();
            }
        }

        private void RegisterPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmPasswordBox.Focus();
            }
        }

        private void ConfirmPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptRegister();
            }
        }
    }
}
