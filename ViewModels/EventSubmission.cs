using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using MunicipalityPortal.Managers;
using MunicipalityPortal.Models;

namespace MunicipalityPortal.ViewModels
{
    // This view model is based on this tutorial:
    // Author:  Raj Kumar
    // Article: Simple MVVM Pattern in WPF
    // Source: https://www.c-sharpcorner.com/UploadFile/raj1979/simple-mvvm-pattern-in-wpf/

    public class EventSubmissionViewModel : INotifyPropertyChanged
    {
        private readonly EventManager _eventManager;
        private string _title;
        private string _description;
        private DateTime _date = DateTime.Now;
        private string _category;
        private string _location;
        private string _tagsInput;

        private byte[] _thumbnailImage;
        private string _thumbnailImagePath;

        // Allow users to add a thumbnail image
        public byte[] ThumbnailImage
        {
            get => _thumbnailImage;
            set { _thumbnailImage = value; OnPropertyChanged(); }
        }

        // Get the image path
        public string ThumbnailImagePath
        {
            get => _thumbnailImagePath;
            set { _thumbnailImagePath = value; OnPropertyChanged(); }
        }
      
        // Title of the event or announcement
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        // Description for the event or announcement
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // DateTime picker for the event or announcement being created
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        // Allow user to pick a category
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        // Store the location provided by the user
        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(); }
        }

        // Allow users to also include tags
        public string TagsInput
        {
            get => _tagsInput;
            set { _tagsInput = value; OnPropertyChanged(); }
        }

        // List to contain all categories
        public List<string> Categories { get; }

        // ICommand interfaces for submitting events and selecting a thumbnail
        public ICommand SubmitEventCommand { get; }
        public ICommand SelectImageCommand { get; }

        public EventSubmissionViewModel(EventManager eventManager)
        {
            _eventManager = eventManager;
            
            Categories = new List<string>
            {
                "Local Events",
                "Community Announcement",
                "Charity",
                "Entertainment",
                "Sports",
                "Cultural",
                "Educational",
                "Government Meeting",
                "Public Service",
                "Festivals",
                "Health and Wellness"
            };
            SubmitEventCommand = new RelayCommand(SubmitEvent, CanSubmitEvent);
            SelectImageCommand = new RelayCommand(SelectImage);
        }

        // Using the OpenFileDialog function been adapted from this source:
        // Author:  CSharp Corner
        // Article: OpenFileDialog In WPF
        // Source: https://www.c-sharpcorner.com/uploadfile/mahesh/openfiledialog-in-wpf/

        // Allow user to select an image
        private void SelectImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ThumbnailImagePath = openFileDialog.FileName;
                ThumbnailImage = File.ReadAllBytes(ThumbnailImagePath);
            }
        }

        // Check if a user provided a title and selcted a category
        private bool CanSubmitEvent()
        {
            return !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Category);
        }

        // Add the event or announcement to the list
        private void SubmitEvent()
        {
            var newEvent = new Event
            {
                Title = Title,
                Description = Description,
                Date = Date,
                Category = Category,
                Location = Location,
                Tags = TagsInput?.Split(',').Select(t => t.Trim()).ToList() ?? new List<string>(),
                ThumbnailImage = ThumbnailImage
            };

            _eventManager.AddEvent(newEvent);

            // Clear the form
            Title = Description = Category = Location = TagsInput = ThumbnailImagePath = string.Empty;
            Date = DateTime.Now;
            ThumbnailImage = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
