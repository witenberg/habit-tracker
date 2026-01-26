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
            
            LoadGeneralStats();
            
            // Ustaw domyślną zakładkę na "Miesiąc"
            PeriodTabControl.SelectedIndex = 1;
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
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
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

            switch (PeriodTabControl.SelectedIndex)
            {
                case 0: // Tydzień
                    LoadPeriodStats(WEEK_DAYS, WeekStatsPanel);
                    LoadCharts(WEEK_DAYS, WeekMainChart, WeekHeatmap, WeekStreakChart, showHeatmap: false);
                    break;
                case 1: // Miesiąc
                    LoadPeriodStats(MONTH_DAYS, MonthStatsPanel);
                    LoadCharts(MONTH_DAYS, MonthMainChart, MonthHeatmap, MonthStreakChart, showHeatmap: true);
                    break;
                case 2: // Rok
                    LoadPeriodStats(YEAR_DAYS, YearStatsPanel);
                    LoadCharts(YEAR_DAYS, YearMainChart, YearHeatmap, YearStreakChart, showHeatmap: true);
                    break;
            }
        }

        private void LoadPeriodStats(int days, StackPanel panel)
        {
            // Usuń stare statystyki (zachowaj tytuł)
            while (panel.Children.Count > 1)
                panel.Children.RemoveAt(1);

            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-(days - 1));

            // Procent wykonania w okresie
            double percentage = _statsEngine.GetCompletionPercentage(_habit, startDate, endDate);
            AddPeriodStatistic(panel, "Procent wykonania", $"{percentage:F1}%");

            // Dla nawyków ilościowych - średnia wartość
            if (_habit is QuantitativeHabit quantitativeHabit)
            {
                var avgValue = _statsEngine.GetAverageValue(_habit, startDate, endDate);
                if (avgValue.HasValue)
                {
                    AddPeriodStatistic(panel, "Średnia wartość", $"{avgValue.Value:F2} {quantitativeHabit.Unit}");
                }

                // Liczba dni, gdy osiągnięto cel
                var completedDays = _habit.History.Count(e => 
                    e.Date.Date >= startDate && e.Date.Date <= endDate && e.IsTargetMet);
                AddPeriodStatistic(panel, "Dni z osiągniętym celem", $"{completedDays} / {days}");
            }
            else
            {
                // Dla nawyków boolean - liczba wykonanych dni
                var completedDays = _habit.History.Count(e => 
                    e.Date.Date >= startDate && e.Date.Date <= endDate && e.IsTargetMet);
                AddPeriodStatistic(panel, "Wykonane dni", $"{completedDays} / {days}");
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
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(labelText);
            stackPanel.Children.Add(valueText);
            panel.Children.Add(stackPanel);
        }

        private void LoadCharts(int days, 
            OxyPlot.Wpf.PlotView mainChart, 
            OxyPlot.Wpf.PlotView heatmapChart,
            OxyPlot.Wpf.PlotView streakChart,
            bool showHeatmap)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-(days - 1));

            if (_habit is QuantitativeHabit quantitativeHabit)
            {
                // Dla nawyków ilościowych - wykres liniowy z celem
                mainChart.Model = CreateQuantitativeLineChart(startDate, endDate, quantitativeHabit);
            }
            else
            {
                // Dla nawyków boolean - wykres kołowy
                mainChart.Model = CreateBooleanPieChart(startDate, endDate);
            }

            // Kalendarz aktywności (tylko dla miesiąca i roku)
            if (showHeatmap)
            {
                heatmapChart.Model = CreateHeatmapChart(startDate, endDate, days);
            }

            // Wykres serii
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

        private PlotModel CreateHeatmapChart(DateTime startDate, DateTime endDate, int periodDays)
        {
            var model = new PlotModel 
            { 
                Title = "Kalendarz aktywności",
                TitleFontSize = 16,
                Background = OxyColors.White,
                Padding = new OxyThickness(60, 10, 80, 40)
            };

            var chartData = _statsEngine.GetChartData(_habit, startDate, endDate);

            // Przygotuj dane dla heatmapy
            var heatmapData = new List<(int Week, int Day, double Intensity)>();
            
            int weekIndex = 0;
            DateTime currentWeekStart = startDate;

            while (currentWeekStart <= endDate)
            {
                for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
                {
                    var currentDate = currentWeekStart.AddDays(dayOfWeek);
                    if (currentDate > endDate)
                        break;

                    var entry = chartData.FirstOrDefault(d => d.Date.Date == currentDate.Date);
                    double intensity = 0;

                    if (_habit.History.Any(e => e.Date.Date == currentDate.Date))
                    {
                        if (_habit is QuantitativeHabit qHabit)
                        {
                            // Dla Quantitative: intensywność = % wartości docelowej
                            intensity = entry.Value / qHabit.TargetValue * 100.0;
                            intensity = Math.Min(intensity, 100); // Maksymalnie 100%
                        }
                        else
                        {
                            // Dla Boolean: 100 jeśli wykonane, 0 jeśli nie
                            intensity = entry.IsTargetMet ? 100 : 0;
                        }
                    }

                    heatmapData.Add((weekIndex, dayOfWeek, intensity));
                }

                currentWeekStart = currentWeekStart.AddDays(7);
                weekIndex++;
            }

            // Oś Y - dni tygodnia (CategoryAxis)
            var dayAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "DayAxis",
                ItemsSource = new[] { "Pn", "Wt", "Śr", "Cz", "Pt", "So", "Nd" },
                MajorStep = 1,
                MinorStep = 1,
                GapWidth = 0.0,
                IsTickCentered = true,
                FontSize = 12
            };
            model.Axes.Add(dayAxis);

            // Oś X - tygodnie (bez etykiet dla roku, z etykietami dla miesiąca)
            var weekAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = -0.5,
                Maximum = weekIndex - 0.5,
                MajorStep = periodDays >= YEAR_DAYS ? 10 : 1,
                MinorStep = 1,
                IsAxisVisible = periodDays < YEAR_DAYS, // Ukryj oś dla roku
                AxislineStyle = LineStyle.None,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None
            };
            model.Axes.Add(weekAxis);

            // Oś kolorów
            model.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Palette = OxyPalette.Interpolate(100, OxyColors.LightGray, OxyColors.Green),
                Minimum = 0,
                Maximum = 100,
                Title = "Intensywność (%)",
                HighColor = OxyColors.Green,
                LowColor = OxyColors.LightGray
            });

            // Heatmap seria z prawidłowymi zakresami
            var heatMapSeries = new HeatMapSeries
            {
                X0 = -0.5,
                X1 = weekIndex - 0.5,
                Y0 = -0.5,
                Y1 = 6.5,
                Interpolate = false,
                RenderMethod = HeatMapRenderMethod.Rectangles,
                Data = new double[weekIndex, 7]
            };

            foreach (var data in heatmapData)
            {
                if (data.Week < weekIndex && data.Day < 7)
                    heatMapSeries.Data[data.Week, data.Day] = data.Intensity;
            }

            model.Series.Add(heatMapSeries);

            return model;
        }

        private PlotModel CreateStreakChart()
        {
            var model = new PlotModel 
            { 
                Title = "Historia serii (streaki)",
                TitleFontSize = 16,
                Background = OxyColors.White
            };

            var streaks = _statsEngine.GetAllStreaks(_habit);

            if (streaks.Count == 0)
            {
                model.Subtitle = "Brak danych o seriach";
                return model;
            }

            // Oś kategorii (serie)
            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = "Serie"
            };

            for (int i = 0; i < streaks.Count; i++)
            {
                categoryAxis.Labels.Add($"Seria {i + 1}\n{streaks[i].StartDate:dd.MM.yy}");
            }

            model.Axes.Add(categoryAxis);

            // Oś wartości (długość serii)
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Długość serii (dni)",
                MinimumPadding = 0.1,
                MaximumPadding = 0.1
            };
            model.Axes.Add(valueAxis);

            // Serie słupków poziomych
            var barSeries = new BarSeries
            {
                FillColor = OxyColor.FromRgb(255, 152, 0),
                StrokeThickness = 1,
                StrokeColor = OxyColor.FromRgb(245, 124, 0)
            };

            for (int i = 0; i < streaks.Count; i++)
            {
                barSeries.Items.Add(new BarItem(streaks[i].Length, i));
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
