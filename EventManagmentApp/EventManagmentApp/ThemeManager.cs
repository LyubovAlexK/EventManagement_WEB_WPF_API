using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace EventManagmentApp
{
    public static class ThemeManager
    {
        private static string _currentColorTheme = "Purple";
        private static string _currentTheme = "Light";

        // Заменяем статические поля на свойства
        public static string CurrentColor => CurrentColorTheme;
        public static bool IsPurpleTheme => CurrentColorTheme == "Purple";
        public static bool IsYellowTheme => CurrentColorTheme == "Yellow";
        public static bool IsRedTheme => CurrentColorTheme == "Red";
        public static bool IsGreenTheme => CurrentColorTheme == "Green";
        public static bool IsBlueTheme => CurrentColorTheme == "Blue";
        public static bool IsDarkTheme => CurrentTheme == "Dark";
        public static bool IsLightTheme => CurrentTheme == "Light";

        public static string CurrentColorTheme
        {
            get => _currentColorTheme;
            private set
            {
                if (_currentColorTheme != value)
                {
                    _currentColorTheme = value;
                    OnStaticPropertyChanged();

                    // Также уведомляем об изменении зависимых свойств
                    OnStaticPropertyChanged(nameof(CurrentColor));
                    OnStaticPropertyChanged(nameof(IsPurpleTheme));
                    OnStaticPropertyChanged(nameof(IsYellowTheme));
                    OnStaticPropertyChanged(nameof(IsRedTheme));
                    OnStaticPropertyChanged(nameof(IsGreenTheme));
                    OnStaticPropertyChanged(nameof(IsBlueTheme));
                }
            }
        }

        public static string CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnStaticPropertyChanged();

                    // Уведомляем об изменении зависимых свойств
                    OnStaticPropertyChanged(nameof(IsDarkTheme));
                    OnStaticPropertyChanged(nameof(IsLightTheme));
                }
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static void ApplyColorTheme(string themeColor)
        {
            CurrentColorTheme = themeColor;
            ApplyThemeToApplication();
        }

        public static void ApplyTheme(string theme)
        {
            CurrentTheme = theme;
            ApplyThemeToApplication();
        }

        public static void ApplyThemeToApplication()
        {
            var resources = Application.Current.Resources;

            // Применяем цветовую тему (существующий код без изменений)
            switch (CurrentColorTheme)
            {
                case "Purple":
                    resources["PrimaryColor"] = Color.FromRgb(111, 81, 255);
                    resources["PrimaryColor40"] = Color.FromRgb(177, 161, 255);
                    resources["PrimaryColor30"] = Color.FromRgb(229, 224, 255);
                    break;
                case "Yellow":
                    resources["PrimaryColor"] = Color.FromRgb(245, 158, 11);
                    resources["PrimaryColor40"] = Color.FromRgb(251, 191, 36);
                    resources["PrimaryColor30"] = Color.FromRgb(252, 211, 77);
                    break;
                case "Red":
                    resources["PrimaryColor"] = Color.FromRgb(244, 63, 94);
                    resources["PrimaryColor40"] = Color.FromRgb(251, 113, 133);
                    resources["PrimaryColor30"] = Color.FromRgb(253, 164, 175);
                    break;
                case "Green":
                    resources["PrimaryColor"] = Color.FromRgb(34, 197, 94);
                    resources["PrimaryColor40"] = Color.FromRgb(74, 222, 128);
                    resources["PrimaryColor30"] = Color.FromRgb(134, 239, 172);
                    break;
                case "Blue":
                    resources["PrimaryColor"] = Color.FromRgb(59, 130, 246);
                    resources["PrimaryColor40"] = Color.FromRgb(96, 165, 250);
                    resources["PrimaryColor30"] = Color.FromRgb(147, 197, 253);
                    break;
            }

            // Применяем тему (светлая/темная)
            if (IsDarkTheme)
            {
                resources["BackgroundColor"] = Color.FromRgb(30, 41, 59);
                resources["PanelBackground"] = Color.FromRgb(51, 65, 85);
                resources["HeaderBackground"] = Color.FromRgb(51, 65, 85);
                resources["TextColor"] = Colors.White;
                resources["BorderColor"] = Color.FromRgb(71, 85, 105);
                resources["SecondaryTextColor"] = Color.FromRgb(148, 163, 184);
                resources["HoverColor"] = Color.FromRgb(71, 85, 105);

                // Цвета для боковой панели в темной теме
                resources["SidebarTextColor"] = Colors.White;
                resources["SidebarSecondaryTextColor"] = Color.FromRgb(203, 213, 225);
            }
            else
            {
                resources["BackgroundColor"] = Colors.White;
                resources["PanelBackground"] = Colors.White;
                resources["HeaderBackground"] = Colors.White;
                resources["TextColor"] = Color.FromRgb(30, 41, 59);
                resources["BorderColor"] = Color.FromRgb(203, 213, 225);
                resources["SecondaryTextColor"] = Color.FromRgb(107, 125, 150);
                resources["HoverColor"] = Color.FromRgb(229, 231, 235);

                // Цвета для боковой панели в светлой теме
                resources["SidebarTextColor"] = Colors.White; // В боковой панели всегда белый в светлой теме
                resources["SidebarSecondaryTextColor"] = Color.FromRgb(248, 250, 252); // Более светлый оттенок
            }

            UpdateBrushResources();
        }

        private static void UpdateBrushResources()
        {
            var resources = Application.Current.Resources;

            // Обновляем SolidColorBrush ресурсы (существующий код)
            resources["PrimaryBrush"] = new SolidColorBrush((Color)resources["PrimaryColor"]);
            resources["PrimaryBrush40"] = new SolidColorBrush((Color)resources["PrimaryColor40"]);
            resources["PrimaryBrush30"] = new SolidColorBrush((Color)resources["PrimaryColor30"]);
            resources["BackgroundBrush"] = new SolidColorBrush((Color)resources["BackgroundColor"]);
            resources["TextBrush"] = new SolidColorBrush((Color)resources["TextColor"]);
            resources["BorderBrush"] = new SolidColorBrush((Color)resources["BorderColor"]);
            resources["PanelBackgroundBrush"] = new SolidColorBrush((Color)resources["PanelBackground"]);
            resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)resources["HeaderBackground"]);
            resources["SecondaryTextBrush"] = new SolidColorBrush((Color)resources["SecondaryTextColor"]);
            resources["HoverBrush"] = new SolidColorBrush((Color)resources["HoverColor"]);

            // Новые ресурсы для боковой панели
            resources["SidebarTextBrush"] = new SolidColorBrush((Color)resources["SidebarTextColor"]);
            resources["SidebarSecondaryTextBrush"] = new SolidColorBrush((Color)resources["SidebarSecondaryTextColor"]);
        }

        // Метод для инициализации темы при запуске приложения
        public static void InitializeTheme()
        {
            ApplyThemeToApplication();
        }
    }
}