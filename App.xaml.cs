using System;
using System.Windows;
using HabitTracker.Services;

namespace HabitTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static HabitManager HabitManager { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Obsługa nieobsłużonych wyjątków
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                // Inicjalizacja serwisu danych i managera nawyków
                var dataService = new JsonDataService();
                HabitManager = new HabitManager(dataService);

                // Utwórz i pokaż główne okno
                var mainWindow = new MainWindow(HabitManager);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas uruchamiania aplikacji: {ex.Message}\n\n{ex}", 
                    "Błąd krytyczny", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Nieobsłużony wyjątek: {e.Exception.Message}\n\n{e.Exception}", 
                "Błąd", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Krytyczny błąd: {ex.Message}\n\n{ex}", 
                    "Błąd krytyczny", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }
}
