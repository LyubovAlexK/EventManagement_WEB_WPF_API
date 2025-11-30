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
    public class VenuesPanelViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationContext _context;
        private string _searchText = string.Empty;
        private ObservableCollection<VenueViewModel> _venues;
        private ICollectionView _venuesView;
        private VenueViewModel _selectedVenue;

        public event PropertyChangedEventHandler PropertyChanged;

        public VenuesPanelViewModel()
        {
            _context = new ApplicationContext();
            LoadVenuesCommand = new RelayCommand(_ => LoadVenues());
            ApplyCapacityFilterCommand = new RelayCommand(_ => ApplyCapacityFilter());
            AddNewRecordCommand = new RelayCommand(_ => AddNewRecord());
            EditVenueCommand = new RelayCommand(param => EditVenue(param as VenueViewModel));
            DeleteVenueCommand = new RelayCommand(param => DeleteVenue(param as VenueViewModel));

            MinCapacity = 0;
            MaxCapacity = 0;
            LoadVenues();
        }

        public ObservableCollection<VenueViewModel> Venues
        {
            get => _venues;
            set
            {
                _venues = value;
                OnPropertyChanged(nameof(Venues));
            }
        }

        public ICollectionView VenuesView => _venuesView;

        public VenueViewModel SelectedVenue
        {
            get => _selectedVenue;
            set
            {
                _selectedVenue = value;
                OnPropertyChanged(nameof(SelectedVenue));
                OnPropertyChanged(nameof(IsVenueSelected));
            }
        }

        public bool IsVenueSelected => SelectedVenue != null;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _venuesView?.Refresh();
            }
        }

        public int MinCapacity { get; set; }
        public int MaxCapacity { get; set; }

        public ICommand LoadVenuesCommand { get; }
        public ICommand ApplyCapacityFilterCommand { get; }
        public ICommand AddNewRecordCommand { get; }
        public ICommand EditVenueCommand { get; }
        public ICommand DeleteVenueCommand { get; }

        public void LoadVenues()
        {
            try
            {
                var venues = _context.Venues
                    .Select(v => new VenueViewModel
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName,
                        Address = v.Address,
                        Capacity = v.Capacity,
                        Description = v.Description ?? string.Empty
                    })
                    .ToList();

                Venues = new ObservableCollection<VenueViewModel>(venues);
                _venuesView = CollectionViewSource.GetDefaultView(Venues);
                _venuesView.Filter = FilterVenues;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки мест проведения: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool FilterVenues(object item)
        {
            if (item is VenueViewModel venueItem)
            {
                // Поиск по тексту
                bool searchFilter = string.IsNullOrWhiteSpace(SearchText) ||
                    venueItem.VenueName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    venueItem.Address?.ToLower().Contains(SearchText.ToLower()) == true ||
                    venueItem.Description?.ToLower().Contains(SearchText.ToLower()) == true;

                // Фильтр по вместимости
                bool capacityFilter = (MinCapacity == 0 && MaxCapacity == 0) ||
                    (venueItem.Capacity >= MinCapacity &&
                     (MaxCapacity == 0 || venueItem.Capacity <= MaxCapacity));

                return searchFilter && capacityFilter;
            }

            return false;
        }

        public void ApplyCapacityFilter()
        {
            _venuesView?.Refresh();
        }

        public void ClearAllFilters()
        {
            SearchText = string.Empty;
            MinCapacity = 0;
            MaxCapacity = 0;
            _venuesView?.Refresh();
        }

        public void RefreshData()
        {
            LoadVenues();
        }

        public void DeleteVenue(VenueViewModel venueItem)
        {
            if (venueItem != null)
            {
                // Проверяем, есть ли связанные события
                if (HasEventsWithVenue(venueItem.VenueId))
                {
                    MessageBox.Show(
                        "Невозможно удалить место проведения, так как с ним связаны события. " +
                        "Сначала измените места проведения у событий или удалите их.",
                        "Ошибка удаления",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить место проведения \"{venueItem.VenueName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var venueToDelete = _context.Venues.Find(venueItem.VenueId);
                        if (venueToDelete != null)
                        {
                            _context.Venues.Remove(venueToDelete);
                            _context.SaveChanges();
                            LoadVenues();
                            MessageBox.Show("Место проведения успешно удалено!", "Успех",
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

        // Метод проверки наличия событий с указанным местом проведения
        private bool HasEventsWithVenue(int venueId)
        {
            try
            {
                return _context.Event.Any(e => e.VenueId == venueId);
            }
            catch (Exception)
            {
                return true;
            }
        }

        private void AddNewRecord()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowAddPanelForVenue();
        }

        private void EditVenue(VenueViewModel venueItem)
        {
            if (venueItem != null)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowEditPanelForVenue(venueItem);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VenueViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }

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