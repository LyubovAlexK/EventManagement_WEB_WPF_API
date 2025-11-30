using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EventManagmentApp
{
    public partial class UserPanel : UserControl
    {
        private UsersPanelViewModel _viewModel;
        public ICommand ToggleAddPanelCommand { get; set; }

        public UserPanel()
        {
            InitializeComponent();

            _viewModel = new UsersPanelViewModel();
            DataContext = _viewModel;

            Loaded += UserPanel_Loaded;
        }

        private void UserPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadUsers();
        }

        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowAddPanelForUser();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedUser == null)
                {
                    MessageBox.Show("Выберите пользователя для редактирования", "Информация");
                    return;
                }

                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowEditPanelForUser(_viewModel.SelectedUser);
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

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedUser == null)
                {
                    MessageBox.Show("Выберите пользователя для удаления", "Информация");
                    return;
                }

                _viewModel.DeleteUser(_viewModel.SelectedUser);
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
            _viewModel.LoadUsers();
        }

        private void btnAllRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ClearAllFilters();
                SearchBox.Text = string.Empty;
                _viewModel.LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}