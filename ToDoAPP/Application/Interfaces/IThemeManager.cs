using System.Windows;
using System.Windows.Controls;

namespace ToDoAPP.Application.Interfaces
{
    public interface IThemeManager
    {
        void ApplyTheme(Window window, ComboBox themeComboBox, bool isDarkMode);
    }
}
