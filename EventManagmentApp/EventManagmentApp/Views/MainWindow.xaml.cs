using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventManagmentApp
{
    public partial class MainWindow : RoundedWindow
    {
        public ICommand ToggleAddPanelCommand { get; }
        public ICommand CloseAddPanelCommand { get; }
        private bool isAddPanelVisible = false;

        // Убираем статические поля и делаем их обычными свойствами
        private Color ActiveColor => GetActiveColor();
        private SolidColorBrush ActiveBrush => new SolidColorBrush(ActiveColor);
        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);

        public MainWindow()
        {
            ToggleAddPanelCommand = new RelayCommand((parameter) => ToggleAddPanel());
            CloseAddPanelCommand = new RelayCommand((parameter) => HideAddEditPanel());

            InitializeComponent();

            // Подписываемся на изменение темы
            ThemeManager.StaticPropertyChanged += ThemeManager_StaticPropertyChanged;

            EventsPanelControl.ToggleAddPanelCommand = ToggleAddPanelCommand;
            CategoriesPanelControl.ToggleAddPanelCommand = ToggleAddPanelCommand;
            VenuesPanelControl.ToggleAddPanelCommand = ToggleAddPanelCommand;
            ClientsPanelControl.ToggleAddPanelCommand = ToggleAddPanelCommand;
            UsersPanelControl.ToggleAddPanelCommand = ToggleAddPanelCommand;
            RolesPanelControl.ToggleAddPanelCommand = ToggleAddPanelCommand;

            AddEditPanelControl.ClosePanelCommand = CloseAddPanelCommand;

            EnterRoles();

            ShowEventsPanel();
        }

        private void ThemeManager_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Если изменилась цветовая тема, обновляем UI
            if (e.PropertyName == nameof(ThemeManager.CurrentColorTheme) ||
                e.PropertyName == nameof(ThemeManager.CurrentColor))
            {
                UpdateActivePanelColors();
            }
        }

        private Color GetActiveColor()
        {
            // Получаем цвет из текущей темы
            return ThemeManager.CurrentColorTheme switch
            {
                "Purple" => Color.FromRgb(111, 81, 255),
                "Yellow" => Color.FromRgb(245, 158, 11),
                "Red" => Color.FromRgb(244, 63, 94),
                "Green" => Color.FromRgb(34, 197, 94),
                "Blue" => Color.FromRgb(59, 130, 246),
                _ => Color.FromRgb(111, 81, 255) // По умолчанию фиолетовый
            };
        }

        private void UpdateActivePanelColors()
        {
            // Обновляем цвета активной панели
            Dispatcher.Invoke(() =>
            {
                // Находим активную кнопку и обновляем ее цвет
                var activeButton = FindActiveButton();
                if (activeButton != null)
                {
                    activeButton.Foreground = ActiveBrush;
                }

                // Обновляем иконку активной панели
                UpdateActivePanelIcon();
            });
        }

        private Button FindActiveButton()
        {
            // Проверяем, какая панель активна и возвращаем соответствующую кнопку
            if (EventsPanelControl.Visibility == Visibility.Visible)
                return btnEvent;
            else if (CategoriesPanelControl.Visibility == Visibility.Visible)
                return btnCategory;
            else if (VenuesPanelControl.Visibility == Visibility.Visible)
                return btnVenues;
            else if (SettingPanelControl.Visibility == Visibility.Visible)
                return btnSettings;
            else if (ClientsPanelControl.Visibility == Visibility.Visible)
                return btnClient;
            else if (UsersPanelControl.Visibility == Visibility.Visible)
                return btnUser;
            else if (RolesPanelControl.Visibility == Visibility.Visible)
                return btnRole;

            return btnEvent; // По умолчанию
        }

        private void UpdateActivePanelIcon()
        {
            // Обновляем иконку активной панели
            if (EventsPanelControl.Visibility == Visibility.Visible)
                SetIcon(IconEvent, GetIconName("Event"));
            else if (CategoriesPanelControl.Visibility == Visibility.Visible)
                SetIcon(IconCategory, GetIconName("Category"));
            else if (VenuesPanelControl.Visibility == Visibility.Visible)
                SetIcon(IconVenue, GetIconName("map"));
            else if (SettingPanelControl.Visibility == Visibility.Visible)
                SetIcon(IconSetting, GetIconName("Setting"));
            else if (ClientsPanelControl.Visibility == Visibility.Visible)
                SetIcon(IconClient, GetIconName("Clients"));
            else if (UsersPanelControl.Visibility == Visibility.Visible)
                SetIcon(IconUser, GetIconName("userSetting"));
            else if (RolesPanelControl.Visibility == Visibility.Visible)
                SetIcon(IconRole, GetIconName("Role"));
        }

        private string GetIconName(string baseName)
        {
            // Получаем имя иконки в зависимости от темы
            return ThemeManager.CurrentColorTheme switch
            {
                "Purple" => $"{baseName}Purple.png",
                "Yellow" => $"{baseName}Orange.png", // или Yellow.png, в зависимости от ваших файлов
                "Red" => $"{baseName}Red.png",
                "Green" => $"{baseName}Green.png",
                "Blue" => $"{baseName}Blue.png",
                _ => $"{baseName}Purple.png"
            };
        }

        private void EnterRoles()
        {
            if (data.roleAuth == 1)
            {
                ControlCenter.Visibility = Visibility.Hidden;
            }
            if (data.roleAuth == 0)
            {
                ControlCenter.Visibility = Visibility.Visible;
            }
            EventsPanelControl.UpdateUIForUserRole(data.roleAuth);
        }

        private void ShowEventsPanel()
        {
            ActivatePanel("Events", EventsPanelControl, EventItem, btnEvent, IconEvent, "Event");
        }

        // УНИФИЦИРОВАННЫЕ МЕТОДЫ ДЛЯ ВСЕХ ПАНЕЛЕЙ
        private void ShowPanelForEntity(string entityType, object selectedEntity = null)
        {
            try
            {
                data.addEditTable = entityType;

                // Сбрасываем все выделенные объекты
                AddEditPanelControl.SelectedEvent = null;
                AddEditPanelControl.SelectedCategory = null;
                AddEditPanelControl.SelectedVenue = null;
                AddEditPanelControl.SelectedClient = null;
                AddEditPanelControl.SelectedUser = null;
                AddEditPanelControl.SelectedRole = null;

                // Устанавливаем выбранный объект в зависимости от типа
                switch (entityType)
                {
                    case "Event":
                        AddEditPanelControl.SelectedEvent = selectedEntity as EventViewModel;
                        break;
                    case "Category":
                        AddEditPanelControl.SelectedCategory = selectedEntity as CategoryViewModel;
                        break;
                    case "Venue":
                        AddEditPanelControl.SelectedVenue = selectedEntity as VenueViewModel;
                        break;
                    case "Client":
                        AddEditPanelControl.SelectedClient = selectedEntity as ClientViewModel;
                        break;
                    case "User":
                        AddEditPanelControl.SelectedUser = selectedEntity as UserViewModel;
                        break;
                    case "Role":
                        AddEditPanelControl.SelectedRole = selectedEntity as RoleViewModel;
                        break;
                }

                AddEditPanelControl.GenerateFormFields();
                ShowAddEditPanel();
            }
            catch (Exception ex)
            {
                string action = selectedEntity == null ? "добавления" : "редактирования";
                MessageBox.Show($"Ошибка открытия формы {action}: {ex.Message}", "Ошибка");
            }
        }

        // МЕТОДЫ ДЛЯ СОБЫТИЙ
        public void ShowAddPanelForEvent() => InitializeAddPanel("Event");
        public void ShowEditPanelForEvent(EventViewModel selectedEvent) => ShowPanelForEntity("Event", selectedEvent);

        // МЕТОДЫ ДЛЯ КАТЕГОРИЙ
        public void ShowAddPanelForCategory() => InitializeAddPanel("Category");
        public void ShowEditPanelForCategory(CategoryViewModel selectedCategory) => ShowPanelForEntity("Category", selectedCategory);

        // МЕТОДЫ ДЛЯ МЕСТ ПРОВЕДЕНИЯ
        public void ShowAddPanelForVenue() => InitializeAddPanel("Venue");
        public void ShowEditPanelForVenue(VenueViewModel selectedVenue) => ShowPanelForEntity("Venue", selectedVenue);

        // МЕТОДЫ ДЛЯ КЛИЕНТОВ
        public void ShowAddPanelForClient() => InitializeAddPanel("Client");
        public void ShowEditPanelForClient(ClientViewModel selectedClient) => ShowPanelForEntity("Client", selectedClient);

        // МЕТОДЫ ДЛЯ ПОЛЬЗОВАТЕЛЕЙ
        public void ShowAddPanelForUser() => InitializeAddPanel("User");
        public void ShowEditPanelForUser(UserViewModel selectedUser) => ShowPanelForEntity("User", selectedUser);

        // МЕТОДЫ ДЛЯ РОЛЕЙ
        public void ShowAddPanelForRole() => InitializeAddPanel("Role");
        public void ShowEditPanelForRole(RoleViewModel selectedRole) => ShowPanelForEntity("Role", selectedRole);

        private void ToggleAddPanel()
        {
            if (isAddPanelVisible)
                HideAddEditPanel();
            else
                ShowAddEditPanel();
        }

        public void ShowAddEditPanel()
        {
            LeftColumn.Width = new GridLength(30);
            RightColumn.Width = new GridLength(300);
            AddEditPanelControl.Visibility = Visibility.Visible;
            isAddPanelVisible = true;
        }

        public void HideAddEditPanel()
        {
            LeftColumn.Width = new GridLength(320);
            RightColumn.Width = new GridLength(0);
            AddEditPanelControl.Visibility = Visibility.Collapsed;
            isAddPanelVisible = false;

            // Очищаем форму при закрытии панели
            AddEditPanelControl.ClearForm();
        }

        private void DefaultElements()
        {
            EventsPanelControl.Visibility = Visibility.Collapsed;
            CategoriesPanelControl.Visibility = Visibility.Collapsed;
            VenuesPanelControl.Visibility = Visibility.Collapsed;
            ClientsPanelControl.Visibility = Visibility.Collapsed;
            UsersPanelControl.Visibility = Visibility.Collapsed;
            RolesPanelControl.Visibility = Visibility.Collapsed;
            SettingPanelControl.Visibility = Visibility.Collapsed;

            var menuItems = new[] { EventItem, CategoryItem, VenueItem, SettingsItem, ExitItem, ClientItem, UserItem, RoleItem };
            foreach (var item in menuItems)
            {
                item.Background = TransparentBrush;
            }

            var buttons = new[] { btnEvent, btnCategory, btnVenues, btnSettings, btnExit, btnClient, btnUser, btnRole };
            foreach (var button in buttons)
            {
                button.Foreground = WhiteBrush;
            }

            ResetIconsToWhite();
        }

        private void ResetIconsToWhite()
        {
            SetIcon(IconEvent, "EventWhite.png");
            SetIcon(IconCategory, "CategoryWhite.png");
            SetIcon(IconVenue, "mapWhite.png");
            SetIcon(IconSetting, "SettingWhite.png");
            SetIcon(IconExit, "LogoutWhite.png");
            SetIcon(IconClient, "ClientsWhite.png");
            SetIcon(IconRole, "RoleWhite.png");
            SetIcon(IconUser, "userSettingWhite.png");
        }

        private void ActivatePanel(string panelType, UserControl panel, Border menuItem, Button button, Image icon, string iconBaseName)
        {
            DefaultElements();

            panel.Visibility = Visibility.Visible;

            menuItem.Background = WhiteBrush;
            button.Foreground = ActiveBrush; // Теперь используем актуальный цвет

            SetIcon(icon, GetIconName(iconBaseName));

            HideAddEditPanel();
        }

        private void SetIcon(Image image, string iconName)
        {
            if (image != null)
            {
                image.Source = new BitmapImage(new Uri($"/EventManagmentApp;component/img/{iconName}", UriKind.Relative));
            }
        }

        private void BtnEvent_Click(object sender, RoutedEventArgs e)
        {
            ActivatePanel("Events", EventsPanelControl, EventItem, btnEvent, IconEvent, "Event");
        }

        private void btnCategory_Click(object sender, RoutedEventArgs e)
        {
            ActivatePanel("Categories", CategoriesPanelControl, CategoryItem, btnCategory, IconCategory, "Category");
        }

        private void btnVenues_Click(object sender, RoutedEventArgs e)
        {
            ActivatePanel("Venues", VenuesPanelControl, VenueItem, btnVenues, IconVenue, "map");
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ActivatePanel("Settings", SettingPanelControl, SettingsItem, btnSettings, IconSetting, "Setting");
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var authentication = new Authentication();
            authentication.Show();
            this.Close();
        }

        private void btnClient_Click(object sender, RoutedEventArgs e)
        {
            ActivatePanel("Clients", ClientsPanelControl, ClientItem, btnClient, IconClient, "Clients");
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            ActivatePanel("Users", UsersPanelControl, UserItem, btnUser, IconUser, "userSetting");
        }

        private void btnRole_Click(object sender, RoutedEventArgs e)
        {
            ActivatePanel("Roles", RolesPanelControl, RoleItem, btnRole, IconRole, "Role");
        }

        // Старый метод для обратной совместимости
        public void ShowEditPanel()
        {
            // Просто показываем панель редактирования без установки данных
            ShowAddEditPanel();
        }

        private void InitializeAddPanel(string entityType)
        {
            try
            {
                data.addEditTable = entityType;

                // Сбрасываем все выделенные объекты (устанавливаем в null для добавления)
                AddEditPanelControl.SelectedEvent = null;
                AddEditPanelControl.SelectedCategory = null;
                AddEditPanelControl.SelectedVenue = null;
                AddEditPanelControl.SelectedClient = null;
                AddEditPanelControl.SelectedUser = null;
                AddEditPanelControl.SelectedRole = null;

                // Генерируем поля формы
                AddEditPanelControl.GenerateFormFields();
                ShowAddEditPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы добавления: {ex.Message}", "Ошибка");
            }
        }

        // Вспомогательный метод для поиска дочерних элементов определенного типа
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
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