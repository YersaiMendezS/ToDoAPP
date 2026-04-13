using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ToDoAPP.Application.Interfaces;

namespace ToDoAPP.Infrastructure.Theme
{
    public class WpfThemeManager : IThemeManager
    {
        public void ApplyTheme(Window window, ComboBox themeComboBox, bool isDarkMode)
        {
            if (isDarkMode)
            {
                window.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                window.Foreground = Brushes.White;
                System.Windows.Application.Current.Resources["PopupBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
                System.Windows.Application.Current.Resources["TextBrush"] = Brushes.White;
                System.Windows.Application.Current.Resources["ControlBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42"));
            }
            else
            {
                window.Background = Brushes.White;
                window.Foreground = Brushes.Black;
                System.Windows.Application.Current.Resources["PopupBackgroundBrush"] = Brushes.White;
                System.Windows.Application.Current.Resources["TextBrush"] = Brushes.Black;
                System.Windows.Application.Current.Resources["ControlBackgroundBrush"] = Brushes.White;
            }

            if (themeComboBox != null) 
                themeComboBox.SelectedIndex = isDarkMode ? 1 : 0;
        }
    }
}
