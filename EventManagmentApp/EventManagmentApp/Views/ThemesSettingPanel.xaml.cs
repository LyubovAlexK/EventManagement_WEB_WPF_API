using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace EventManagmentApp
{
    public partial class ThemesSettingPanel : UserControl, INotifyPropertyChanged
    {
        private bool _isInitialized = false;
        private string _currentColor;

        public string CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor != value)
                {
                    _currentColor = value;
                    OnPropertyChanged(nameof(CurrentColor));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ThemesSettingPanel()
        {
            InitializeComponent();
            this.Loaded += ThemesSettingPanel_Loaded;

            // Подписываемся на изменение темы
            ThemeManager.StaticPropertyChanged += ThemeManager_StaticPropertyChanged;
        }

        private void ThemesSettingPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                InitializeCurrentTheme();
                _isInitialized = true;
            }
        }

        private void ThemeManager_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeManager.CurrentColorTheme))
            {
                // Обновляем переменную при изменении темы
                CurrentColor = ThemeManager.CurrentColorTheme;

                // Также можно обновить UI если нужно
                UpdateColorDisplay();
            }
        }

        private void InitializeCurrentTheme()
        {
            // Инициализируем текущее значение
            CurrentColor = ThemeManager.CurrentColorTheme;

            // Устанавливаем текущую тему в CheckBox'ы
            switch (ThemeManager.CurrentColorTheme)
            {
                case "Purple":
                    PurpleThemeCheckBox.IsChecked = true;
                    break;
                case "Yellow":
                    YellowThemeCheckBox.IsChecked = true;
                    break;
                case "Red":
                    RedThemeCheckBox.IsChecked = true;
                    break;
                case "Green":
                    GreenThemeCheckBox.IsChecked = true;
                    break;
                case "Blue":
                    BlueThemeCheckBox.IsChecked = true;
                    break;
            }

            // Устанавливаем переключатели светлой/темной темы
            if (ThemeManager.CurrentTheme == "Dark")
            {
                DarkThemeToggle.IsChecked = true;
                LightThemeToggle.IsChecked = false;
            }
            else
            {
                DarkThemeToggle.IsChecked = false;
                LightThemeToggle.IsChecked = true;
            }
        }

        private void ColorTheme_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            if (sender is CheckBox checkBox && checkBox.IsChecked == true)
            {
                // Проверяем элементы на null перед использованием
                if (PurpleThemeCheckBox != null)
                    PurpleThemeCheckBox.IsChecked = checkBox == PurpleThemeCheckBox;
                if (YellowThemeCheckBox != null)
                    YellowThemeCheckBox.IsChecked = checkBox == YellowThemeCheckBox;
                if (RedThemeCheckBox != null)
                    RedThemeCheckBox.IsChecked = checkBox == RedThemeCheckBox;
                if (GreenThemeCheckBox != null)
                    GreenThemeCheckBox.IsChecked = checkBox == GreenThemeCheckBox;
                if (BlueThemeCheckBox != null)
                    BlueThemeCheckBox.IsChecked = checkBox == BlueThemeCheckBox;

                string theme = checkBox.Tag?.ToString() ?? "Purple";

                // Применяем тему - это автоматически обновит CurrentColorTheme
                ThemeManager.ApplyColorTheme(theme);

                // Переменная CurrentColor автоматически обновится через событие
                Console.WriteLine($"Тема изменена на: {CurrentColor}");
            }
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            if (sender is ToggleButton toggleButton)
            {
                // Устанавливаем противоположное состояние для другого ToggleButton
                if (toggleButton == DarkThemeToggle)
                {
                    LightThemeToggle.IsChecked = !DarkThemeToggle.IsChecked;
                }
                else
                {
                    DarkThemeToggle.IsChecked = !LightThemeToggle.IsChecked;
                }

                // Применяем выбранную тему
                ThemeManager.ApplyTheme(DarkThemeToggle.IsChecked == true ? "Dark" : "Light");
            }
        }

        private void UpdateColorDisplay()
        {
            // Здесь можно обновить отображение текущего цвета
            // Например, показать текст или изменить какой-то элемент
            Console.WriteLine($"Текущий цвет обновлен: {CurrentColor}");
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Не забудьте отписаться от события при уничтожении контрола
        private void ThemesSettingPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.StaticPropertyChanged -= ThemeManager_StaticPropertyChanged;
        }
    }
}