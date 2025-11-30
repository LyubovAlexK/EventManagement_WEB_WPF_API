using EventManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace EventManagmentApp
{
    public class UsersPanelViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationContext _context;
        private string _searchText = string.Empty;
        private ObservableCollection<UserViewModel> _users;
        private ICollectionView _usersView;
        private UserViewModel _selectedUser;

        public event PropertyChangedEventHandler PropertyChanged;

        public UsersPanelViewModel()
        {
            _context = new ApplicationContext();
            LoadUsersCommand = new RelayCommand(_ => LoadUsers());
            AddNewRecordCommand = new RelayCommand(_ => AddNewRecord());
            EditUserCommand = new RelayCommand(param => EditUser(param as UserViewModel));
            DeleteUserCommand = new RelayCommand(param => DeleteUser(param as UserViewModel));

            LoadUsers();
        }

        public ObservableCollection<UserViewModel> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        public ICollectionView UsersView => _usersView;

        public UserViewModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
                OnPropertyChanged(nameof(IsUserSelected));
            }
        }

        public bool IsUserSelected => SelectedUser != null;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _usersView?.Refresh();
            }
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand AddNewRecordCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public void LoadUsers()
        {
            try
            {
                var users = _context.Users
                    .Include(u => u.Role)
                    .Select(u => new UserViewModel
                    {
                        UserId = u.UserId,
                        LastName = u.LastName,
                        Name = u.Name,
                        MiddleName = u.MiddleName ?? string.Empty,
                        Phone = u.Phone ?? string.Empty,
                        Specialty = u.Specialty ?? string.Empty,
                        Login = u.Login,
                        Password = u.Password,
                        RoleId = u.RoleId,
                        RoleName = u.Role != null ? u.Role.RoleName : "Не указана"
                    })
                    .ToList();

                Users = new ObservableCollection<UserViewModel>(users);
                _usersView = CollectionViewSource.GetDefaultView(Users);
                _usersView.Filter = FilterUsers;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool FilterUsers(object item)
        {
            if (item is UserViewModel userItem)
            {
                // Поиск по тексту
                bool searchFilter = string.IsNullOrWhiteSpace(SearchText) ||
                    userItem.LastName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    userItem.Name?.ToLower().Contains(SearchText.ToLower()) == true ||
                    userItem.MiddleName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    userItem.Phone?.ToLower().Contains(SearchText.ToLower()) == true ||
                    userItem.Specialty?.ToLower().Contains(SearchText.ToLower()) == true ||
                    userItem.Login?.ToLower().Contains(SearchText.ToLower()) == true ||
                    userItem.RoleName?.ToLower().Contains(SearchText.ToLower()) == true;

                return searchFilter;
            }

            return false;
        }

        public void ClearAllFilters()
        {
            SearchText = string.Empty;
            _usersView?.Refresh();
        }

        public void RefreshData()
        {
            LoadUsers();
        }

        public void DeleteUser(UserViewModel userItem)
        {
            if (userItem != null)
            {
                // Проверка на последнего администратора
                if (IsLastAdmin(userItem))
                {
                    System.Windows.MessageBox.Show(
                        "Невозможно удалить последнего администратора системы!",
                        "Ошибка удаления",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var result = System.Windows.MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя \"{userItem.FullName}\"?",
                    "Подтверждение удаления",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        var userToDelete = _context.Users.Find(userItem.UserId);
                        if (userToDelete != null)
                        {
                            _context.Users.Remove(userToDelete);
                            _context.SaveChanges();
                            LoadUsers();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool IsLastAdmin(UserViewModel userToDelete)
        {
            try
            {
                // Предполагаем, что роль администратора имеет RoleId = 1
                // Если это не так, нужно настроить соответствующим образом
                const int adminRoleId = 1;

                // Если удаляемый пользователь не администратор - можно удалять
                if (userToDelete.RoleId != adminRoleId)
                    return false;

                // Считаем количество оставшихся администраторов
                var remainingAdmins = _context.Users
                    .Where(u => u.RoleId == adminRoleId && u.UserId != userToDelete.UserId)
                    .Count();

                return remainingAdmins == 0;
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
            mainWindow?.ShowAddPanelForUser();
        }

        private void EditUser(UserViewModel userItem)
        {
            if (userItem != null)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowEditPanelForUser(userItem);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int UserId { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public string Phone { get; set; }
        public string Specialty { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public string FullName => $"{LastName} {Name} {MiddleName}".Trim();
        public string FullNameShort => $"{LastName} {Name} {(!string.IsNullOrEmpty(MiddleName) ? MiddleName[0] + "." : "")}".Trim();

        public string DisplayName => $"{LastName} {Name}";

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