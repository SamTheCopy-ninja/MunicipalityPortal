using System;
using System.Collections.Generic;
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
//using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using MunicipalityPortal.Managers;
using MunicipalityPortal.Models;

namespace MunicipalityPortal
{
    /// <summary>
    /// Interaction logic for ViewReports.xaml
    /// </summary>
    public partial class ViewReports : Window
    {
        

        public ViewReports()
        {
            InitializeComponent();

            // Load reports stored in the ObservableCollection<T> list
            LoadReports();
        }

        private void LoadReports()
        {
            // Log number of reports in list, to check if they exist in to the collection
            Console.WriteLine($"Loading {IssueManager.ReportedIssues.Count} issues...");

            // Populate the ListBox with the reports stored in the collection/issue manager
            ReportsListBox.ItemsSource = IssueManager.ReportedIssues;
            // ReportsListBox.DisplayMemberPath = "DisplaySummary";
        }

        private void ReportsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if the report in the ListBox is selected by the user
            if (ReportsListBox.SelectedItem is Issue selectedIssue)
            {
                LocationTextBlock.Text = selectedIssue.Location;
                CategoryTextBlock.Text = selectedIssue.Category;
                DescriptionTextBox.Text = selectedIssue.Description;
                ReportedAtTextBlock.Text = selectedIssue.ReportedAt.ToString("g");

                // Clear previous media when viewing a different list item, and hide the ImageView
                MediaImage.Source = null;
                MediaImage.Visibility = Visibility.Collapsed;

                if (selectedIssue.Attachment != null)
                {
                    ViewMediaButton.IsEnabled = true;
                    ViewMediaButton.Content = $"View {selectedIssue.Attachment.FileName}";
                }
                else
                {
                    ViewMediaButton.IsEnabled = false;
                    ViewMediaButton.Content = "No Media Attached";
                }
            }

        }

        // Viewing media from the device has been adapted from this source:
        // Author:  CSharp Tutorial
        // Article: C# Path
        // Source: https://www.csharptutorial.net/csharp-file/csharp-path/ 
        private void ViewMediaButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve media from the list by accessing it's path on the device
            if (ReportsListBox.SelectedItem is Issue selectedIssue && selectedIssue.Attachment != null)
            {
                string fileExtension = Path.GetExtension(selectedIssue.Attachment.FilePath).ToLower();

                if (fileExtension == ".jpg" || fileExtension == ".png")
                {
                    DisplayImage(selectedIssue.Attachment.FilePath);
                }
                else if (fileExtension == ".pdf")
                {
                    OpenPdf(selectedIssue.Attachment.FilePath);
                }
            }
        }


        // Displaying images has been adapted from this source:
        // Author:  Microsoft
        // Article: How to: Use a BitmapImage
        // Source: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-use-a-bitmapimage?view=netframeworkdesktop-4.8 
        private void DisplayImage(string imagePath)
        {
            // Access the image path and display the image in the app
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                MediaImage.Source = bitmap;
                MediaImage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying image: {ex.Message}");
                MediaImage.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenPdf(string pdfPath)
        {
            // Open PDF attachments by using the user's default PDF Viewer
            try
            {
                Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening PDF: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ReportIssues reportIssuesPage = new ReportIssues();
            reportIssuesPage.Show();
            this.Close();
        }

       
    }
}
