using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EventManagmentApp
{
    public partial class RolePanel : UserControl
    {
        private RolesPanelViewModel _viewModel;
        public ICommand ToggleAddPanelCommand { get; set; }

        public RolePanel()
        {
            InitializeComponent();

            _viewModel = new RolesPanelViewModel();
            DataContext = _viewModel;

            Loaded += RolePanel_Loaded;
        }

        private void RolePanel_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadRoles();
        }

        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowAddPanelForRole();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void EditRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedRole == null)
                {
                    MessageBox.Show("Выберите роль для редактирования", "Информация");
                    return;
                }

                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowEditPanelForRole(_viewModel.SelectedRole);
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

        private void DeleteRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedRole == null)
                {
                    MessageBox.Show("Выберите роль для удаления", "Информация");
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить роль \"{_viewModel.SelectedRole.RoleName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.DeleteRole(_viewModel.SelectedRole);
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
            _viewModel.LoadRoles();
        }

        private void btnAllRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ClearAllFilters();
                SearchBox.Text = string.Empty;
                _viewModel.LoadRoles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}