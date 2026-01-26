using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;
using FontWeights = System.Windows.FontWeights;
using HabitTracker.Models;
using HabitTracker.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace HabitTracker
{
    /// <summary>
    /// Interaction logic for StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        private readonly Habit _habit;
        private readonly StatsEngine _statsEngine;
        private const int WEEK_DAYS = 7;
        private const int MONTH_DAYS = 30;
        private const int YEAR_DAYS = 365;

        public StatsWindow(Habit habit)
        {
            InitializeComponent();
            _habit = habit ?? throw new ArgumentNullException(nameof(habit));
            _statsEngine = new StatsEngine();

            InitializeDatePickers();

            var hoverController = new OxyPlot.PlotController();
            hoverController.UnbindAll();
            hoverController.BindMouseEnter(OxyPlot.PlotCommands.HoverSnapTrack); // Pokaż dymek po najechaniu
                                                                                 // hoverController.BindMouseLeave(OxyPlot.PlotCommands.HoverSnapTrack); // Ukryj po zjechaniu

            // 2. Przypisujemy ten kontroler do Twoich wykresów w oknie
            // (Musisz to zrobić dla każdego wykresu, który ma mieć dymki)
            MonthHeatmap.Controller = hoverController;
            YearHeatmap.Controller = hoverController;

            LoadGeneralStats();

            PeriodTabControl.SelectedIndex = 1;
        }

        private void InitializeDatePickers()
        {
            // Wypełnij lata 
            var currentYear = DateTime.Today.Year;
            var years = Enumerable.Range(currentYear - 4, 5).ToList();
            YearComboBox.ItemsSource = years;
            YearComboBox.SelectedItem = currentYear;

            // Wypełnij miesiące
            var months = new List<string> {
                "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
                "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
            };
            MonthComboBox.ItemsSource = months;
            MonthComboBox.SelectedIndex = DateTime.Today.Month - 1; // -1 bo indeksy są od 0

            YearOnlyComboBox.ItemsSource = years;
            YearOnlyComboBox.SelectedItem = currentYear;
        }

        private void OnDateFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            // Sprawdź oba ComboBoxy z latami - null check
            if (YearComboBox == null || YearComboBox.SelectedItem == null ||
                YearOnlyComboBox == null || YearOnlyComboBox.SelectedItem == null ||
                MonthComboBox == null || MonthComboBox.SelectedIndex == -1)
                return;

            // Sprawdź zakładkę
            if (PeriodTabControl.SelectedIndex == 1) // Zakładka "Miesiąc"
            {
                RefreshMonthView();
            }
            else if (PeriodTabControl.SelectedIndex == 2) // Zakładka "Rok"
            {
                RefreshYearView();
            }
        }

        private void RefreshYearView()
        {
            int selectedYear = (int)YearOnlyComboBox.SelectedItem;

            // zakres: od 1 stycznia do 31 grudnia wybranego roku
            DateTime startDate = new DateTime(selectedYear, 1, 1);
            DateTime endDate = new DateTime(selectedYear, 12, 31);

            LoadPeriodStats(startDate, endDate, YearStatsPanel);
            LoadCharts(startDate, endDate, YearMainChart, YearHeatmap, YearStreakChart, showHeatmap: true);
        }
        private void RefreshMonthView()
        {
            int selectedYear = (int)YearComboBox.SelectedItem;
            int selectedMonth = MonthComboBox.SelectedIndex + 1; // +1 bo indeksy od 0

            // Obliczenie pierwszy i ostatni dzień wybranego miesiąca
            DateTime startDate = new DateTime(selectedYear, selectedMonth, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // Wywołaj metody ładujące z konkretnymi datami
            LoadPeriodStats(startDate, endDate, MonthStatsPanel);
            LoadCharts(startDate, endDate, MonthMainChart, MonthHeatmap, MonthStreakChart, showHeatmap: true);
        }

        private void LoadGeneralStats()
        {
            // Ustaw nazwę i opis nawyku
            HabitNameTextBlock.Text = _habit.Name;
            HabitDescriptionTextBlock.Text = string.IsNullOrWhiteSpace(_habit.Description)
                ? "(Brak opisu)"
                : _habit.Description;

            // Wyczyść panel statystyk ogólnych
            GeneralStatsPanel.Children.Clear();

            // Statystyki niezależne od okresu
            AddGeneralStatistic("🎯 Obecna passa", $"{_statsEngine.GetCurrentStreak(_habit)} dni");
            AddGeneralStatistic("🏆 Najdłuższa passa", $"{_statsEngine.GetLongestStreak(_habit)} dni");
            AddGeneralStatistic("✅ Całkowita liczba dni", $"{_statsEngine.GetTotalCompletedDays(_habit)} dni");
            AddGeneralStatistic("📅 Data utworzenia", _habit.CreatedDate.ToString("dd.MM.yyyy"));
            AddGeneralStatistic("📝 Liczba wpisów", _habit.History.Count.ToString());

            // Dla nawyków ilościowych dodaj cel
            if (_habit is QuantitativeHabit quantitativeHabit)
            {
                AddGeneralStatistic("🎯 Cel", $"{quantitativeHabit.TargetValue} {quantitativeHabit.Unit}");
            }
        }

        private void AddGeneralStatistic(string label, string value)
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(15, 0, 15, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(labelText);
            stackPanel.Children.Add(valueText);
            GeneralStatsPanel.Children.Add(stackPanel);
        }

        private void PeriodTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PeriodTabControl.SelectedIndex == -1)
                return;

            DateTime today = DateTime.Today;

            switch (PeriodTabControl.SelectedIndex)
            {
                case 0: // Tydzień (Ostatnie 7 dni)
                    var weekStart = today.AddDays(-6);
                    LoadPeriodStats(weekStart, today, WeekStatsPanel);
                    LoadCharts(weekStart, today, WeekMainChart, WeekHeatmap, WeekStreakChart, showHeatmap: false);
                    break;

                case 1: // Miesiąc (Korzystamy z tego, co wybrano w ComboBoxach)
                    RefreshMonthView();
                    break;
                case 2: // Rok
                        // Zamiast liczyć daty ręcznie, po prostu wywołujemy odświeżenie
                        // na podstawie tego, co jest w ComboBoxie
                    if (YearOnlyComboBox != null && YearOnlyComboBox.SelectedItem != null)
                    {
                        RefreshYearView();
                    }
                    break;
            }
        }

        private void LoadPeriodStats(DateTime startDate, DateTime endDate, StackPanel panel)
        {
            // Usuń stare statystyki (zachowaj tytuł)
            while (panel.Children.Count > 1)
                panel.Children.RemoveAt(1);

            int totalDays = (endDate - startDate).Days + 1;

            double percentage = _statsEngine.GetCompletionPercentage(_habit, startDate, endDate);
            AddPeriodStatistic(panel, "Procent wykonania", $"{percentage:F1}%");

            if (_habit is QuantitativeHabit quantitativeHabit)
            {
                var avgValue = _statsEngine.GetAverageValue(_habit, startDate, endDate);
                if (avgValue.HasValue)
                {
                    AddPeriodStatistic(panel, "Średnia wartość", $"{avgValue.Value:F2} {quantitativeHabit.Unit}");
                }

                var completedDays = _habit.History.Count(e =>
                    e.Date.Date >= startDate && e.Date.Date <= endDate && e.IsTargetMet);
                AddPeriodStatistic(panel, "Dni z osiągniętym celem", $"{completedDays} / {totalDays}");
            }
            else
            {
                var completedDays = _habit.History.Count(e =>
                    e.Date.Date >= startDate && e.Date.Date <= endDate && e.IsTargetMet);
                AddPeriodStatistic(panel, "Wykonane dni", $"{completedDays} / {totalDays}");
            }
        }

        private void AddPeriodStatistic(StackPanel panel, string label, string value)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var labelText = new TextBlock
            {
                Text = label + ":",
                FontWeight = FontWeights.SemiBold,
                Width = 200,
                VerticalAlignment = VerticalAlignment.Center
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(labelText);
            stackPanel.Children.Add(valueText);
            panel.Children.Add(stackPanel);
        }

        private void LoadCharts(DateTime startDate, DateTime endDate,
            OxyPlot.Wpf.PlotView mainChart,
            OxyPlot.Wpf.PlotView heatmapChart,
            OxyPlot.Wpf.PlotView streakChart,
            bool showHeatmap)
        {
            int totalDays = (endDate - startDate).Days + 1;

            if (_habit is QuantitativeHabit quantitativeHabit)
            {
                mainChart.Model = CreateQuantitativeLineChart(startDate, endDate, quantitativeHabit);
            }
            else
            {
                mainChart.Model = CreateBooleanPieChart(startDate, endDate);
            }

            if (showHeatmap)
            {
                heatmapChart.Model = CreateHeatmapChart(startDate, endDate, totalDays);
            }

            streakChart.Model = CreateStreakChart();
        }

        // ============= WYKRESY DLA QUANTITATIVE =============

        private PlotModel CreateQuantitativeLineChart(DateTime startDate, DateTime endDate, QuantitativeHabit habit)
        {
            var model = new PlotModel
            {
                Title = "Wartości w czasie",
                TitleFontSize = 16,
                Background = OxyColors.White
            };

            // Oś X - daty
            var totalDays = (endDate - startDate).Days + 1;
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = totalDays <= 7 ? "dd.MM" : "dd.MM",
                Title = "Data",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            };
            model.Axes.Add(dateAxis);

            // Oś Y - wartości
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = $"Wartość ({habit.Unit})",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240),
                MinimumPadding = 0.1,
                MaximumPadding = 0.1
            };
            model.Axes.Add(valueAxis);

            // Seria danych - wartości
            var lineSeries = new LineSeries
            {
                Title = "Wartość",
                Color = OxyColor.FromRgb(33, 150, 243),
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColor.FromRgb(33, 150, 243)
            };

            var chartData = _statsEngine.GetChartData(_habit, startDate, endDate);
            foreach (var data in chartData)
            {
                if (_habit.History.Any(e => e.Date.Date == data.Date))
                {
                    lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(data.Date), data.Value));
                }
            }

            model.Series.Add(lineSeries);

            // Linia celu (przerywana, czerwona)
            var targetLine = new LineSeries
            {
                Title = "Cel",
                Color = OxyColor.FromRgb(244, 67, 54),
                StrokeThickness = 2,
                LineStyle = LineStyle.Dash
            };

            targetLine.Points.Add(new DataPoint(DateTimeAxis.ToDouble(startDate), habit.TargetValue));
            targetLine.Points.Add(new DataPoint(DateTimeAxis.ToDouble(endDate), habit.TargetValue));

            model.Series.Add(targetLine);

            // Legenda
            model.Legends.Add(new Legend
            {
                LegendPosition = LegendPosition.TopRight,
                LegendPlacement = LegendPlacement.Inside
            });

            return model;
        }

        // ============= WYKRESY DLA BOOLEAN =============

        private PlotModel CreateBooleanPieChart(DateTime startDate, DateTime endDate)
        {
            var model = new PlotModel
            {
                Title = "Podsumowanie wykonania",
                TitleFontSize = 16,
                Background = OxyColors.White
            };

            var totalDays = (endDate - startDate).Days + 1;
            var completedDays = _habit.History.Count(e =>
                e.Date.Date >= startDate && e.Date.Date <= endDate && e.IsTargetMet);
            var notCompletedDays = totalDays - completedDays;

            var pieSeries = new PieSeries
            {
                StrokeThickness = 2,
                InsideLabelPosition = 0.5,
                AngleSpan = 360,
                StartAngle = 0
            };

            pieSeries.Slices.Add(new PieSlice("Wykonane", completedDays)
            {
                Fill = OxyColor.FromRgb(76, 175, 80),
                IsExploded = false
            });

            pieSeries.Slices.Add(new PieSlice("Niewykonane", notCompletedDays)
            {
                Fill = OxyColor.FromRgb(244, 67, 54),
                IsExploded = false
            });

            model.Series.Add(pieSeries);

            model.Legends.Add(new Legend
            {
                LegendPosition = LegendPosition.RightMiddle,
                LegendPlacement = LegendPlacement.Outside
            });

            return model;
        }

        // ============= WYKRESY UNIWERSALNE =============

        private PlotModel CreateHeatmapChart(DateTime startDate, DateTime endDate, int totalDays)
        {
            var model = new PlotModel
            {
                Title = totalDays > 40 ? "Kalendarz aktywności (Rok)" : "Kalendarz aktywności",
                TitleFontSize = 15,
                TitleFontWeight = OxyPlot.FontWeights.Bold,
                Background = OxyColors.White,
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            // Oś Y - Dni tygodnia
            var dayAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                ItemsSource = new[] { "Pn", "Wt", "Śr", "Cz", "Pt", "So", "Nd" },
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                IsTickCentered = true,
                GapWidth = 0
            };
            model.Axes.Add(dayAxis);

            // Oś X - Tygodnie
            var weekAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                IsTickCentered = true,
                GapWidth = 0
            };
            model.Axes.Add(weekAxis);

            // Seria Prostokątów
            var rectangleSeries = new RectangleBarSeries
            {
                StrokeColor = OxyColors.White, // Biała siatka
                StrokeThickness = 2,
                LabelFormatString = null,
                TrackerFormatString = "{Title}"
            };

            var chartData = _statsEngine.GetChartData(_habit, startDate, endDate);

            Func<DateTime, string> getTooltipText = (date) =>
                {
                    if (totalDays <= 40) // Widok Miesięczny
                    {
                        return $"{date:dddd, dd} ({date:MMMM})";
                    }
                    else // Widok Roczny
                    {
                        return $"{date:dd.MM.yyyy}";
                    }
                };

            DateTime currentWeekStart = startDate;
            int weekIndex = 0;
            int lastMonth = -1;

            while (currentWeekStart <= endDate)
            {
                // --- Etykiety ---
                DateTime middleOfWeek = currentWeekStart.AddDays(3);
                string label = "";

                if (totalDays > 40) // Widok roczny
                {
                    if (middleOfWeek.Month != lastMonth)
                    {
                        label = middleOfWeek.ToString("MMM");
                        lastMonth = middleOfWeek.Month;
                    }
                }
                else // Widok miesięczny
                {
                    label = (weekIndex + 1).ToString();
                }
                weekAxis.Labels.Add(label);
                // ----------------

                for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
                {
                    var currentDate = currentWeekStart.AddDays(dayOfWeek);
                    OxyColor fillColor = OxyColor.Parse("#EBEDF0");

                    if (currentDate <= endDate)
                    {
                        var entry = chartData.FirstOrDefault(d => d.Date.Date == currentDate.Date);

                        if (_habit.History.Any(e => e.Date.Date == currentDate.Date))
                        {
                            double intensity = 0;
                            if (_habit is QuantitativeHabit qHabit)
                                intensity = (entry.Value / qHabit.TargetValue);
                            else
                                intensity = entry.IsTargetMet ? 1.0 : 0.0;

                            if (intensity > 1) intensity = 1;

                            if (intensity <= 0) fillColor = OxyColor.Parse("#EBEDF0");
                            else if (intensity < 0.4) fillColor = OxyColor.Parse("#9BE9A8");
                            else if (intensity < 0.7) fillColor = OxyColor.Parse("#40C463");
                            else fillColor = OxyColor.Parse("#039632");
                        }
                    }
                    rectangleSeries.Items.Add(new RectangleBarItem
                    {
                        X0 = weekIndex - 0.5,
                        X1 = weekIndex + 0.5,
                        Y0 = dayOfWeek - 0.5,
                        Y1 = dayOfWeek + 0.5,
                        Color = fillColor,
                        Title = getTooltipText(currentDate)
                    });
                }

                currentWeekStart = currentWeekStart.AddDays(7);
                weekIndex++;
            }

            model.Series.Add(rectangleSeries);
            return model;
        }
        private PlotModel CreateStreakChart()
        {
            var model = new PlotModel
            {
                Title = "🏆 Twoje rekordy (Top 5 serii)",
                TitleFontSize = 15,
                TitleFontWeight = OxyPlot.FontWeights.Bold,
                Background = OxyColors.White,
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var allStreaks = _statsEngine.GetAllStreaks(_habit);

            var topStreaks = allStreaks
                .OrderByDescending(s => s.Length)
                .Take(5)
                .Reverse()
                .ToList();

            if (topStreaks.Count == 0)
            {
                model.Subtitle = "Brak danych. Zbuduj swoją pierwszą serię!";
                model.SubtitleColor = OxyColors.Gray;
                return model;
            }

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                GapWidth = 0.3,
            };
            model.Axes.Add(categoryAxis);

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                MajorStep = 1,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(224, 224, 224),
                AxislineStyle = LineStyle.None,
                MaximumPadding = 0.1
            };
            model.Axes.Add(valueAxis);

            var barSeries = new BarSeries
            {
                // FillColor = OxyColor.Parse("#2ECC71),
                StrokeThickness = 0,
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0} dni"
            };

            var greenColor = OxyColor.Parse("#2ECC71");

            foreach (var streak in topStreaks)
            {
                barSeries.Items.Add(new BarItem { Value = streak.Length, Color = greenColor});
                var endDate = streak.StartDate.AddDays(streak.Length - 1);

                string dateLabel = $"{streak.StartDate:dd.MM} - {endDate:dd.MM}";

                categoryAxis.Labels.Add(dateLabel);
            }

            model.Series.Add(barSeries);

            return model;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
