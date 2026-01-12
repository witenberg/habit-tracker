# HabitTracker

Aplikacja WPF do śledzenia nawyków napisana w .NET 10.0.

## Wymagania

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) lub nowszy
- Windows (aplikacja wymaga WPF, które jest dostępne tylko na Windows)

## Instalacja

1. Sklonuj repozytorium:
   ```bash
   git clone <url-repozytorium>
   cd HabitTracker
   ```

2. Sprawdź, czy masz zainstalowany .NET SDK:
   ```bash
   dotnet --version
   ```
   Powinno wyświetlić wersję 10.0.x lub nowszą.

## Uruchomienie aplikacji

### Metoda 1: Uruchomienie z linii poleceń

1. Otwórz terminal w folderze projektu `HabitTracker`
2. Uruchom aplikację za pomocą:
   ```bash
   dotnet run
   ```

### Metoda 2: Uruchomienie zbudowanej aplikacji

1. Zbuduj projekt:
   ```bash
   dotnet build
   ```

2. Uruchom plik wykonywalny:
   ```bash
   .\bin\Debug\net10.0-windows\HabitTracker.exe
   ```

### Metoda 3: Uruchomienie z Visual Studio

1. Otwórz plik `HabitTracker.sln` w Visual Studio (jeśli istnieje) lub otwórz folder projektu
2. Naciśnij `F5` lub kliknij przycisk "Start"

## Przechowywanie danych

Aplikacja przechowuje dane w pliku JSON w lokalizacji:
```
%LocalAppData%\HabitTracker\data.json
```

Plik ten jest automatycznie tworzony przy pierwszym uruchomieniu aplikacji.

## Struktura projektu

- `Models/` - Modele danych (User, Habit, BooleanHabit, QuantitativeHabit, HabitEntry)
- `Services/` - Serwisy aplikacji (HabitManager, JsonDataService, StatsEngine, InputValidator)
- `ViewModels/` - ViewModele dla WPF
- `MainWindow.xaml` - Główne okno aplikacji
- `AddHabitWindow.xaml` - Okno dodawania nawyków
- `StatsWindow.xaml` - Okno ze statystykami

## Rozwiązywanie problemów

### Błąd: "Nie można znaleźć .NET SDK"

Upewnij się, że masz zainstalowany .NET 10.0 SDK. Pobierz go z [oficjalnej strony Microsoft](https://dotnet.microsoft.com/download/dotnet/10.0).

### Błąd podczas uruchamiania aplikacji

Sprawdź, czy wszystkie pliki projektu są obecne i czy nie ma błędów kompilacji:
```bash
dotnet build
```

### Problem z uprawnieniami do zapisu danych

Aplikacja próbuje zapisać dane w folderze `%LocalAppData%\HabitTracker`. Jeśli wystąpi błąd zapisu, sprawdź uprawnienia do tego folderu.
