using EventManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EventManagmentApp
{
    public partial class AddEditPanel : UserControl
    {
        private ApplicationContext _context;
        private EventViewModel _selectedEvent;
        private CategoryViewModel _selectedCategory;
        private TextBox _actualBudgetTextBox;
        private RoleViewModel _selectedRole;
        private VenueViewModel _selectedVenue;
        private ClientViewModel _selectedClient;
        private UserViewModel _selectedUser;

        private string _currentPanelType;
        private ComboBox _statusComboBox;
        private ComboBox _eventComboBox;
        private ComboBox _roleComboBox;
        private ComboBox _categoryComboBox;
        private ComboBox _venueComboBox;

        // Коллекции для ComboBox
        private List<EventViewModel> _availableEvents = new List<EventViewModel>();
        private List<RoleViewModel> _availableRoles = new List<RoleViewModel>();

        public ICommand ClosePanelCommand { get; set; }

        public AddEditPanel()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            ClosePanelCommand = new RelayCommand(_ => { });
            Loaded += AddEditPanel_Loaded;
        }

        public class ManagerViewModel
        {
            public int UserId { get; set; }
            public string DisplayName { get; set; }
            public string Specialty { get; set; }
        }

        // Свойства для передачи выбранных объектов
        public EventViewModel SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                if (_selectedEvent != null)
                {
                    LoadEventData();
                }
            }
        }

        public CategoryViewModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                if (_selectedCategory != null)
                {
                    LoadCategoryData();
                }
            }
        }

        public VenueViewModel SelectedVenue
        {
            get => _selectedVenue;
            set
            {
                _selectedVenue = value;
                if (_selectedVenue != null)
                {
                    LoadVenueData();
                }
                else
                {
                    ClearForm();
                }
            }
        }

        public ClientViewModel SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                if (_selectedClient != null)
                {
                    LoadClientData();
                }
            }
        }

        public UserViewModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                if (_selectedUser != null)
                {
                    LoadUserData();
                }
            }
        }

        public RoleViewModel SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                if (_selectedRole != null)
                {
                    LoadRoleData();
                }
            }
        }

        private void AddEditPanel_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateFormFields();
        }

        public void GenerateFormFields()
        {
            try
            {
                if (MainFormStackPanel != null)
                {
                    MainFormStackPanel.Children.Clear();

                    _currentPanelType = data.addEditTable;

                    switch (_currentPanelType)
                    {
                        case "Event":
                            GenerateEventFields();
                            UpdateHeaderText(_selectedEvent != null ? "Редактирование события" : "Добавление события");
                            UpdateButtonText(_selectedEvent != null ? "Изменить" : "Добавить");
                            if (_selectedEvent != null) LoadEventData();
                            break;

                        case "Category":
                            GenerateCategoryFields();
                            UpdateHeaderText(_selectedCategory != null ? "Редактирование категории" : "Добавление категории");
                            UpdateButtonText(_selectedCategory != null ? "Изменить" : "Добавить");
                            if (_selectedCategory != null) LoadCategoryData();
                            break;

                        case "Venue":
                            GenerateVenueFields();
                            UpdateHeaderText(_selectedVenue != null ? "Редактирование места проведения" : "Добавление места проведения");
                            UpdateButtonText(_selectedVenue != null ? "Изменить" : "Добавить");
                            if (_selectedVenue != null) LoadVenueData();
                            break;

                        case "Client":
                            GenerateClientFields();
                            UpdateHeaderText(_selectedClient != null ? "Редактирование клиента" : "Добавление клиента");
                            UpdateButtonText(_selectedClient != null ? "Изменить" : "Добавить");
                            if (_selectedClient != null) LoadClientData();
                            break;

                        case "User":
                            GenerateUserFields();
                            UpdateHeaderText(_selectedUser != null ? "Редактирование пользователя" : "Добавление пользователя");
                            UpdateButtonText(_selectedUser != null ? "Изменить" : "Добавить");
                            if (_selectedUser != null) LoadUserData();
                            break;

                        case "Role":
                            GenerateRoleFields();
                            UpdateHeaderText(_selectedRole != null ? "Редактирование роли" : "Добавление роли");
                            UpdateButtonText(_selectedRole != null ? "Изменить" : "Добавить");
                            if (_selectedRole != null) LoadRoleData();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании формы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReloadFormData()
        {
            if (MainFormStackPanel == null || MainFormStackPanel.Children.Count == 0) return;

            switch (_currentPanelType)
            {
                case "User":
                    if (_selectedUser != null) LoadUserData();
                    break;
                case "Client":
                    if (_selectedClient != null) LoadClientData();
                    break;
                case "Role":
                    if (_selectedRole != null) LoadRoleData();
                    break;
            }
        }

        // ========== GENERATE FIELDS METHODS ==========

        private void GenerateEventFields()
        {
            if (_selectedEvent != null && data.roleAuth != 0) // Редактирование для не-админов
            {
                GenerateEventFieldsReadOnly();
                GenerateEditableActualBudgetField();
            }
            else // Добавление или редактирование для админов
            {
                GenerateEventFieldsEditable();
                if (_selectedEvent != null) // Если редактирование для админа
                {
                    GenerateEditableActualBudgetField();
                }
            }
        }

        private void GenerateEventFieldsReadOnly()
        {
            var readOnlyFields = new Dictionary<string, string>
            {
                { "EventId", "Код события" },
                { "EventName", "Название события" },
                { "Description", "Описание" },
                { "DateTimeStart", "Дата и время начала" },
                { "DateTimeFinish", "Дата и время окончания" },
                { "Status", "Статус" },
                { "EstimatedBudget", "Предполагаемый бюджет" },
                { "MaxNumOfGuests", "Максимальное количество гостей" },
                { "CategoryName", "Категория" },
                { "VenueName", "Место проведения" },
                { "UserId", "Ответственный менеджер" },
                { "ClientsDisplay", "Клиенты" }
            };

            foreach (var field in readOnlyFields)
            {
                var stackPanel = CreateFieldStackPanel(field.Value);

                var textBox = new TextBox
                {
                    Style = (Style)FindResource("InputBox"),
                    Padding = new Thickness(10),
                    TextWrapping = TextWrapping.Wrap,
                    Name = $"TextBox_{field.Key}",
                    IsReadOnly = true,
                    Background = Brushes.LightGray
                };

                if (field.Key == "Description")
                {
                    textBox.AcceptsReturn = true;
                    textBox.TextWrapping = TextWrapping.Wrap;
                }

                AddControlToStackPanel(stackPanel, textBox);
                MainFormStackPanel.Children.Add(stackPanel);
            }
        }

        private void GenerateEventFieldsEditable()
        {
            var fields = new Dictionary<string, string>
            {
                { "EventName", "Название события *" },
                { "Description", "Описание *" },
                { "DateTimeStart", "Дата и время начала *" },
                { "DateTimeFinish", "Дата и время окончания *" },
                { "Status", "Статус *" },
                { "EstimatedBudget", "Предполагаемый бюджет *" },
                { "MaxNumOfGuests", "Максимальное количество гостей *" },
                { "CategoryId", "Категория *" },
                { "VenueId", "Место проведения *" },
                { "UserId", "Ответственный менеджер *" }
            };

            foreach (var field in fields)
            {
                var stackPanel = CreateFieldStackPanel(field.Value);

                if (field.Key == "UserId")
                {
                    var managerComboBox = new ComboBox
                    {
                        Style = (Style)FindResource("CustomComboBox"),
                        Name = "ComboBox_UserId",
                        DisplayMemberPath = "DisplayName",
                        SelectedValuePath = "UserId"
                    };
                    LoadManagers(managerComboBox);
                    AddControlToStackPanel(stackPanel, managerComboBox);
                }
                else if (field.Key == "CategoryId")
                {
                    _categoryComboBox = new ComboBox
                    {
                        Style = (Style)FindResource("CustomComboBox"),
                        Name = "ComboBox_CategoryId",
                        DisplayMemberPath = "CategoryName",
                        SelectedValuePath = "CategoryId"
                    };
                    LoadCategories(_categoryComboBox);
                    AddControlToStackPanel(stackPanel, _categoryComboBox);
                }
                else if (field.Key == "VenueId")
                {
                    _venueComboBox = new ComboBox
                    {
                        Style = (Style)FindResource("CustomComboBox"),
                        Name = "ComboBox_VenueId",
                        DisplayMemberPath = "VenueName",
                        SelectedValuePath = "VenueId"
                    };
                    LoadVenues(_venueComboBox);
                    AddControlToStackPanel(stackPanel, _venueComboBox);
                }
                else if (field.Key == "DateTimeStart" || field.Key == "DateTimeFinish")
                {
                    var datePicker = new DatePicker
                    {
                        Style = (Style)FindResource("DatePickerBox"),
                        Name = $"DatePicker_{field.Key}",
                        SelectedDateFormat = DatePickerFormat.Short,
                        DisplayDateStart = DateTime.Now
                    };

                    var timeStackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                    var hourTextBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Name = $"Hour_{field.Key}",
                        Width = 40,
                        Text = "12",
                        Tag = "Time"
                    };
                    hourTextBox.PreviewTextInput += TimeTextBox_PreviewTextInput;

                    var minuteTextBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Name = $"Minute_{field.Key}",
                        Width = 40,
                        Text = "00",
                        Tag = "Time"
                    };
                    minuteTextBox.PreviewTextInput += TimeTextBox_PreviewTextInput;

                    timeStackPanel.Children.Add(new TextBlock { Text = "Время: ", VerticalAlignment = VerticalAlignment.Center });
                    timeStackPanel.Children.Add(hourTextBox);
                    timeStackPanel.Children.Add(new TextBlock { Text = ":", VerticalAlignment = VerticalAlignment.Center });
                    timeStackPanel.Children.Add(minuteTextBox);

                    var mainStackPanel = new StackPanel();
                    mainStackPanel.Children.Add(datePicker);
                    mainStackPanel.Children.Add(timeStackPanel);

                    AddControlToStackPanel(stackPanel, mainStackPanel);
                }
                else if (field.Key == "Status")
                {
                    _statusComboBox = new ComboBox
                    {
                        Style = (Style)FindResource("CustomComboBox"),
                        Name = "ComboBox_Status",
                        ItemsSource = new List<string> { "Согласован", "В обработке", "Ждет утверждения" }
                    };
                    AddControlToStackPanel(stackPanel, _statusComboBox);
                }
                else if (field.Key == "EstimatedBudget")
                {
                    var textBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Padding = new Thickness(10),
                        Name = $"TextBox_{field.Key}",
                        Tag = "Decimal"
                    };
                    textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
                    AddControlToStackPanel(stackPanel, textBox);
                }
                else
                {
                    var textBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Padding = new Thickness(10),
                        Name = $"TextBox_{field.Key}"
                    };

                    if (field.Key == "Description")
                    {
                        textBox.AcceptsReturn = true;
                        textBox.TextWrapping = TextWrapping.Wrap;
                    }
                    else if (field.Key == "MaxNumOfGuests")
                    {
                        textBox.Tag = "Integer";
                        textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
                    }

                    AddControlToStackPanel(stackPanel, textBox);
                }

                MainFormStackPanel.Children.Add(stackPanel);
            }
        }

        private void GenerateEditableActualBudgetField()
        {
            var stackPanel = CreateFieldStackPanel("Реальный бюджет");

            _actualBudgetTextBox = new TextBox
            {
                Style = (Style)FindResource("InputBox"),
                Padding = new Thickness(10),
                Name = "TextBox_ActualBudget",
                IsReadOnly = false,
                Tag = "Decimal"
            };

            _actualBudgetTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            _actualBudgetTextBox.LostFocus += ActualBudgetTextBox_LostFocus;

            AddControlToStackPanel(stackPanel, _actualBudgetTextBox);
            MainFormStackPanel.Children.Add(stackPanel);
        }

        private void GenerateCategoryFields()
        {
            var fields = new Dictionary<string, string>
            {
                { "CategoryName", "Название категории *" }
            };

            foreach (var field in fields)
            {
                var stackPanel = CreateFieldStackPanel(field.Value);

                var textBox = new TextBox
                {
                    Style = (Style)FindResource("InputBox"),
                    Padding = new Thickness(10),
                    Name = $"TextBox_{field.Key}",
                    IsReadOnly = false,
                    Background = Brushes.White
                };

                AddControlToStackPanel(stackPanel, textBox);
                MainFormStackPanel.Children.Add(stackPanel);
            }
        }

        private void GenerateVenueFields()
        {
            var fields = new Dictionary<string, string>
            {
                { "VenueName", "Название места *" },
                { "Address", "Адрес *" },
                { "Capacity", "Вместимость *" },
                { "Description", "Описание" }
            };

            foreach (var field in fields)
            {
                var stackPanel = CreateFieldStackPanel(field.Value);

                var textBox = new TextBox
                {
                    Style = (Style)FindResource("InputBox"),
                    Padding = new Thickness(10),
                    TextWrapping = TextWrapping.Wrap,
                    Name = $"TextBox_{field.Key}"
                };

                if (field.Key == "Description")
                {
                    textBox.AcceptsReturn = true;
                    textBox.TextWrapping = TextWrapping.Wrap;
                }
                else if (field.Key == "Capacity")
                {
                    textBox.Tag = "Integer";
                    textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
                }

                AddControlToStackPanel(stackPanel, textBox);
                MainFormStackPanel.Children.Add(stackPanel);
            }
        }

        private void GenerateClientFields()
        {
            LoadAvailableEvents();

            var fields = new Dictionary<string, string>
            {
                { "LastName", "Фамилия *" },
                { "Name", "Имя *" },
                { "MiddleName", "Отчество" },
                { "Email", "Email *" },
                { "Phone", "Телефон *" },
                { "EventId", "Событие *" }
            };

            foreach (var field in fields)
            {
                var stackPanel = CreateFieldStackPanel(field.Value);

                if (field.Key == "EventId")
                {
                    _eventComboBox = new ComboBox
                    {
                        Style = (Style)FindResource("CustomComboBox"),
                        Name = "ComboBox_Event",
                        DisplayMemberPath = "EventName",
                        SelectedValuePath = "EventId"
                    };
                    _eventComboBox.ItemsSource = _availableEvents;
                    AddControlToStackPanel(stackPanel, _eventComboBox);
                }
                else if (field.Key == "Phone")
                {
                    var textBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Padding = new Thickness(10),
                        Name = $"TextBox_{field.Key}"
                    };
                    textBox.PreviewTextInput += PhoneTextBox_PreviewTextInput;
                    textBox.TextChanged += PhoneTextBox_TextChanged;
                    AddControlToStackPanel(stackPanel, textBox);
                }
                else
                {
                    var textBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Padding = new Thickness(10),
                        Name = $"TextBox_{field.Key}"
                    };

                    if (field.Key == "LastName" || field.Key == "Name" || field.Key == "MiddleName")
                    {
                        textBox.Tag = "Text";
                        textBox.PreviewTextInput += TextTextBox_PreviewTextInput;
                    }
                    else if (field.Key == "Email")
                    {
                        textBox.LostFocus += EmailTextBox_LostFocus;
                    }

                    AddControlToStackPanel(stackPanel, textBox);
                }

                MainFormStackPanel.Children.Add(stackPanel);
            }
        }

        private void GenerateUserFields()
        {
            LoadAvailableRoles();

            var fields = new Dictionary<string, string>
            {
                { "LastName", "Фамилия *" },
                { "Name", "Имя *" },
                { "MiddleName", "Отчество" },
                { "Phone", "Телефон *" },
                { "Specialty", "Специализация *" },
                { "Login", "Логин *" },
                { "Password", "Пароль *" },
                { "RoleId", "Роль *" }
            };

            foreach (var field in fields)
            {
                var stackPanel = CreateFieldStackPanel(field.Value);

                if (field.Key == "RoleId")
                {
                    _roleComboBox = new ComboBox
                    {
                        Style = (Style)FindResource("CustomComboBox"),
                        Name = "ComboBox_Role",
                        DisplayMemberPath = "RoleName",
                        SelectedValuePath = "RoleId"
                    };
                    _roleComboBox.ItemsSource = _availableRoles;
                    AddControlToStackPanel(stackPanel, _roleComboBox);
                }
                else if (field.Key == "Phone")
                {
                    var textBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Padding = new Thickness(10),
                        Name = $"TextBox_{field.Key}"
                    };
                    textBox.PreviewTextInput += PhoneTextBox_PreviewTextInput;
                    textBox.TextChanged += PhoneTextBox_TextChanged;
                    AddControlToStackPanel(stackPanel, textBox);
                }
                else if (field.Key == "Password")
                {
                    var passwordBox = new PasswordBox
                    {
                        Style = (Style)FindResource("InputPassBox"),
                        Name = $"PasswordBox_{field.Key}"
                    };
                    passwordBox.LostFocus += PasswordBox_LostFocus;
                    AddControlToStackPanel(stackPanel, passwordBox);
                }
                else
                {
                    var textBox = new TextBox
                    {
                        Style = (Style)FindResource("InputBox"),
                        Padding = new Thickness(10),
                        Name = $"TextBox_{field.Key}"
                    };

                    if (field.Key == "LastName" || field.Key == "Name" || field.Key == "MiddleName")
                    {
                        textBox.Tag = "Text";
                        textBox.PreviewTextInput += TextTextBox_PreviewTextInput;
                    }

                    AddControlToStackPanel(stackPanel, textBox);
                }

                MainFormStackPanel.Children.Add(stackPanel);
            }
        }

        private void GenerateRoleFields()
        {
            var fields = new Dictionary<string, string>
            {
                { "RoleName", "Название роли *" }
            };

            foreach (var field in fields)
            {
                var stackPanel = CreateFieldStackPanel(field.Value);

                var textBox = new TextBox
                {
                    Style = (Style)FindResource("InputBox"),
                    Padding = new Thickness(10),
                    Name = $"TextBox_{field.Key}",
                    IsReadOnly = false,
                    Background = Brushes.White
                };

                AddControlToStackPanel(stackPanel, textBox);
                MainFormStackPanel.Children.Add(stackPanel);
            }
        }

        // ========== LOAD DATA METHODS ==========

        private void LoadManagers(ComboBox comboBox)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var managers = context.Users
                        .Include(u => u.Role)
                        .Where(u => u.RoleId == 2)
                        .Select(u => new ManagerViewModel
                        {
                            UserId = u.UserId,
                            DisplayName = $"{u.LastName} {u.Name} {u.MiddleName}, {u.Specialty}",
                            Specialty = u.Specialty
                        })
                        .ToList();

                    comboBox.ItemsSource = managers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка менеджеров: {ex.Message}", "Ошибка");
            }
        }

        private void LoadCategories(ComboBox comboBox)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var categories = context.EventCategories
                        .Select(c => new
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName
                        })
                        .ToList();

                    comboBox.ItemsSource = categories;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка категорий: {ex.Message}", "Ошибка");
            }
        }

        private void LoadVenues(ComboBox comboBox)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var venues = context.Venues
                        .Select(v => new
                        {
                            VenueId = v.VenueId,
                            VenueName = v.VenueName
                        })
                        .ToList();

                    comboBox.ItemsSource = venues;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка мест проведения: {ex.Message}", "Ошибка");
            }
        }

        private void LoadAvailableEvents()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    _availableEvents = context.Event
                        .Select(e => new EventViewModel
                        {
                            EventId = e.EventId,
                            EventName = e.EventName
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки событий: {ex.Message}", "Ошибка");
            }
        }

        private void LoadAvailableRoles()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    _availableRoles = context.Roles
                        .Select(r => new RoleViewModel
                        {
                            RoleId = r.RoleId,
                            RoleName = r.RoleName
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка");
            }
        }

        // ========== LOAD DATA TO FORM ==========

        private void LoadEventData()
        {
            if (_selectedEvent == null) return;

            try
            {
                SetTextBoxValue("TextBox_EventName", _selectedEvent.EventName);
                SetTextBoxValue("TextBox_Description", _selectedEvent.Description);

                if (_statusComboBox != null)
                    _statusComboBox.SelectedItem = _selectedEvent.Status;

                SetTextBoxValue("TextBox_EstimatedBudget", _selectedEvent.EstimatedBudget?.ToString("N0") ?? "0");
                SetTextBoxValue("TextBox_MaxNumOfGuests", _selectedEvent.MaxNumOfGuests?.ToString() ?? "0");

                // Устанавливаем значения в ComboBox
                SetSelectedCategory(_selectedEvent.CategoryId);
                SetSelectedVenue(_selectedEvent.VenueId);

                // Устанавливаем даты
                SetDateTimeInFields("DateTimeStart", _selectedEvent.DateTimeStart);
                SetDateTimeInFields("DateTimeFinish", _selectedEvent.DateTimeFinish);

                if (_actualBudgetTextBox != null)
                    _actualBudgetTextBox.Text = _selectedEvent.ActualBudget?.ToString("N0") ?? "0";

                // Устанавливаем менеджера
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetSelectedManager(_selectedEvent.UserId);
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void SetSelectedManager(int userId)
        {
            try
            {
                var managerComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_UserId");
                if (managerComboBox != null && userId > 0)
                {
                    managerComboBox.SelectedValue = userId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке ответственного менеджера: {ex.Message}", "Ошибка");
            }
        }

        private void SetSelectedCategory(int categoryId)
        {
            try
            {
                var categoryComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_CategoryId");
                if (categoryComboBox != null && categoryId > 0)
                {
                    categoryComboBox.SelectedValue = categoryId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке категории: {ex.Message}", "Ошибка");
            }
        }

        private void SetSelectedVenue(int venueId)
        {
            try
            {
                var venueComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_VenueId");
                if (venueComboBox != null && venueId > 0)
                {
                    venueComboBox.SelectedValue = venueId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке места проведения: {ex.Message}", "Ошибка");
            }
        }

        private void SetDateTimeInFields(string fieldPrefix, DateTime dateTime)
        {
            var datePicker = FindVisualChild<DatePicker>(MainFormStackPanel, $"DatePicker_{fieldPrefix}");
            var hourTextBox = FindVisualChild<TextBox>(MainFormStackPanel, $"Hour_{fieldPrefix}");
            var minuteTextBox = FindVisualChild<TextBox>(MainFormStackPanel, $"Minute_{fieldPrefix}");

            if (datePicker != null)
                datePicker.SelectedDate = dateTime.Date;

            if (hourTextBox != null)
                hourTextBox.Text = dateTime.Hour.ToString("00");

            if (minuteTextBox != null)
                minuteTextBox.Text = dateTime.Minute.ToString("00");
        }

        private void LoadCategoryData()
        {
            if (_selectedCategory == null) return;

            try
            {
                SetTextBoxValue("TextBox_CategoryName", _selectedCategory.CategoryName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void LoadVenueData()
        {
            if (_selectedVenue == null) return;

            try
            {
                SetTextBoxValue("TextBox_VenueName", _selectedVenue.VenueName);
                SetTextBoxValue("TextBox_Address", _selectedVenue.Address);
                SetTextBoxValue("TextBox_Capacity", _selectedVenue.Capacity.ToString());
                SetTextBoxValue("TextBox_Description", _selectedVenue.Description ?? "");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void LoadClientData()
        {
            try
            {
                SetTextBoxValue("TextBox_LastName", _selectedClient.LastName);
                SetTextBoxValue("TextBox_Name", _selectedClient.Name);
                SetTextBoxValue("TextBox_MiddleName", _selectedClient.MiddleName ?? "");
                SetTextBoxValue("TextBox_Email", _selectedClient.Email ?? "");
                SetTextBoxValue("TextBox_Phone", _selectedClient.Phone ?? "");

                if (_eventComboBox != null)
                {
                    _eventComboBox.SelectedValue = _selectedClient.EventId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void LoadUserData()
        {
            try
            {
                SetTextBoxValue("TextBox_LastName", _selectedUser.LastName);
                SetTextBoxValue("TextBox_Name", _selectedUser.Name);
                SetTextBoxValue("TextBox_MiddleName", _selectedUser.MiddleName ?? "");
                SetTextBoxValue("TextBox_Phone", _selectedUser.Phone);
                SetTextBoxValue("TextBox_Specialty", _selectedUser.Specialty ?? "");
                SetTextBoxValue("TextBox_Login", _selectedUser.Login);
                SetPasswordBoxValue("PasswordBox_Password", _selectedUser.Password);

                if (_roleComboBox != null)
                {
                    _roleComboBox.SelectedValue = _selectedUser.RoleId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void LoadRoleData()
        {
            try
            {
                SetTextBoxValue("TextBox_RoleName", _selectedRole.RoleName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        // ========== VALIDATION AND SAVE METHODS ==========

        private bool ValidateForm()
        {
            bool isValid = true;
            var errors = new List<string>();

            switch (_currentPanelType)
            {
                case "Event":
                    isValid = ValidateEventForm(errors);
                    break;
                case "Venue":
                    isValid = ValidateVenueForm(errors);
                    break;
                case "Client":
                    isValid = ValidateClientForm(errors);
                    break;
                case "User":
                    isValid = ValidateUserForm(errors);
                    break;
                case "Category":
                    isValid = ValidateCategoryForm(errors);
                    break;
                case "Role":
                    isValid = ValidateRoleForm(errors);
                    break;
            }

            if (!isValid)
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return isValid;
        }

        private bool ValidateEventForm(List<string> errors)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_EventName")))
            {
                errors.Add("Название события обязательно для заполнения");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_Description")))
            {
                errors.Add("Описание обязательно для заполнения");
                isValid = false;
            }

            var categoryComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_CategoryId");
            if (categoryComboBox == null || categoryComboBox.SelectedItem == null)
            {
                errors.Add("Категория обязательна для выбора");
                isValid = false;
            }

            var venueComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_VenueId");
            if (venueComboBox == null || venueComboBox.SelectedItem == null)
            {
                errors.Add("Место проведения обязательно для выбора");
                isValid = false;
            }

            if (_statusComboBox == null || _statusComboBox.SelectedItem == null)
            {
                errors.Add("Статус обязателен для выбора");
                isValid = false;
            }

            if (!decimal.TryParse(GetTextBoxValue("TextBox_EstimatedBudget"), out decimal budget) || budget <= 0)
            {
                errors.Add("Предполагаемый бюджет должен быть положительным числом");
                isValid = false;
            }

            if (!int.TryParse(GetTextBoxValue("TextBox_MaxNumOfGuests"), out int guests) || guests <= 0)
            {
                errors.Add("Максимальное количество гостей должно быть положительным числом");
                isValid = false;
            }

            var managerComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_UserId");
            if (managerComboBox == null || managerComboBox.SelectedItem == null)
            {
                errors.Add("Ответственный менеджер обязателен для выбора");
                isValid = false;
            }

            return isValid;
        }

        private bool ValidateVenueForm(List<string> errors)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_VenueName")))
            {
                errors.Add("Название места обязательно для заполнения");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_Address")))
            {
                errors.Add("Адрес обязателен для заполнения");
                isValid = false;
            }

            if (!int.TryParse(GetTextBoxValue("TextBox_Capacity"), out int capacity) || capacity <= 0)
            {
                errors.Add("Вместимость должна быть положительным числом");
                isValid = false;
            }

            return isValid;
        }

        private bool ValidateClientForm(List<string> errors)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_LastName")))
            {
                errors.Add("Фамилия обязательна для заполнения");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_Name")))
            {
                errors.Add("Имя обязательно для заполнения");
                isValid = false;
            }

            string email = GetTextBoxValue("TextBox_Email");
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                errors.Add("Введите корректный email");
                isValid = false;
            }

            string clientPhone = GetTextBoxValue("TextBox_Phone");
            string cleanClientPhone = "";

            if (!string.IsNullOrEmpty(clientPhone))
            {
                cleanClientPhone = new string(clientPhone.Where(char.IsDigit).ToArray());
            }

            if (string.IsNullOrWhiteSpace(clientPhone) || cleanClientPhone.Length != 11)
            {
                errors.Add("Введите корректный номер телефона в формате +700-000-00-00");
                isValid = false;
            }
            else if (!cleanClientPhone.StartsWith("7"))
            {
                errors.Add("Номер телефона должен начинаться с +7");
                isValid = false;
            }

            if (_eventComboBox == null || _eventComboBox.SelectedItem == null)
            {
                errors.Add("Событие обязательно для выбора");
                isValid = false;
            }

            // Проверка уникальности телефона
            if (!string.IsNullOrWhiteSpace(clientPhone))
            {
                using (var context = new ApplicationContext())
                {
                    var existingClient = context.Clients
                        .FirstOrDefault(c => c.Phone == clientPhone &&
                            (_selectedClient == null || c.ClientId != _selectedClient.ClientId));

                    if (existingClient != null)
                    {
                        errors.Add("Клиент с таким номером телефона уже существует");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        private bool ValidateUserForm(List<string> errors)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_LastName")))
            {
                errors.Add("Фамилия обязательна для заполнения");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(GetTextBoxValue("TextBox_Name")))
            {
                errors.Add("Имя обязательно для заполнения");
                isValid = false;
            }

            string userPhone = GetTextBoxValue("TextBox_Phone");
            string cleanUserPhone = "";

            if (!string.IsNullOrEmpty(userPhone))
            {
                cleanUserPhone = new string(userPhone.Where(char.IsDigit).ToArray());
            }

            if (string.IsNullOrWhiteSpace(userPhone) || cleanUserPhone.Length != 11)
            {
                errors.Add("Введите корректный номер телефона в формате +700-000-00-00");
                isValid = false;
            }
            else if (!cleanUserPhone.StartsWith("7"))
            {
                errors.Add("Номер телефона должен начинаться с +7");
                isValid = false;
            }

            string login = GetTextBoxValue("TextBox_Login");
            if (string.IsNullOrWhiteSpace(login))
            {
                errors.Add("Логин обязателен для заполнения");
                isValid = false;
            }

            string password = GetPasswordBoxValue("PasswordBox_Password");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                errors.Add("Пароль должен содержать не менее 8 символов");
                isValid = false;
            }

            if (_roleComboBox == null || _roleComboBox.SelectedItem == null)
            {
                errors.Add("Роль обязательна для выбора");
                isValid = false;
            }

            // Проверка уникальности логина и телефона
            if (!string.IsNullOrWhiteSpace(login))
            {
                using (var context = new ApplicationContext())
                {
                    var existingUser = context.Users
                        .FirstOrDefault(u => (u.Login == login || u.Phone == userPhone) &&
                            (_selectedUser == null || u.UserId != _selectedUser.UserId));

                    if (existingUser != null)
                    {
                        if (existingUser.Login == login)
                            errors.Add("Пользователь с таким логином уже существует");
                        if (existingUser.Phone == userPhone)
                            errors.Add("Пользователь с таким номером телефона уже существует");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        private bool ValidateCategoryForm(List<string> errors)
        {
            bool isValid = true;

            string categoryName = GetTextBoxValue("TextBox_CategoryName");
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                errors.Add("Название категории обязательно для заполнения");
                isValid = false;
            }
            else
            {
                // Проверка уникальности категории
                using (var context = new ApplicationContext())
                {
                    var existingCategory = context.EventCategories
                        .FirstOrDefault(c => c.CategoryName.ToLower() == categoryName.ToLower() &&
                            (_selectedCategory == null || c.CategoryId != _selectedCategory.CategoryId));

                    if (existingCategory != null)
                    {
                        errors.Add("Категория с таким названием уже существует");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        private bool ValidateRoleForm(List<string> errors)
        {
            bool isValid = true;

            string roleName = GetTextBoxValue("TextBox_RoleName");
            if (string.IsNullOrWhiteSpace(roleName))
            {
                errors.Add("Название роли обязательно для заполнения");
                isValid = false;
            }
            else
            {
                // Проверка уникальности роли
                using (var context = new ApplicationContext())
                {
                    var existingRole = context.Roles
                        .FirstOrDefault(r => r.RoleName.ToLower() == roleName.ToLower() &&
                            (_selectedRole == null || r.RoleId != _selectedRole.RoleId));

                    if (existingRole != null)
                    {
                        errors.Add("Роль с таким названием уже существует");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        // ========== HELPER METHODS ==========

        private string GetTextBoxValue(string controlName)
        {
            var textBox = FindVisualChild<TextBox>(MainFormStackPanel, controlName);
            return textBox?.Text ?? "";
        }

        private string GetPasswordBoxValue(string controlName)
        {
            var passwordBox = FindVisualChild<PasswordBox>(MainFormStackPanel, controlName);
            return passwordBox?.Password ?? "";
        }

        private void SetTextBoxValue(string controlName, string value)
        {
            var textBox = FindVisualChild<TextBox>(MainFormStackPanel, controlName);
            if (textBox != null)
            {
                textBox.Text = value;
            }
        }

        private void SetPasswordBoxValue(string controlName, string value)
        {
            var passwordBox = FindVisualChild<PasswordBox>(MainFormStackPanel, controlName);
            if (passwordBox != null)
            {
                passwordBox.Password = value;
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        return typedChild;
                    }
                }

                var result = FindVisualChild<T>(child, childName);
                if (result != null) return result;

                if (child is Panel panel)
                {
                    foreach (var panelChild in panel.Children)
                    {
                        if (panelChild is T panelTypedChild)
                        {
                            var panelFrameworkElement = panelChild as FrameworkElement;
                            if (panelFrameworkElement != null && panelFrameworkElement.Name == childName)
                            {
                                return panelTypedChild;
                            }
                        }
                    }
                }

                if (child is ContentControl contentControl && contentControl.Content is DependencyObject content)
                {
                    var resultFromContent = FindVisualChild<T>(content, childName);
                    if (resultFromContent != null) return resultFromContent;
                }
            }
            return null;
        }

        private StackPanel CreateFieldStackPanel(string fieldName)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

            var textBlock = new TextBlock
            {
                Text = fieldName,
                Style = (Style)FindResource("H3Text"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = (Brush)new BrushConverter().ConvertFromString("#6B7D96"),
                FontWeight = FontWeights.Medium
            };

            stackPanel.Children.Add(textBlock);
            return stackPanel;
        }

        private void AddControlToStackPanel(StackPanel stackPanel, Control control)
        {
            var border = new Border
            {
                Style = (Style)FindResource("TextBlockBorder")
            };
            border.Child = control;
            stackPanel.Children.Add(border);
        }

        private void AddControlToStackPanel(StackPanel stackPanel, UIElement element)
        {
            var border = new Border
            {
                Style = (Style)FindResource("TextBlockBorder")
            };
            border.Child = element;
            stackPanel.Children.Add(border);
        }

        private void UpdateHeaderText(string text)
        {
            var textBlock = FindName("HeaderTextBlock") as TextBlock;
            if (textBlock != null)
            {
                textBlock.Text = text;
            }
        }

        private void UpdateButtonText(string text)
        {
            if (btnAddEdit != null)
            {
                btnAddEdit.Content = text;
            }
        }

        private void ActualBudgetTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && decimal.TryParse(textBox.Text, out decimal result))
            {
                textBox.Text = result.ToString("N0");
            }
        }

        public void ClearForm()
        {
            try
            {
                _selectedEvent = null;
                _selectedCategory = null;
                _selectedVenue = null;
                _selectedClient = null;
                _selectedUser = null;
                _selectedRole = null;
                GenerateFormFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка очистки формы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm())
                {
                    return;
                }

                string panelType = data.addEditTable;

                switch (panelType)
                {
                    case "Event":
                        if (_selectedEvent != null)
                        {
                            // РЕДАКТИРОВАНИЕ
                            if (data.roleAuth == 0) // Админ
                            {
                                UpdateEventInDatabase();
                            }
                            else // Пользователь
                            {
                                if (_actualBudgetTextBox != null && decimal.TryParse(_actualBudgetTextBox.Text, out decimal newActualBudget))
                                {
                                    UpdateEventActualBudgetOnly(newActualBudget);
                                }
                            }
                            MessageBox.Show("Данные успешно обновлены!", "Успех");
                        }
                        else
                        {
                            // ДОБАВЛЕНИЕ
                            if (ValidateEventFormForAdd())
                            {
                                AddEventToDatabase();
                                MessageBox.Show("Событие успешно добавлено!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        break;

                    case "Category":
                        var categoryNameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_CategoryName");
                        if (categoryNameTextBox != null && !string.IsNullOrWhiteSpace(categoryNameTextBox.Text))
                        {
                            if (_selectedCategory != null)
                            {
                                UpdateCategoryInDatabase(categoryNameTextBox.Text);
                                MessageBox.Show("Категория успешно обновлена!", "Успех");
                            }
                            else
                            {
                                AddCategoryToDatabase(categoryNameTextBox.Text);
                                MessageBox.Show("Категория успешно добавлена!", "Успех");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Введите название категории", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;

                    case "Venue":
                        var venueNameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_VenueName");
                        var addressTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Address");
                        var capacityTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Capacity");
                        var descriptionTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Description");

                        if (venueNameTextBox != null && !string.IsNullOrWhiteSpace(venueNameTextBox.Text) &&
                            addressTextBox != null && !string.IsNullOrWhiteSpace(addressTextBox.Text) &&
                            capacityTextBox != null && int.TryParse(capacityTextBox.Text, out int capacity))
                        {
                            if (_selectedVenue != null)
                            {
                                UpdateVenueInDatabase(venueNameTextBox.Text, addressTextBox.Text, capacity, descriptionTextBox?.Text);
                                MessageBox.Show("Место проведения успешно обновлено!", "Успех");
                            }
                            else
                            {
                                AddVenueToDatabase(venueNameTextBox.Text, addressTextBox.Text, capacity, descriptionTextBox?.Text);
                                MessageBox.Show("Место проведения успешно добавлено!", "Успех");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Заполните обязательные поля: название, адрес и вместимость", "Ошибка");
                        }
                        break;

                    case "Client":
                        var lastNameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_LastName");
                        var nameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Name");

                        int eventId = 0;
                        if (_eventComboBox != null && _eventComboBox.SelectedValue != null)
                        {
                            eventId = (int)_eventComboBox.SelectedValue;
                        }

                        if (lastNameTextBox != null && !string.IsNullOrWhiteSpace(lastNameTextBox.Text) &&
                            nameTextBox != null && !string.IsNullOrWhiteSpace(nameTextBox.Text) &&
                            eventId > 0)
                        {
                            var middleNameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_MiddleName");
                            var emailTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Email");
                            var phoneTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Phone");

                            if (_selectedClient != null)
                            {
                                UpdateClientInDatabase(
                                    lastNameTextBox.Text,
                                    nameTextBox.Text,
                                    middleNameTextBox?.Text,
                                    emailTextBox?.Text,
                                    phoneTextBox?.Text,
                                    eventId
                                );
                                MessageBox.Show("Клиент успешно обновлен!", "Успех");
                            }
                            else
                            {
                                AddClientToDatabase(
                                    lastNameTextBox.Text,
                                    nameTextBox.Text,
                                    middleNameTextBox?.Text,
                                    emailTextBox?.Text,
                                    phoneTextBox?.Text,
                                    eventId
                                );
                                MessageBox.Show("Клиент успешно добавлен!", "Успех");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Заполните обязательные поля: фамилия, имя и выберите событие", "Ошибка");
                        }
                        break;

                    case "User":
                        lastNameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_LastName");
                        nameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Name");
                        var loginTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Login");
                        var passwordBox = FindVisualChild<PasswordBox>(MainFormStackPanel, "PasswordBox_Password");

                        int roleId = 0;
                        if (_roleComboBox != null && _roleComboBox.SelectedValue != null)
                        {
                            roleId = (int)_roleComboBox.SelectedValue;
                        }

                        if (lastNameTextBox != null && !string.IsNullOrWhiteSpace(lastNameTextBox.Text) &&
                            nameTextBox != null && !string.IsNullOrWhiteSpace(nameTextBox.Text) &&
                            loginTextBox != null && !string.IsNullOrWhiteSpace(loginTextBox.Text) &&
                            passwordBox != null && !string.IsNullOrWhiteSpace(passwordBox.Password) &&
                            roleId > 0)
                        {
                            var middleNameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_MiddleName");
                            var phoneTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Phone");
                            var specialtyTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_Specialty");

                            if (_selectedUser != null)
                            {
                                UpdateUserInDatabase(
                                    lastNameTextBox.Text,
                                    nameTextBox.Text,
                                    middleNameTextBox?.Text,
                                    phoneTextBox?.Text,
                                    specialtyTextBox?.Text,
                                    loginTextBox.Text,
                                    passwordBox.Password,
                                    roleId
                                );
                                MessageBox.Show("Пользователь успешно обновлен!", "Успех");
                            }
                            else
                            {
                                AddUserToDatabase(
                                    lastNameTextBox.Text,
                                    nameTextBox.Text,
                                    middleNameTextBox?.Text,
                                    phoneTextBox?.Text,
                                    specialtyTextBox?.Text,
                                    loginTextBox.Text,
                                    passwordBox.Password,
                                    roleId
                                );
                                MessageBox.Show("Пользователь успешно добавлен!", "Успех");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Заполните обязательные поля: фамилия, имя, логин, пароль и выберите роль", "Ошибка");
                        }
                        break;

                    case "Role":
                        var roleNameTextBox = FindVisualChild<TextBox>(MainFormStackPanel, "TextBox_RoleName");

                        if (roleNameTextBox != null && !string.IsNullOrWhiteSpace(roleNameTextBox.Text))
                        {
                            if (_selectedRole != null)
                            {
                                UpdateRoleInDatabase(roleNameTextBox.Text);
                                MessageBox.Show("Роль успешно обновлена!", "Успех");
                            }
                            else
                            {
                                AddRoleToDatabase(roleNameTextBox.Text);
                                MessageBox.Show("Роль успешно добавлена!", "Успех");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Введите название роли", "Ошибка");
                        }
                        break;

                    default:
                        MessageBox.Show("Неизвестный тип операции", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                ClosePanelCommand?.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== DATABASE METHODS ==========

        private void AddEventToDatabase()
        {
            try
            {
                // Получаем выбранные значения из ComboBox
                var managerComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_UserId");
                var categoryComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_CategoryId");
                var venueComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_VenueId");

                int userId = 0;
                int categoryId = 0;
                int venueId = 0;

                if (managerComboBox?.SelectedValue != null)
                    userId = (int)managerComboBox.SelectedValue;

                if (categoryComboBox?.SelectedValue != null)
                    categoryId = (int)categoryComboBox.SelectedValue;

                if (venueComboBox?.SelectedValue != null)
                    venueId = (int)venueComboBox.SelectedValue;

                var newEvent = new Event
                {
                    EventName = GetTextBoxValue("TextBox_EventName"),
                    Description = GetTextBoxValue("TextBox_Description"),
                    DateTimeStart = GetDateTimeFromFields("DateTimeStart"),
                    DateTimeFinish = GetDateTimeFromFields("DateTimeFinish"),
                    Status = _statusComboBox?.SelectedItem?.ToString(),
                    EstimatedBudget = decimal.Parse(GetTextBoxValue("TextBox_EstimatedBudget")),
                    ActualBudget = 0,
                    MaxNumOfGuests = int.Parse(GetTextBoxValue("TextBox_MaxNumOfGuests")),
                    CategoryId = categoryId,
                    VenueId = venueId,
                    UserId = userId
                };

                _context.Event.Add(newEvent);
                _context.SaveChanges();
                RefreshEventsData();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления события: {ex.Message}");
            }
        }

        private void UpdateEventInDatabase()
        {
            try
            {
                var eventToUpdate = _context.Event.Find(_selectedEvent.EventId);
                if (eventToUpdate != null)
                {
                    // Получаем выбранные значения из ComboBox
                    var managerComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_UserId");
                    var categoryComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_CategoryId");
                    var venueComboBox = FindVisualChild<ComboBox>(MainFormStackPanel, "ComboBox_VenueId");

                    eventToUpdate.EventName = GetTextBoxValue("TextBox_EventName");
                    eventToUpdate.Description = GetTextBoxValue("TextBox_Description");
                    eventToUpdate.DateTimeStart = GetDateTimeFromFields("DateTimeStart");
                    eventToUpdate.DateTimeFinish = GetDateTimeFromFields("DateTimeFinish");
                    eventToUpdate.Status = _statusComboBox?.SelectedItem?.ToString();

                    if (decimal.TryParse(GetTextBoxValue("TextBox_EstimatedBudget"), out decimal estimatedBudget))
                        eventToUpdate.EstimatedBudget = estimatedBudget;

                    if (int.TryParse(GetTextBoxValue("TextBox_MaxNumOfGuests"), out int maxGuests))
                        eventToUpdate.MaxNumOfGuests = maxGuests;

                    if (_actualBudgetTextBox != null && decimal.TryParse(_actualBudgetTextBox.Text, out decimal actualBudget))
                        eventToUpdate.ActualBudget = actualBudget;

                    if (managerComboBox?.SelectedValue != null)
                        eventToUpdate.UserId = (int)managerComboBox.SelectedValue;

                    if (categoryComboBox?.SelectedValue != null)
                        eventToUpdate.CategoryId = (int)categoryComboBox.SelectedValue;

                    if (venueComboBox?.SelectedValue != null)
                        eventToUpdate.VenueId = (int)venueComboBox.SelectedValue;

                    _context.SaveChanges();
                    RefreshEventsData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления события: {ex.Message}");
            }
        }

        private void UpdateEventActualBudgetOnly(decimal newActualBudget)
        {
            try
            {
                var eventToUpdate = _context.Event.Find(_selectedEvent.EventId);
                if (eventToUpdate != null)
                {
                    eventToUpdate.ActualBudget = newActualBudget;
                    _context.SaveChanges();
                    RefreshEventsData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления бюджета: {ex.Message}");
            }
        }

        private void UpdateCategoryInDatabase(string newCategoryName)
        {
            try
            {
                var categoryToUpdate = _context.EventCategories.Find(_selectedCategory.CategoryId);
                if (categoryToUpdate != null)
                {
                    categoryToUpdate.CategoryName = newCategoryName;
                    _context.SaveChanges();
                    RefreshCategoriesData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления категории: {ex.Message}");
            }
        }

        private void AddCategoryToDatabase(string categoryName)
        {
            try
            {
                var newCategory = new EventCategories
                {
                    CategoryName = categoryName
                };

                _context.EventCategories.Add(newCategory);
                _context.SaveChanges();
                RefreshCategoriesData();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления категории: {ex.Message}");
            }
        }

        private void UpdateVenueInDatabase(string venueName, string address, int capacity, string description)
        {
            try
            {
                var venueToUpdate = _context.Venues.Find(_selectedVenue.VenueId);
                if (venueToUpdate != null)
                {
                    venueToUpdate.VenueName = venueName;
                    venueToUpdate.Address = address;
                    venueToUpdate.Capacity = capacity;
                    venueToUpdate.Description = description;
                    _context.SaveChanges();
                    RefreshVenuesData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления места проведения: {ex.Message}");
            }
        }

        private void AddVenueToDatabase(string venueName, string address, int capacity, string description)
        {
            try
            {
                var newVenue = new Venues
                {
                    VenueName = venueName,
                    Address = address,
                    Capacity = capacity,
                    Description = description
                };

                _context.Venues.Add(newVenue);
                _context.SaveChanges();
                RefreshVenuesData();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления места проведения: {ex.Message}");
            }
        }

        private void UpdateClientInDatabase(string lastName, string name, string middleName, string email, string phone, int eventId)
        {
            try
            {
                var clientToUpdate = _context.Clients.Find(_selectedClient.ClientId);
                if (clientToUpdate != null)
                {
                    clientToUpdate.LastName = lastName;
                    clientToUpdate.Name = name;
                    clientToUpdate.MiddleName = middleName;
                    clientToUpdate.Email = email;
                    clientToUpdate.Phone = phone;
                    clientToUpdate.EventId = eventId;
                    _context.SaveChanges();
                    RefreshClientsData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления клиента: {ex.Message}");
            }
        }

        private void AddClientToDatabase(string lastName, string name, string middleName, string email, string phone, int eventId)
        {
            try
            {
                var newClient = new Clients
                {
                    LastName = lastName,
                    Name = name,
                    MiddleName = middleName,
                    Email = email,
                    Phone = phone,
                    EventId = eventId
                };

                _context.Clients.Add(newClient);
                _context.SaveChanges();
                RefreshClientsData();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления клиента: {ex.Message}");
            }
        }

        private void UpdateUserInDatabase(string lastName, string name, string middleName, string phone, string specialty, string login, string password, int roleId)
        {
            try
            {
                var userToUpdate = _context.Users.Find(_selectedUser.UserId);
                if (userToUpdate != null)
                {
                    userToUpdate.LastName = lastName;
                    userToUpdate.Name = name;
                    userToUpdate.MiddleName = middleName;
                    userToUpdate.Phone = phone;
                    userToUpdate.Specialty = specialty;
                    userToUpdate.Login = login;
                    userToUpdate.Password = password;
                    userToUpdate.RoleId = roleId;
                    _context.SaveChanges();
                    RefreshUsersData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления пользователя: {ex.Message}");
            }
        }

        private void AddUserToDatabase(string lastName, string name, string middleName, string phone, string specialty, string login, string password, int roleId)
        {
            try
            {
                var newUser = new Users
                {
                    LastName = lastName,
                    Name = name,
                    MiddleName = middleName,
                    Phone = phone,
                    Specialty = specialty,
                    Login = login,
                    Password = password,
                    RoleId = roleId
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                RefreshUsersData();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления пользователя: {ex.Message}");
            }
        }

        private void UpdateRoleInDatabase(string roleName)
        {
            try
            {
                var roleToUpdate = _context.Roles.Find(_selectedRole.RoleId);
                if (roleToUpdate != null)
                {
                    roleToUpdate.RoleName = roleName;
                    _context.SaveChanges();
                    RefreshRolesData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления роли: {ex.Message}");
            }
        }

        private void AddRoleToDatabase(string roleName)
        {
            try
            {
                var newRole = new Role
                {
                    RoleName = roleName
                };

                _context.Roles.Add(newRole);
                _context.SaveChanges();
                RefreshRolesData();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления роли: {ex.Message}");
            }
        }

        // ========== REFRESH DATA METHODS ==========

        private void RefreshEventsData()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.EventsPanelControl?.RefreshData();
            }
        }

        private void RefreshCategoriesData()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.CategoriesPanelControl?.RefreshData();
            }
        }

        private void RefreshVenuesData()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.VenuesPanelControl?.RefreshData();
            }
        }

        private void RefreshClientsData()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ClientsPanelControl?.RefreshData();
            }
        }

        private void RefreshUsersData()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.UsersPanelControl?.RefreshData();
            }
        }

        private void RefreshRolesData()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.RolesPanelControl?.RefreshData();
            }
        }

        // ========== ADDITIONAL VALIDATION METHODS ==========

        private bool ValidateEventFormForAdd()
        {
            var errors = new List<string>();

            DateTime startDate = GetDateTimeFromFields("DateTimeStart");
            DateTime finishDate = GetDateTimeFromFields("DateTimeFinish");

            if (startDate >= finishDate)
            {
                errors.Add("Дата окончания должна быть позже даты начала");
            }

            if (startDate < DateTime.Now)
            {
                errors.Add("Дата начала не может быть в прошлом");
            }

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private DateTime GetDateTimeFromFields(string fieldPrefix)
        {
            try
            {
                var datePicker = FindVisualChild<DatePicker>(MainFormStackPanel, $"DatePicker_{fieldPrefix}");
                var hourTextBox = FindVisualChild<TextBox>(MainFormStackPanel, $"Hour_{fieldPrefix}");
                var minuteTextBox = FindVisualChild<TextBox>(MainFormStackPanel, $"Minute_{fieldPrefix}");

                if (datePicker?.SelectedDate != null && hourTextBox != null && minuteTextBox != null)
                {
                    int hours = int.Parse(hourTextBox.Text);
                    int minutes = int.Parse(minuteTextBox.Text);

                    return datePicker.SelectedDate.Value.AddHours(hours).AddMinutes(minutes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения даты и времени: {ex.Message}", "Ошибка");
            }

            return DateTime.Now;
        }

        // ========== VALIDATION METHODS ==========

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                foreach (char c in e.Text)
                {
                    if (!char.IsDigit(c))
                    {
                        e.Handled = true;
                        return;
                    }
                }

                string newText = textBox.Text + e.Text;
                if (textBox.Name.Contains("Hour"))
                {
                    if (int.TryParse(newText, out int hours) && (hours < 0 || hours > 23))
                    {
                        e.Handled = true;
                    }
                }
                else if (textBox.Name.Contains("Minute"))
                {
                    if (int.TryParse(newText, out int minutes) && (minutes < 0 || minutes > 59))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (textBox.Tag?.ToString() == "Decimal")
                {
                    foreach (char c in e.Text)
                    {
                        if (!char.IsDigit(c) && c != ',' && c != '.')
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }
                else if (textBox.Tag?.ToString() == "Integer")
                {
                    foreach (char c in e.Text)
                    {
                        if (!char.IsDigit(c))
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }
        }

        private void TextTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (char.IsDigit(c))
                {
                    e.Handled = true;
                    ShowValidationError(sender as TextBox, "Нельзя вводить цифры");
                    return;
                }
            }
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }

            string currentText = textBox.Text.Replace("-", "").Replace("+", "");
            if (currentText.Length + e.Text.Length > 11)
            {
                e.Handled = true;
                return;
            }
        }

        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.IsFocused)
            {
                string text = textBox.Text;
                int cursorPos = textBox.CaretIndex;

                string formatted = FormatPhoneNumber(text);
                if (formatted != text)
                {
                    int newCursorPos = CalculateNewCursorPosition(text, formatted, cursorPos);
                    textBox.Text = formatted;
                    textBox.CaretIndex = newCursorPos;
                }

                if (text.Length == 1 && char.IsDigit(text[0]))
                {
                    textBox.Text = "+7" + text;
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return phone;

            string digits = new string(phone.Where(c => char.IsDigit(c)).ToArray());

            if (digits.StartsWith("7") || digits.StartsWith("8"))
            {
                digits = "7" + digits.Substring(1);
            }

            if (!digits.StartsWith("7") && digits.Length > 0)
            {
                digits = "7" + digits;
            }

            StringBuilder formatted = new StringBuilder();

            if (digits.Length >= 1)
            {
                formatted.Append("+7");

                if (digits.Length > 1)
                {
                    formatted.Append(digits[1]);

                    if (digits.Length > 2)
                    {
                        formatted.Append(digits[2]);

                        if (digits.Length > 3)
                        {
                            formatted.Append(digits[3]);
                            formatted.Append("-");

                            if (digits.Length > 4)
                            {
                                formatted.Append(digits[4]);

                                if (digits.Length > 5)
                                {
                                    formatted.Append(digits[5]);

                                    if (digits.Length > 6)
                                    {
                                        formatted.Append(digits[6]);
                                        formatted.Append("-");

                                        if (digits.Length > 7)
                                        {
                                            formatted.Append(digits[7]);

                                            if (digits.Length > 8)
                                            {
                                                formatted.Append(digits[8]);
                                                formatted.Append("-");

                                                if (digits.Length > 9)
                                                {
                                                    formatted.Append(digits[9]);

                                                    if (digits.Length > 10)
                                                    {
                                                        formatted.Append(digits[10]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return formatted.ToString();
        }

        private int CalculateNewCursorPosition(string oldText, string newText, int oldCursorPos)
        {
            if (oldCursorPos >= oldText.Length)
                return newText.Length;

            int digitsBeforeCursor = 0;
            for (int i = 0; i < oldCursorPos && i < oldText.Length; i++)
            {
                if (char.IsDigit(oldText[i]))
                    digitsBeforeCursor++;
            }

            int newPos = 0;
            int digitsCount = 0;

            for (int i = 0; i < newText.Length && digitsCount < digitsBeforeCursor; i++)
            {
                newPos++;
                if (char.IsDigit(newText[i]))
                    digitsCount++;
            }

            return newPos;
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;

            string cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
            if (cleanPhone.Length != 11) return false;
            if (!cleanPhone.StartsWith("7")) return false;

            return true;
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (!IsValidEmail(textBox.Text))
                {
                    ShowValidationError(textBox, "Некорректный формат email");
                }
                else
                {
                    ClearValidationError(textBox);
                }
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && !string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                if (passwordBox.Password.Length < 8)
                {
                    ShowValidationError(passwordBox, "Пароль должен содержать не менее 8 символов");
                }
                else
                {
                    ClearValidationError(passwordBox);
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ShowValidationError(Control control, string message)
        {
            control.ToolTip = message;
        }

        private void ClearValidationError(Control control)
        {
            control.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#CBD5E1");
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
        }
    }
}