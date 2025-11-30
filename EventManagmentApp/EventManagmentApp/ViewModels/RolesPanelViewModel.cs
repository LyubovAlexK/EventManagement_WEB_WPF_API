using EventManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace EventManagmentApp
{
    public class RolesPanelViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationContext _context;
        private string _searchText = string.Empty;
        private ObservableCollection<RoleViewModel> _roles;
        private ICollectionView _rolesView;
        private RoleViewModel _selectedRole;

        public event PropertyChangedEventHandler PropertyChanged;

        public RolesPanelViewModel()
        {
            _context = new ApplicationContext();
            LoadRolesCommand = new RelayCommand(_ => LoadRoles());
            AddNewRecordCommand = new RelayCommand(_ => AddNewRecord());
            EditRoleCommand = new RelayCommand(param => EditRole(param as RoleViewModel));
            DeleteRoleCommand = new RelayCommand(param => DeleteRole(param as RoleViewModel));

            LoadRoles();
        }

        public ObservableCollection<RoleViewModel> Roles
        {
            get => _roles;
            set
            {
                _roles = value;
                OnPropertyChanged(nameof(Roles));
            }
        }

        public ICollectionView RolesView => _rolesView;

        public RoleViewModel SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
                OnPropertyChanged(nameof(IsRoleSelected));
            }
        }

        public bool IsRoleSelected => SelectedRole != null;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _rolesView?.Refresh();
            }
        }

        public ICommand LoadRolesCommand { get; }
        public ICommand AddNewRecordCommand { get; }
        public ICommand EditRoleCommand { get; }
        public ICommand DeleteRoleCommand { get; }

        public void LoadRoles()
        {
            try
            {
                var roles = _context.Roles
                    .Select(r => new RoleViewModel
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName
                    })
                    .ToList();

                Roles = new ObservableCollection<RoleViewModel>(roles);
                _rolesView = CollectionViewSource.GetDefaultView(Roles);
                _rolesView.Filter = FilterRoles;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool FilterRoles(object item)
        {
            if (item is RoleViewModel roleItem)
            {
                // Поиск по тексту
                bool searchFilter = string.IsNullOrWhiteSpace(SearchText) ||
                    roleItem.RoleName?.ToLower().Contains(SearchText.ToLower()) == true;

                return searchFilter;
            }

            return false;
        }

        public void ClearAllFilters()
        {
            SearchText = string.Empty;
            _rolesView?.Refresh();
        }

        public void RefreshData()
        {
            LoadRoles();
        }

        public void DeleteRole(RoleViewModel roleItem)
        {
            if (roleItem != null)
            {
                // Проверяем, есть ли пользователи с этой ролью
                if (HasUsersWithRole(roleItem.RoleId))
                {
                    MessageBox.Show(
                        "Невозможно удалить роль, так как есть пользователи с этой ролью. " +
                        "Сначала измените роли у пользователей или удалите их.",
                        "Ошибка удаления",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить роль \"{roleItem.RoleName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var roleToDelete = _context.Roles.Find(roleItem.RoleId);
                        if (roleToDelete != null)
                        {
                            _context.Roles.Remove(roleToDelete);
                            _context.SaveChanges();
                            LoadRoles();
                            MessageBox.Show("Роль успешно удалена!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Метод проверки наличия пользователей с указанной ролью
        private bool HasUsersWithRole(int roleId)
        {
            try
            {
                return _context.Users.Any(u => u.RoleId == roleId);
            }
            catch (Exception)
            {
                // В случае ошибки лучше не разрешать удаление
                return true;
            }
        }

        private void AddNewRecord()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowAddPanelForRole();
        }

        private void EditRole(RoleViewModel roleItem)
        {
            if (roleItem != null)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowEditPanelForRole(roleItem);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RoleViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}