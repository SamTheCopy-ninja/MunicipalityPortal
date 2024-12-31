using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using MunicipalityPortal.Managers;
using MunicipalityPortal.Models;
using EventManager = MunicipalityPortal.Managers.EventManager;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;



namespace MunicipalityPortal.ViewModels
{
    // This view model is based on this tutorial:
    // Author:  Raj Kumar
    // Article: Simple MVVM Pattern in WPF
    // Source: https://www.c-sharpcorner.com/UploadFile/raj1979/simple-mvvm-pattern-in-wpf/

    public class EventsAndAnnouncementsViewModel : INotifyPropertyChanged
    {
        private readonly EventManager _eventManager;
        private List<Event> _filteredEvents;
        private List<Event> _recommendedEvents;
        private string _searchTerm;
        private string _selectedCategory;
        private DateTime? _startDate;
        private DateTime? _endDate;

        // ICommand interfaces for searching and posting events/announcements
        public ICommand SearchCommand { get; }
        public ICommand AddNewEventCommand { get; }

        // List for storing filtered events
        public List<Event> FilteredEvents
        {
            get => _filteredEvents;
            set
            {
                _filteredEvents = value;
                OnPropertyChanged(nameof(FilteredEvents));
            }
        }

        // List for storing events to recommend to the user, based on their searches
        public List<Event> RecommendedEvents
        {
            get => _recommendedEvents;
            set
            {
                _recommendedEvents = value;
                OnPropertyChanged(nameof(RecommendedEvents));
            }
        }

        // Search term for filtering the lists
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
            }
        }

        // Category selection on the events page
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        // Start date for filtering the search results
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        // End date for filtering the search results
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        // List of all available categories
        public List<string> Categories { get; private set; }

        public EventsAndAnnouncementsViewModel(EventManager eventManager)
        {
            _eventManager = eventManager;
            SearchCommand = new RelayCommand(PerformSearch);
            AddNewEventCommand = new RelayCommand(OpenAddNewEventWindow);
            LoadInitialEvents();
            UpdateCategories();
            SelectedCategory = "All Categories";
        }

        // Check if events exist in the events.json file, then add them to the list
        private void LoadInitialEvents()
        {
            FilteredEvents = _eventManager.GetAllEvents();
            UpdatePastEventStatus(FilteredEvents);
            RecommendedEvents = new List<Event>();
        }

        // If an event date, in the list, is before today's current date, mark it as a "Past Event"
        private void UpdatePastEventStatus(List<Event> events)
        {
            var today = DateTime.Today;
            foreach (var ev in events)
            {
                ev.IsPastEvent = ev.Date.Date < today;
            }
        }

        // Allow the user to post an event
        private void OpenAddNewEventWindow()
        {
            var eventSubmissionWindow = new EventSubmissionWindow(_eventManager);
            eventSubmissionWindow.ShowDialog();

            // Refresh the events list after the window is closed
            PerformSearch();

            UpdateCategories();
        }

        // Update the categories displayed for searches, based on the current events in the list
        private void UpdateCategories()
        {
            Categories = _eventManager.GetAllCategories();
            Categories.Insert(0, "All Categories");
            OnPropertyChanged(nameof(Categories));
        }

        // Allow users to search for events
        private void PerformSearch()
        {
            // Start with events filtered by category
            FilteredEvents = _eventManager.GetEventsByCategory(SelectedCategory);

            // Apply date range filter if both start and end dates are provided
            if (StartDate.HasValue && EndDate.HasValue)
            {
                FilteredEvents = _eventManager.GetEventsByDateRange(StartDate.Value, EndDate.Value)
                    .Intersect(FilteredEvents)
                    .ToList();
            }
            else
            {
                // Apply individual date filters if only one is provided
                if (StartDate.HasValue)
                {
                    FilteredEvents = FilteredEvents.FindAll(e => e.Date >= StartDate.Value);
                }

                if (EndDate.HasValue)
                {
                    FilteredEvents = FilteredEvents.FindAll(e => e.Date <= EndDate.Value);
                }
            }

            // Apply search term filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                FilteredEvents = FilteredEvents.FindAll(e =>
                    e.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Update IsPastEvent property
            UpdatePastEventStatus(FilteredEvents);

            UpdateRecommendations();
        }

        private void UpdateRecommendations()
        {
            // Recommend other events or annoucements based on user searches
            var filteredCategories = FilteredEvents.Select(e => e.Category).Distinct().ToList();
            RecommendedEvents = _eventManager.GetAllEvents()
                .Where(e => !filteredCategories.Contains(e.Category))
                .OrderBy(e => Guid.NewGuid())
                .Take(3)
                .ToList();

            // Update IsPastEvent property for recommended events
            UpdatePastEventStatus(RecommendedEvents);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // If a an event or annoucement has a thumbnail image, display that as well
        public ImageSource GetThumbnailImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }

    // Relay command interface for actions, based on this tutorial:
    // Author:  Rikam Palkar
    // Article: ICommand Interface In MVVM
    // Source: https://www.c-sharpcorner.com/article/icommand-interface-in-mvvm/

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

}
