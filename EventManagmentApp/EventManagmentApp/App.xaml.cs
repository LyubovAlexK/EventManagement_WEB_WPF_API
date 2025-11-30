using System.Configuration;
using System.Data;
using System.Windows;

namespace EventManagmentApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализируем тему и размер
            ThemeManager.InitializeTheme();
            SizeManager.InitializeSize();
        }
    }

}
