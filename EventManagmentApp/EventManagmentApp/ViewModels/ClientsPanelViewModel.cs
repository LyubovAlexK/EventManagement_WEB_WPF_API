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
    public class ClientsPanelViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationContext _context;
        private string _searchText = string.Empty;
        private ObservableCollection<ClientViewModel> _clients;
        private ICollectionView _clientsView;
        private ClientViewModel _selectedClient;

        public event PropertyChangedEventHandler PropertyChanged;

        public ClientsPanelViewModel()
        {
            _context = new ApplicationContext();
            LoadClientsCommand = new RelayCommand(_ => LoadClients());
            AddNewRecordCommand = new RelayCommand(_ => AddNewRecord());
            EditClientCommand = new RelayCommand(param => EditClient(param as ClientViewModel));
            DeleteClientCommand = new RelayCommand(param => DeleteClient(param as ClientViewModel));

            LoadClients();
        }

        public ObservableCollection<ClientViewModel> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        public ICollectionView ClientsView => _clientsView;

        public ClientViewModel SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                OnPropertyChanged(nameof(IsClientSelected));
            }
        }

        public bool IsClientSelected => SelectedClient != null;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _clientsView?.Refresh();
            }
        }

        public ICommand LoadClientsCommand { get; }
        public ICommand AddNewRecordCommand { get; }
        public ICommand EditClientCommand { get; }
        public ICommand DeleteClientCommand { get; }

        public void LoadClients()
        {
            try
            {
                var clients = _context.Clients
                    .Include(c => c.Events)
                    .Select(c => new ClientViewModel
                    {
                        ClientId = c.ClientId,
                        LastName = c.LastName,
                        Name = c.Name,
                        MiddleName = c.MiddleName ?? string.Empty,
                        Email = c.Email ?? string.Empty,
                        Phone = c.Phone ?? string.Empty,
                        EventId = c.EventId,
                        EventName = c.Events != null ? c.Events.EventName : "Не указано"
                    })
                    .ToList();

                Clients = new ObservableCollection<ClientViewModel>(clients);
                _clientsView = CollectionViewSource.GetDefaultView(Clients);
                _clientsView.Filter = FilterClients;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool FilterClients(object item)
        {
            if (item is ClientViewModel clientItem)
            {
                // Поиск по тексту
                bool searchFilter = string.IsNullOrWhiteSpace(SearchText) ||
                    clientItem.LastName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    clientItem.Name?.ToLower().Contains(SearchText.ToLower()) == true ||
                    clientItem.MiddleName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    clientItem.Email?.ToLower().Contains(SearchText.ToLower()) == true ||
                    clientItem.Phone?.ToLower().Contains(SearchText.ToLower()) == true ||
                    clientItem.EventName?.ToLower().Contains(SearchText.ToLower()) == true;

                return searchFilter;
            }

            return false;
        }

        public void ClearAllFilters()
        {
            SearchText = string.Empty;
            _clientsView?.Refresh();
        }

        public void RefreshData()
        {
            LoadClients();
        }

        public void DeleteClient(ClientViewModel clientItem)
        {
            if (clientItem != null)
            {
                try
                {
                    var clientToDelete = _context.Clients.Find(clientItem.ClientId);
                    if (clientToDelete != null)
                    {
                        _context.Clients.Remove(clientToDelete);
                        _context.SaveChanges();
                        LoadClients();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }

            }
        }

        private void AddNewRecord()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowAddPanelForClient();
        }

        private void EditClient(ClientViewModel clientItem)
        {
            if (clientItem != null)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowEditPanelForClient(clientItem);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ClientViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int ClientId { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }

        public string FullName => $"{LastName} {Name} {MiddleName}".Trim();
        public string FullNameShort => $"{LastName} {Name} {(!string.IsNullOrEmpty(MiddleName) ? MiddleName[0] + "." : "")}".Trim();

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