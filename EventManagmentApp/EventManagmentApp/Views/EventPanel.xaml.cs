using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace EventManagmentApp
{
    public partial class EventPanel : UserControl
    {
        private EventsPanelViewModel _viewModel;
        private string currentVisibleFilter = "";

        public ICommand ToggleAddPanelCommand { get; set; }

        public EventPanel()
        {
            InitializeComponent();

            _viewModel = new EventsPanelViewModel();
            DataContext = _viewModel;

            Loaded += EventPanel_Loaded;
        }

        public void UpdateUIForUserRole(int roleAuth)
        {
            if (roleAuth == 1)
            {
                btnEdit.Margin = new Thickness(0, 0, 0, 0);
            }
        }
        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedEvent == null)
                {
                    MessageBox.Show("Выберите событие для редактирования", "Информация");
                    return;
                }

                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    if (data.roleAuth == 0) // Админ - полное редактирование
                    {
                        mainWindow.ShowEditPanelForEvent(_viewModel.SelectedEvent);
                    }
                    else // Обычный пользователь - только реальный бюджет
                    {
                        mainWindow.ShowEditPanelForEvent(_viewModel.SelectedEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void EventPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadEvents();
            InitializeFilterEvents();
        }

        private void btnStatusFilter_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterPanel("Status");
            SetImageFromResources(ArowStatusFilter, "ArowUp.png");
            SetImageFromResources(ArowBudgetFilter, "ArowDown.png");
            SetImageFromResources(ArowCountGuestsFilter, "ArowDown.png");
        }

        private void btnBudgetFilter_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterPanel("Budget");
            SetImageFromResources(ArowStatusFilter, "ArowDown.png");
            SetImageFromResources(ArowBudgetFilter, "ArowUp.png");
            SetImageFromResources(ArowCountGuestsFilter, "ArowDown.png");
        }

        private void btnCountGuestsFilter_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterPanel("CountGuests");
            SetImageFromResources(ArowStatusFilter, "ArowDown.png");
            SetImageFromResources(ArowBudgetFilter, "ArowDown.png");
            SetImageFromResources(ArowCountGuestsFilter, "ArowUp.png");
        }

        private void InitializeFilterEvents()
        {
            // Обработчики для контейнеров
            StatusFilterContainer.MouseLeftButtonDown += (s, e) => ToggleFilterPanel("Status");
            BudgetFilterContainer.MouseLeftButtonDown += (s, e) => ToggleFilterPanel("Budget");
            CountGuestsFilterContainer.MouseLeftButtonDown += (s, e) => ToggleFilterPanel("CountGuests");

            // Обработчики для кнопок
            btnStatusFilter.Click += btnStatusFilter_Click;
            btnBudgetFilter.Click += btnBudgetFilter_Click;
            btnCountGuestsFilter.Click += btnCountGuestsFilter_Click;
        }

        private void ToggleFilterPanel(string filterName)
        {
            if (currentVisibleFilter == filterName)
            {
                currentVisibleFilter = "";
                CloseAllFilterPanels();
            }
            else
            {
                currentVisibleFilter = filterName;
                CloseAllFilterPanels();
                BringFilterToFront(filterName);

                switch (filterName)
                {
                    case "Status":
                        ShowStatusFilter();
                        break;
                    case "Budget":
                        ShowBudgetFilter();
                        break;
                    case "CountGuests":
                        ShowGuestsFilter();
                        break;
                }
            }
        }

        private void ShowStatusFilter()
        {
            MenuStatusFilterContainer.Visibility = Visibility.Visible;
            SetImageFromResources(ArowStatusFilter, "ArowUp.png");
        }

        private void ShowBudgetFilter()
        {
            MenuBudgetFilterContainer.Visibility = Visibility.Visible;
            SetImageFromResources(ArowBudgetFilter, "ArowUp.png");
        }

        private void ShowGuestsFilter()
        {
            MenuCountGuestsFilterContainer.Visibility = Visibility.Visible;
            SetImageFromResources(ArowCountGuestsFilter, "ArowUp.png");
        }

        private void CloseAllFilterPanels()
        {
            MenuStatusFilterContainer.Visibility = Visibility.Collapsed;
            MenuBudgetFilterContainer.Visibility = Visibility.Collapsed;
            MenuCountGuestsFilterContainer.Visibility = Visibility.Collapsed;

            SetImageFromResources(ArowStatusFilter, "ArowDown.png");
            SetImageFromResources(ArowBudgetFilter, "ArowDown.png");
            SetImageFromResources(ArowCountGuestsFilter, "ArowDown.png");
        }

        private void BringFilterToFront(string filterName)
        {
            Panel.SetZIndex(MenuStatusFilterContainer, 0);
            Panel.SetZIndex(MenuBudgetFilterContainer, 0);
            Panel.SetZIndex(MenuCountGuestsFilterContainer, 0);

            switch (filterName)
            {
                case "Status":
                    Panel.SetZIndex(MenuStatusFilterContainer, 100);
                    break;
                case "Budget":
                    Panel.SetZIndex(MenuBudgetFilterContainer, 100);
                    break;
                case "CountGuests":
                    Panel.SetZIndex(MenuCountGuestsFilterContainer, 100);
                    break;
            }
        }

        private void StatusCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateStatusFilters();
        }

        private void StatusCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateStatusFilters();
        }

        private void UpdateStatusFilters()
        {
            var selectedStatuses = new List<string>();

            if (ChbAgreed.IsChecked == true) selectedStatuses.Add("Согласован");
            if (ChbInProcessing.IsChecked == true) selectedStatuses.Add("В обработке");
            if (ChbAwaitingApproval.IsChecked == true) selectedStatuses.Add("Ждет утверждения");

            _viewModel.UpdateStatusFilter(selectedStatuses);
        }

        private void ApplyBudgetFilter_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(StartBudgetFilter.Text, out decimal minBudget) &&
                decimal.TryParse(FinishBudgetFilter.Text, out decimal maxBudget))
            {
                _viewModel.MinBudget = minBudget;
                _viewModel.MaxBudget = maxBudget;
                _viewModel.ApplyBudgetFilter();
            }
            else
            {
                MessageBox.Show("Введите корректные значения бюджета", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyGuestsFilter_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(StartGuestsFilter.Text, out int minGuests) &&
                int.TryParse(FinishGuestsFilter.Text, out int maxGuests))
            {
                _viewModel.MinGuests = minGuests;
                _viewModel.MaxGuests = maxGuests;
                _viewModel.ApplyGuestsFilter();
            }
            else
            {
                MessageBox.Show("Введите корректные значения количества гостей", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    if (this is EventPanel)
                    {
                        mainWindow.ShowAddPanelForEvent();
                    }
                    if (this is CategoryPanel)
                    {
                        mainWindow.ShowAddPanelForCategory();
                    }
                    if (this is VenuesPanel)
                    {
                        mainWindow.ShowAddPanelForVenue();
                    }
                    if (this is ClientPanel)
                    {
                        mainWindow.ShowAddPanelForClient();
                    }
                    if (this is UserPanel)
                    {
                        mainWindow.ShowAddPanelForUser();
                    }
                    if (this is RolePanel)
                    {
                        mainWindow.ShowAddPanelForUser();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы добавления: {ex.Message}", "Ошибка");
            }
        }

        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedEvent == null)
                {
                    MessageBox.Show("Выберите событие для удаления", "Информация");
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить событие \"{_viewModel.SelectedEvent.EventName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // ВЫЗЫВАЕМ МЕТОД НАПРЯМУЮ, А НЕ ЧЕРЕЗ COMMAND
                    _viewModel.DeleteEvent(_viewModel.SelectedEvent);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обновляем поиск в ViewModel
            if (sender is TextBox textBox)
            {
                _viewModel.SearchText = textBox.Text;
            }
        }

        private void SetImageFromResources(Image image, string imageName)
        {
            if (image == null) return;
            var uri = new Uri($"/img/{imageName}", UriKind.Relative);
            image.Source = new BitmapImage(uri);
        }

        public void RefreshData()
        {
            _viewModel.LoadEvents();
        }

        private void btnAllRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ClearAllFilters();

                SearchBox.Text = string.Empty;

                ChbAgreed.IsChecked = false;
                ChbInProcessing.IsChecked = false;
                ChbAwaitingApproval.IsChecked = false;

                StartBudgetFilter.Text = string.Empty;
                FinishBudgetFilter.Text = string.Empty;

                StartGuestsFilter.Text = string.Empty;
                FinishGuestsFilter.Text = string.Empty;

                CloseAllFilterPanels();
                currentVisibleFilter = "";

                SetImageFromResources(ArowStatusFilter, "ArowDown.png");
                SetImageFromResources(ArowBudgetFilter, "ArowDown.png");
                SetImageFromResources(ArowCountGuestsFilter, "ArowDown.png");

                _viewModel.LoadEvents();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}