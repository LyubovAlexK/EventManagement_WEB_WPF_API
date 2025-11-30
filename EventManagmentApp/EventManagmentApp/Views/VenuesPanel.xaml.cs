using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace EventManagmentApp
{
    public partial class VenuesPanel : UserControl
    {
        private VenuesPanelViewModel _viewModel;
        private string currentVisibleFilter = "";

        public ICommand ToggleAddPanelCommand { get; set; }

        public VenuesPanel()
        {
            InitializeComponent();

            _viewModel = new VenuesPanelViewModel();
            DataContext = _viewModel;

            Loaded += VenuesPanel_Loaded;
        }

        private void VenuesPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadVenues();
            InitializeFilterEvents();
        }

        private void InitializeFilterEvents()
        {
            StatusFilterContainer.MouseLeftButtonDown += (s, e) => ToggleFilterPanel("Capacity");
            btnCapacityFilter.Click += btnCapacityFilter_Click;
        }

        private void btnCapacityFilter_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterPanel("Capacity");
            SetImageFromResources(ArowCapacityFilter, "ArowUp.png");
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
                    case "Capacity":
                        ShowCapacityFilter();
                        break;
                }
            }
        }

        private void ShowCapacityFilter()
        {
            MenuCapacityFilterContainer.Visibility = Visibility.Visible;
            SetImageFromResources(ArowCapacityFilter, "ArowUp.png");
        }

        private void CloseAllFilterPanels()
        {
            MenuCapacityFilterContainer.Visibility = Visibility.Collapsed;
            SetImageFromResources(ArowCapacityFilter, "ArowDown.png");
        }

        private void BringFilterToFront(string filterName)
        {
            Panel.SetZIndex(MenuCapacityFilterContainer, 0);

            switch (filterName)
            {
                case "Capacity":
                    Panel.SetZIndex(MenuCapacityFilterContainer, 100);
                    break;
            }
        }

        private void ApplyCapacityFilter_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(StartCapacityFilter.Text, out int minCapacity) &&
                int.TryParse(FinishCapacityFilter.Text, out int maxCapacity))
            {
                _viewModel.MinCapacity = minCapacity;
                _viewModel.MaxCapacity = maxCapacity;
                _viewModel.ApplyCapacityFilter();
            }
            else
            {
                MessageBox.Show("Введите корректные значения вместимости", "Ошибка",
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
                    mainWindow.ShowAddPanelForVenue();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void EditVenue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedVenue == null)
                {
                    MessageBox.Show("Выберите место проведения для редактирования", "Информация");
                    return;
                }

                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowEditPanelForVenue(_viewModel.SelectedVenue);
                }
                else
                {
                    MessageBox.Show("Ошибка отображения формы редактирования", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void DeleteVenue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedVenue == null)
                {
                    MessageBox.Show("Выберите место проведения для удаления", "Информация");
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить место проведения \"{_viewModel.SelectedVenue.VenueName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.DeleteVenue(_viewModel.SelectedVenue);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
            _viewModel.LoadVenues();
        }

        private void btnAllRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ClearAllFilters();

                SearchBox.Text = string.Empty;
                StartCapacityFilter.Text = string.Empty;
                FinishCapacityFilter.Text = string.Empty;

                CloseAllFilterPanels();
                currentVisibleFilter = "";

                SetImageFromResources(ArowCapacityFilter, "ArowDown.png");

                _viewModel.LoadVenues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновляем обработчик кнопки
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterPanel("Capacity");
        }
    }
}