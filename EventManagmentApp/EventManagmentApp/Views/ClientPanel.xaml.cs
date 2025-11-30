using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EventManagmentApp
{
    public partial class ClientPanel : UserControl
    {
        private ClientsPanelViewModel _viewModel;
        public ICommand ToggleAddPanelCommand { get; set; }

        public ClientPanel()
        {
            InitializeComponent();

            _viewModel = new ClientsPanelViewModel();
            DataContext = _viewModel;

            Loaded += ClientPanel_Loaded;
        }

        private void ClientPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadClients();
        }

        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowAddPanelForClient();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedClient == null)
                {
                    MessageBox.Show("Выберите клиента для редактирования", "Информация");
                    return;
                }

                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowEditPanelForClient(_viewModel.SelectedClient);
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

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedClient == null)
                {
                    MessageBox.Show("Выберите клиента для удаления", "Информация");
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить клиента \"{_viewModel.SelectedClient.FullName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.DeleteClient(_viewModel.SelectedClient);
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

        public void RefreshData()
        {
            _viewModel.LoadClients();
        }

        private void btnAllRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ClearAllFilters();
                SearchBox.Text = string.Empty;
                _viewModel.LoadClients();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}