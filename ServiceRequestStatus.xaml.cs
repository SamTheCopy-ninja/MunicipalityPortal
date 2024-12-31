using MunicipalityPortal.Managers;
using MunicipalityPortal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//using System.IO;
using Path = System.IO.Path;

namespace MunicipalityPortal
{
    /// <summary>
    /// Interaction logic for ServiceRequestStatus.xaml
    /// </summary>
    public partial class ServiceRequestStatus : Window
    {
        private Issue currentIssue;

        private const long MaxFileSize = 20 * 1024 * 1024; // 20MB in bytes
        private readonly string[] AllowedExtensions = { ".jpg", ".png", ".pdf" };
        private readonly string[] ImageExtensions = { ".jpg", ".png" };

        public ServiceRequestStatus()
        {
            InitializeComponent();
            LoadAllRequests();
            InitializeStatusComboBox();
        }

        // Add the statuses to the combo box, to allow users to update service requests
        private void InitializeStatusComboBox()
        {
            StatusUpdateComboBox.ItemsSource = Enum.GetValues(typeof(IssueStatus));
        }

        // Back to main menu
        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        // Search button to filter the service requests based on an ID
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchId = SearchIdTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchId))
            {
                MessageBox.Show("Please enter a request ID", "Search Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var issue = IssueManager.GetIssueById(searchId);
            if (issue != null)
            {
                DisplayIssueDetails(issue);
            }
            else
            {
                MessageBox.Show("No request found with that ID", "Not Found",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Fetch the issue details in the list
        private void DisplayIssueDetails(Issue issue)
        {
            currentIssue = issue;
            RequestDetailsGroup.Visibility = Visibility.Visible;

            // Existing detail updates
            RequestIdText.Text = $"Request ID: {issue.Id}";
            LocationText.Text = $"Location: {issue.Location}";
            CategoryText.Text = $"Category: {issue.Category}";
            StatusText.Text = $"Current Status: {issue.Status}";
            ReportedAtText.Text = $"Reported: {issue.ReportedAt:g}";
            DescriptionText.Text = $"Description: {issue.Description}";

            // Update status controls
            StatusUpdateComboBox.SelectedItem = issue.Status;
            StatusHistoryList.ItemsSource = issue.StatusHistory
                .OrderByDescending(sh => sh.Timestamp)
                .ToList();
            StatusCommentTextBox.Clear();

            // Handle media attachment preview
            UpdateMediaPreview(issue.Attachment);

            // Add related issues display
            DisplayRelatedIssues(issue);
        }

        // Display issues related to current one the user clicked on
        private void DisplayRelatedIssues(Issue issue)
        {
            var relatedIssues = IssueManager.GetRelatedIssues(issue.Id);

            // Create a new section for related issues if it doesn't exist
            var relatedGroup = FindName("RelatedIssuesGroup") as GroupBox;
            if (relatedGroup == null)
            {
                relatedGroup = new GroupBox
                {
                    Header = "Related Issues",
                    Margin = new Thickness(5, 10, 5, 5)
                };

                var relatedList = new ListView
                {
                    MaxHeight = 150,
                    Name = "RelatedIssuesList"
                };

                var gridView = new GridView();
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "ID",
                    DisplayMemberBinding = new Binding("Id"),
                    Width = 120
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Status",
                    DisplayMemberBinding = new Binding("Status"),
                    Width = 100
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Location",
                    DisplayMemberBinding = new Binding("Location"),
                    Width = 150
                });

                relatedList.View = gridView;
                relatedGroup.Content = relatedList;

                var parentGrid = RequestDetailsGroup.Content as Grid;
                parentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(relatedGroup, parentGrid.RowDefinitions.Count - 1);
                parentGrid.Children.Add(relatedGroup);

                RegisterName("RelatedIssuesGroup", relatedGroup);
                RegisterName("RelatedIssuesList", relatedList);
            }

            var listView = FindName("RelatedIssuesList") as ListView;
            if (listView != null)
            {
                listView.ItemsSource = relatedIssues;
            }
        }


        private void LoadAllRequests()
        {
            // Update to show issues in priority order
            AllRequestsList.ItemsSource = IssueManager.GetPriorityOrderedIssues();
        }

        private void UpdateMediaPreview(MediaAttachment attachment)
        {
            // Reset all preview controls
            ResetMediaControls();

            if (attachment == null || string.IsNullOrEmpty(attachment.FilePath))
            {
                MediaPreviewGroup.Visibility = Visibility.Collapsed;
                return;
            }

            MediaPreviewGroup.Visibility = Visibility.Visible;
            AttachmentInfoText.Text = $"File: {attachment.FileName} ({FormatFileSize(attachment.FileSize)})";
            OpenFileButton.Visibility = Visibility.Visible;

            string extension = Path.GetExtension(attachment.FileName).ToLower();

            if (ImageExtensions.Contains(extension))
            {
                DisplayImage(attachment.FilePath);
            }
            else if (extension == ".pdf")
            {
                DisplayPdfInfo(attachment);
            }
        }

        // Reset all the media controls if needed
        private void ResetMediaControls()
        {
            ImagePreview.Visibility = Visibility.Collapsed;
            DocumentPreview.Visibility = Visibility.Collapsed;
            OpenFileButton.Visibility = Visibility.Collapsed;
            ImagePreview.Source = null;
        }

        // Allow users on the service requests page to view any attached images (if included)
        private void DisplayImage(string filePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filePath);
                bitmap.EndInit();

                ImagePreview.Source = bitmap;
                ImagePreview.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Preview Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Allow users to view the PDF attached to a request
        private void DisplayPdfInfo(MediaAttachment attachment)
        {
            DocumentPreview.Text = $"PDF Document\n" +
                                 $"Size: {FormatFileSize(attachment.FileSize)}\n" +
                                 "Click 'Open File' to view the PDF.";
            DocumentPreview.Visibility = Visibility.Visible;
        }

        // Format the file sizes
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        // Button to open the file in either the browser for PDFs or use the images app for pictures
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (currentIssue?.Attachment != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = currentIssue.Attachment.FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "File Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Let users toggle the visibilty of the service request details
        private void ToggleDetailsVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (RequestDetailsGroup.Visibility == Visibility.Visible)
            {
                RequestDetailsGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                RequestDetailsGroup.Visibility = Visibility.Visible;
            }
        }

        // Fetch the details for the selected request
        private void AllRequestsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllRequestsList.SelectedItem is Issue selectedIssue)
            {
                DisplayIssueDetails(selectedIssue);
            }
        }

        // Save and update the status for a request
        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (currentIssue == null)
            {
                MessageBox.Show("Please select a service request first.", "No Request Selected",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StatusUpdateComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a new status.", "Status Required",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string comment = StatusCommentTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(comment))
            {
                MessageBox.Show("Please provide a comment explaining the status update.",
                              "Comment Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to update the status of request {currentIssue.Id} from " +
                $"{currentIssue.Status} to {StatusUpdateComboBox.SelectedItem}?",
                "Confirm Status Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                IssueStatus newStatus = (IssueStatus)StatusUpdateComboBox.SelectedItem;
                IssueManager.UpdateIssueStatus(currentIssue.Id, newStatus, comment);

                DisplayIssueDetails(currentIssue);
                AllRequestsList.Items.Refresh();

                MessageBox.Show("Status updated successfully!", "Update Complete",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

}
