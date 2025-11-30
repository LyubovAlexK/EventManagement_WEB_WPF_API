using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EventManagmentApp
{
    public partial class SettingPanel : UserControl
    {
        public SettingPanel()
        {
            InitializeComponent();
            Loaded += SettingPanel_Loaded;
        }

        private void SettingPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем роль пользователя
            RoleText.Text = data.roleAuth == 0 ? "Администратор" : "Организатор";

            // По умолчанию показываем настройки внешнего вида
            ShowAppearanceSettings();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string tag = button.Tag?.ToString();

                switch (tag)
                {
                    case "Appearance":
                        ShowAppearanceSettings();
                        break;
                    case "Zoom":
                        ShowZoomSettings(); // Добавлен вызов метода
                        break;
                    default:
                        SettingsContentControl.Content = null;
                        break;
                }
            }
        }

        private void ShowAppearanceSettings()
        {
            // Сбрасываем подсветку обеих кнопок
            ResetButtonHighlights();

            // Подсвечиваем кнопку Appearance
            AppearanceButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E0FF"));

            var highlightBorder = FindHighlightBorder(AppearanceButton);
            if (highlightBorder != null)
            {
                highlightBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F51FF"));
            }

            // Показываем панель тем
            SettingsContentControl.Content = new ThemesSettingPanel();
        }

        private void ShowZoomSettings()
        {
            // Сбрасываем подсветку обеих кнопок
            ResetButtonHighlights();

            // Подсвечиваем кнопку Zoom
            ZoomButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E0FF"));

            var highlightBorder = FindHighlightBorder(ZoomButton);
            if (highlightBorder != null)
            {
                highlightBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F51FF"));
            }

            // Показываем панель масштабирования
            SettingsContentControl.Content = new ZoomSettingPanel();
        }

        private void ResetButtonHighlights()
        {
            // Сбрасываем подсветку для обеих кнопок
            AppearanceButton.Background = Brushes.Transparent;
            ZoomButton.Background = Brushes.Transparent;

            var appearanceHighlight = FindHighlightBorder(AppearanceButton);
            var zoomHighlight = FindHighlightBorder(ZoomButton);

            if (appearanceHighlight != null)
                appearanceHighlight.Background = Brushes.Transparent;

            if (zoomHighlight != null)
                zoomHighlight.Background = Brushes.Transparent;
        }

        private Border FindHighlightBorder(Button button)
        {
            // Ищем Border с именем "highlightBorder" в визуальном дереве кнопки
            return FindVisualChild<Border>(button, "highlightBorder");
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // Если child искомого типа и с нужным именем
                if (child is T typedChild && (child as FrameworkElement)?.Name == childName)
                {
                    foundChild = typedChild;
                    break;
                }
                else
                {
                    // Рекурсивно ищем в дочерних элементах
                    foundChild = FindVisualChild<T>(child, childName);
                    if (foundChild != null) break;
                }
            }

            return foundChild;
        }
    }
}