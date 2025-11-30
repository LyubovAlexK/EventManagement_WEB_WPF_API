using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EventManagmentApp
{
    public class RoundedWindow : Window
    {
        public RoundedWindow()
        {
            Style = (Style)Application.Current.Resources["RoundedWindowStyle"];
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Находим заголовок в шаблоне и назначаем обработчик перетаскивания
            if (GetTemplateChild("HeaderBorder") is Border headerBorder)
            {
                headerBorder.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        DragMove();
                };
            }
        }
    }
}