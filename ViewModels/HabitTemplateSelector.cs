using System.Windows;
using System.Windows.Controls;
using HabitTracker.Models;

namespace HabitTracker.ViewModels
{
    /// <summary>
    /// Selektor szablonów dla różnych typów nawyków w ListView
    /// </summary>
    public class HabitTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BooleanHabitTemplate { get; set; }
        public DataTemplate? QuantitativeHabitTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is BooleanHabit)
            {
                return BooleanHabitTemplate;
            }
            else if (item is QuantitativeHabit)
            {
                return QuantitativeHabitTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
