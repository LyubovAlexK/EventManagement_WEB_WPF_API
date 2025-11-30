using EventManagement.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace EventManagmentApp
{
    public class CategoriesPanelViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationContext _context;
        private string _searchText;
        private CategoryViewModel _selectedCategory;
        private ICollectionView _categoriesView;

        public bool IsUserAdmin => data.roleAuth == 0;
        public bool IsCategorySelectedAndAdmin => IsCategorySelected && IsUserAdmin;

        public ObservableCollection<CategoryViewModel> Categories { get; } = new ObservableCollection<CategoryViewModel>();

        public ICollectionView CategoriesView
        {
            get
            {
                if (_categoriesView == null)
                {
                    _categoriesView = CollectionViewSource.GetDefaultView(Categories);
                    _categoriesView.Filter = FilterCategories;
                }
                return _categoriesView;
            }
        }

        public ICommand ToggleAddPanelCommand { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                CategoriesView?.Refresh();
            }
        }

        public CategoryViewModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                OnPropertyChanged(nameof(IsCategorySelected));
            }
        }

        public bool IsCategorySelected => SelectedCategory != null;

        public event PropertyChangedEventHandler PropertyChanged;

        public CategoriesPanelViewModel()
        {
            _context = new ApplicationContext();
            LoadCategories();
            OnPropertyChanged(nameof(IsUserAdmin));
            OnPropertyChanged(nameof(IsCategorySelectedAndAdmin));
        }

        public void LoadCategories()
        {
            // СОЗДАЕМ НОВЫЙ КОНТЕКСТ ДЛЯ КАЖДОЙ ЗАГРУЗКИ
            using (var freshContext = new ApplicationContext())
            {
                var categories = freshContext.EventCategories.ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Categories.Clear();

                    foreach (var category in categories)
                    {
                        Categories.Add(new CategoryViewModel
                        {
                            CategoryId = category.CategoryId,
                            CategoryName = category.CategoryName
                        });
                    }

                });
            }
        }

        private bool FilterCategories(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (obj is CategoryViewModel category)
            {
                return category.CategoryName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                       category.CategoryId.ToString().IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        public void DeleteSelectedCategory()
        {
            if (SelectedCategory == null) return;

            try
            {
                // Проверяем, есть ли связанные события
                if (HasEventsWithCategory(SelectedCategory.CategoryId))
                {
                    MessageBox.Show(
                        "Невозможно удалить категорию, так как с ней связаны события. " +
                        "Сначала измените категории у событий или удалите их.",
                        "Ошибка удаления",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var categoryToDelete = _context.EventCategories.Find(SelectedCategory.CategoryId);
                if (categoryToDelete != null)
                {
                    _context.EventCategories.Remove(categoryToDelete);
                    _context.SaveChanges();

                    // Удаляем из ObservableCollection
                    var categoryToRemove = Categories.FirstOrDefault(c => c.CategoryId == SelectedCategory.CategoryId);
                    if (categoryToRemove != null)
                    {
                        Categories.Remove(categoryToRemove);
                    }

                    SelectedCategory = null;
                    MessageBox.Show("Категория успешно удалена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления категории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод проверки наличия событий с указанной категорией
        private bool HasEventsWithCategory(int categoryId)
        {
            try
            {
                return _context.Event.Any(e => e.CategoryId == categoryId);
            }
            catch (Exception)
            {
                return true;
            }
        }
        // Новый метод для принудительного обновления
        public void RefreshData()
        {
            LoadCategories();
        }
        public void ClearAllFilters()
        {
            SearchText = string.Empty;
            LoadCategories(); // Перезагружаем данные
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // При изменении SelectedCategory обновляем составное свойство
            if (propertyName == nameof(SelectedCategory))
            {
                OnPropertyChanged(nameof(IsCategorySelectedAndAdmin));
            }
        }
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}