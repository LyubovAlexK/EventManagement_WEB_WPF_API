using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace EventManagmentApp
{
    public partial class ZoomSettingPanel : UserControl
    {
        public ZoomSettingPanel()
        {
            InitializeComponent();
            LoadCurrentSize();
        }

        private void LoadCurrentSize()
        {
            // Находим все ToggleButton в контейнере
            var toggleButtons = FindVisualChildren<ToggleButton>(this).ToList();

            foreach (var toggle in toggleButtons)
            {
                toggle.IsChecked = false;
            }

            // Устанавливаем текущий размер
            var currentToggle = toggleButtons.FirstOrDefault(t =>
                t.Tag?.ToString() == SizeManager.CurrentSize);

            if (currentToggle != null)
            {
                currentToggle.IsChecked = true;
            }
            else
            {
                // По умолчанию средний размер
                var mediumToggle = toggleButtons.FirstOrDefault(t =>
                    t.Tag?.ToString() == "Medium");
                mediumToggle?.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
            }
        }

        private void SizeToggle_Checked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            if (toggle?.IsChecked == true && toggle.Tag != null)
            {
                // Находим все ToggleButton и сбрасываем их, кроме текущего
                var toggleButtons = FindVisualChildren<ToggleButton>(this);
                foreach (var otherToggle in toggleButtons)
                {
                    if (otherToggle != toggle && otherToggle.Tag != null)
                    {
                        otherToggle.IsChecked = false;
                    }
                }

                // Применяем выбранный размер
                string size = toggle.Tag.ToString();
                SizeManager.ApplySize(size);
            }
        }

        // Вспомогательный метод для поиска дочерних элементов
        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}