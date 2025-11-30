using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EventManagmentApp
{
    public partial class CategoryPanel : UserControl
    {
        private CategoriesPanelViewModel _viewModel;

        public ICommand ToggleAddPanelCommand { get; set; }

        public CategoryPanel()
        {
            InitializeComponent();
            _viewModel = new CategoriesPanelViewModel();
            DataContext = _viewModel;
            Loaded += CategoryPanel_Loaded;
        }

        private void CategoryPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadCategories();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                _viewModel.SearchText = textBox.Text;
            }
        }

        private void btnAllRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ClearAllFilters();
                SearchBox.Text = string.Empty;
                CategoriesDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении данных: {ex.Message}");
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Дополнительная проверка на сервере
                if (data.roleAuth == 1)
                {
                    MessageBox.Show("У вас недостаточно прав для добавления категорий", "Ошибка прав доступа");
                    return;
                }

                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowAddPanelForCategory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }


        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Дополнительная проверка на сервере
                if (data.roleAuth == 1)
                {
                    MessageBox.Show("У вас недостаточно прав для редактирования категорий", "Ошибка прав доступа");
                    return;
                }

                if (_viewModel.SelectedCategory == null)
                {
                    MessageBox.Show("Выберите категорию для редактирования", "Информация");
                    return;
                }

                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ShowEditPanelForCategory(_viewModel.SelectedCategory);
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

        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Дополнительная проверка на сервере
                if (data.roleAuth == 1)
                {
                    MessageBox.Show("У вас недостаточно прав для удаления категорий", "Ошибка прав доступа");
                    return;
                }

                if (_viewModel.SelectedCategory == null)
                {
                    MessageBox.Show("Выберите категорию для удаления", "Информация");
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить категорию \"{_viewModel.SelectedCategory.CategoryName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.DeleteSelectedCategory();
                    MessageBox.Show("Категория успешно удалена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }

        public void RefreshData()
        {
            _viewModel.RefreshData();
            CategoriesDataGrid.Items.Refresh();
        }
    }
}