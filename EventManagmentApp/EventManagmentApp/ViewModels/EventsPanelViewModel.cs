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
    public class EventsPanelViewModel : INotifyPropertyChanged
    {
        //разделяем возможности ролей
        public bool IsAdmin => data.roleAuth == 0;
        private readonly ApplicationContext _context;
        private string _searchText = string.Empty;
        private ObservableCollection<EventViewModel> _events;
        private ICollectionView _eventsView;
        private EventViewModel _selectedEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        public EventsPanelViewModel()
        {
            _context = new ApplicationContext();
            LoadEventsCommand = new RelayCommand(_ => LoadEvents());
            ApplyBudgetFilterCommand = new RelayCommand(_ => ApplyBudgetFilter());
            ApplyGuestsFilterCommand = new RelayCommand(_ => ApplyGuestsFilter());
            AddNewRecordCommand = new RelayCommand(_ => AddNewRecord());
            EditEventCommand = new RelayCommand(param => EditEvent(param as EventViewModel));
            DeleteEventCommand = new RelayCommand(param => DeleteEvent(param as EventViewModel));

            SelectedStatuses = new List<string>();
            LoadEvents(); // Загружаем данные при создании
        }

        public ObservableCollection<EventViewModel> Events
        {
            get => _events;
            set
            {
                _events = value;
                OnPropertyChanged(nameof(Events));
                OnPropertyChanged(nameof(SelectedEventsCount));
            }
        }

        public ICollectionView EventsView => _eventsView;

        public EventViewModel SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                OnPropertyChanged(nameof(SelectedEvent));
                OnPropertyChanged(nameof(IsEventSelected));
            }
        }

        public bool IsEventSelected => SelectedEvent != null;

        public void DeleteEvent(EventViewModel eventItem)
        {
            if (eventItem != null)
            {
                var result = System.Windows.MessageBox.Show($"Вы уверены, что хотите удалить событие \"{eventItem.EventName}\"?",
                    "Подтверждение удаления", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        var eventToDelete = _context.Event.Find(eventItem.EventId);
                        if (eventToDelete != null)
                        {
                            _context.Event.Remove(eventToDelete);
                            _context.SaveChanges();
                            LoadEvents(); // Перезагружаем данные
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
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _eventsView?.Refresh();
                OnPropertyChanged(nameof(SelectedEventsCount));
            }
        }

        public List<string> SelectedStatuses { get; set; }
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }
        public int MinGuests { get; set; }
        public int MaxGuests { get; set; }

        public int SelectedEventsCount => Events?.Count(e => e.IsSelected) ?? 0;

        public ICommand LoadEventsCommand { get; }
        public ICommand ApplyBudgetFilterCommand { get; }
        public ICommand ApplyGuestsFilterCommand { get; }
        public ICommand AddNewRecordCommand { get; }
        public ICommand EditEventCommand { get; }
        public ICommand DeleteEventCommand { get; }

        public void LoadEvents()
        {
            try
            {
                var events = _context.Event
                    .Include(e => e.EventCategories)
                    .Include(e => e.Venues)
                    .Include(e => e.Users)
                    .ThenInclude(u => u.Role)
                    .Include(e => e.Clients)
                    .Select(e => new EventViewModel
                    {
                        EventId = e.EventId,
                        EventName = e.EventName,
                        Description = e.Description ?? string.Empty,
                        DateTimeStart = e.DateTimeStart,
                        DateTimeFinish = e.DateTimeFinish,
                        CategoryId = e.CategoryId, // Добавляем
                        VenueId = e.VenueId, // Добавляем
                        CategoryName = e.EventCategories != null ? e.EventCategories.CategoryName : "Не указана",
                        VenueName = e.Venues != null ? e.Venues.VenueName : "Не указано",
                        UserName = e.Users != null ? $"{e.Users.LastName} {e.Users.Name}" : "Не указан",
                        UserId = e.UserId,
                        Status = e.Status ?? "Не указан",
                        EstimatedBudget = e.EstimatedBudget,
                        ActualBudget = e.ActualBudget,
                        MaxNumOfGuests = e.MaxNumOfGuests,
                        ClientNames = e.Clients.Select(c => $"{c.LastName} {c.Name}").ToList()
                    })
                    .ToList();

                Events = new ObservableCollection<EventViewModel>(events);
                _eventsView = CollectionViewSource.GetDefaultView(Events);
                _eventsView.Filter = FilterEvents;

                OnPropertyChanged(nameof(SelectedEventsCount));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки событий: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool FilterEvents(object item)
        {
            if (item is EventViewModel eventItem)
            {
                // Поиск по тексту
                bool searchFilter = string.IsNullOrWhiteSpace(SearchText) ||
                    eventItem.EventName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    eventItem.Description?.ToLower().Contains(SearchText.ToLower()) == true ||
                    eventItem.CategoryName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    eventItem.VenueName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    eventItem.Status?.ToLower().Contains(SearchText.ToLower()) == true ||
                    eventItem.UserName?.ToLower().Contains(SearchText.ToLower()) == true ||
                    eventItem.ClientNames.Any(c => c.ToLower().Contains(SearchText.ToLower()));

                // Фильтр по статусам
                bool statusFilter = !SelectedStatuses.Any() ||
                    SelectedStatuses.Contains(eventItem.Status);

                // Фильтр по бюджету
                bool budgetFilter = (MinBudget == 0 && MaxBudget == 0) ||
                    (eventItem.EstimatedBudget >= MinBudget &&
                     (MaxBudget == 0 || eventItem.EstimatedBudget <= MaxBudget));

                // Фильтр по количеству гостей
                bool guestsFilter = (MinGuests == 0 && MaxGuests == 0) ||
                    (eventItem.MaxNumOfGuests >= MinGuests &&
                     (MaxGuests == 0 || eventItem.MaxNumOfGuests <= MaxGuests));

                return searchFilter && statusFilter && budgetFilter && guestsFilter;
            }

            return false;
        }

        public void UpdateStatusFilter(List<string> statuses)
        {
            SelectedStatuses = statuses;
            _eventsView?.Refresh();
            OnPropertyChanged(nameof(SelectedEventsCount));
        }

        public void ApplyBudgetFilter()
        {
            _eventsView?.Refresh();
            OnPropertyChanged(nameof(SelectedEventsCount));
        }

        public void ApplyGuestsFilter()
        {
            _eventsView?.Refresh();
            OnPropertyChanged(nameof(SelectedEventsCount));
        }

        public void ClearAllFilters()
        {
            SearchText = string.Empty;
            SelectedStatuses.Clear();
            MinBudget = 0;
            MaxBudget = 0;
            MinGuests = 0;
            MaxGuests = 0;
            _eventsView?.Refresh();
            OnPropertyChanged(nameof(SelectedEventsCount));
        }

        // Новый метод для принудительного обновления
        public void RefreshData()
        {
            LoadEvents();
        }

        private void AddNewRecord()
        {
            System.Windows.MessageBox.Show("Добавление новой записи", "Информация",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void EditEvent(EventViewModel eventItem)
        {
            if (eventItem != null)
            {
                System.Windows.MessageBox.Show($"Редактирование события: {eventItem.EventName}", "Редактирование",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EventViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeFinish { get; set; }
        public int UserId { get; set; }

        // Добавляем недостающие свойства
        public int CategoryId { get; set; }
        public int VenueId { get; set; }

        public string CategoryName { get; set; }
        public string VenueName { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public decimal? EstimatedBudget { get; set; }
        public decimal? ActualBudget { get; set; }
        public int? MaxNumOfGuests { get; set; }
        public List<string> ClientNames { get; set; } = new List<string>();

        public string ClientsDisplay => ClientNames != null && ClientNames.Any()
            ? string.Join(", ", ClientNames)
            : "Клиенты не указаны";

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