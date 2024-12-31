using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MunicipalityPortal.Managers;
using MunicipalityPortal.Models;
//using System.Windows.Shapes;

namespace MunicipalityPortal
{
    /// <summary>
    /// Interaction logic for ReportIssues.xaml
    /// </summary>
    public partial class ReportIssues : Window
    {

        public ReportIssues()
        {
            InitializeComponent();
        }

        private MediaAttachment currentAttachment;

        private byte[] mediaBytes = null;
        private const long MaxFileSize = 20 * 1024 * 1024; // 20MB in bytes
        private readonly string[] AllowedExtensions = { ".jpg", ".png", ".pdf" };

        private FileInfo fileInfo = null; 
        private string filePath = null;


        // Using the OpenFileDialog function been adapted from this source:
        // Author:  CSharp Corner
        // Article: OpenFileDialog In WPF
        // Source: https://www.c-sharpcorner.com/uploadfile/mahesh/openfiledialog-in-wpf/
        private void AttachMediaButton_Click(object sender, RoutedEventArgs e)
        {
            // Allow users to access files on their device, for attachment in reports
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image and PDF files (*.jpg, *.png, *.pdf)|*.jpg;*.png;*.pdf";

            if (dlg.ShowDialog() == true)
            {
                filePath = dlg.FileName;
                fileInfo = new FileInfo(filePath);

                string fileExtension = fileInfo.Extension.ToLower();

                if (fileInfo.Length > MaxFileSize)
                {
                    MessageBox.Show("The selected file exceeds the 20MB limit. Please choose a smaller file.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetMediaPreview();
                    return;
                }

                if (Array.IndexOf(AllowedExtensions, fileExtension) < 0)
                {
                    MessageBox.Show("Invalid file type. Only .jpg, .png, and .pdf files are allowed.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetMediaPreview();
                    return;
                }

                mediaBytes = File.ReadAllBytes(filePath);
                DisplayMediaPreview(fileExtension);
                UpdateProgress(null, null);
            }

        }

        // Preview image or PDF file when user is filling in report info
        // Please see the ViewReports.xaml.cs file for code attribution about this method (File paths and Images)
        private void DisplayMediaPreview(string fileExtension)
        {
            MediaPreviewBorder.Visibility = Visibility.Visible;
            MediaFileNameTextBlock.Text = Path.GetFileName(filePath);

            if (fileExtension == ".pdf")
            {
                MediaPreviewImage.Visibility = Visibility.Collapsed;
                PdfPreviewIcon.Visibility = Visibility.Visible;
            }
            else
            {
                MediaPreviewImage.Visibility = Visibility.Visible;
                PdfPreviewIcon.Visibility = Visibility.Collapsed;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                MediaPreviewImage.Source = bitmap;
            }
        }
        
        // After a user submits their report reset media preview
        private void ResetMediaPreview()
        {
            MediaPreviewBorder.Visibility = Visibility.Collapsed;
            MediaFileNameTextBlock.Text = string.Empty;
            MediaPreviewImage.Source = null;
            MediaPreviewImage.Visibility = Visibility.Collapsed;
            PdfPreviewIcon.Visibility = Visibility.Collapsed;
            mediaBytes = null;
            fileInfo = null;
            filePath = null;
        }

        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        // Functionality for submittting reports and adding the report data to the ObservableCollection<T> List
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string location = LocationTextBox.Text;
            string category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string description = new TextRange(DescriptionRichTextBox.Document.ContentStart, DescriptionRichTextBox.Document.ContentEnd).Text.Trim();

            // Validate Location, Category, and Description
            if (string.IsNullOrEmpty(location) || !Regex.IsMatch(location, @"^[a-zA-Z0-9\s,.-]+$")) // Regex to make sure location is valid
            {
                MessageBox.Show("Please enter a valid location.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(category))
            {
                MessageBox.Show("Please select a category.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(description) || description.Length < 10)
            {
                MessageBox.Show("Please provide a detailed description.", "Insufficient Details", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // If media is attached, create a MediaAttachment object
            MediaAttachment currentAttachment = null;
            if (mediaBytes != null && fileInfo != null)
            {
                currentAttachment = new MediaAttachment(
                    fileInfo.Name,
                    fileInfo.Extension,
                    fileInfo.Length,
                    filePath
                );
            }

            // Create new issue for user report
            Issue newIssue = new Issue(LocationTextBox.Text,
                (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                new TextRange(DescriptionRichTextBox.Document.ContentStart, DescriptionRichTextBox.Document.ContentEnd).Text.Trim(),
                currentAttachment);

            // Add reported issue to ObservableCollection<T> List
            IssueManager.AddIssue(newIssue);

            MessageBox.Show("Issue submitted successfully!", "Submission Complete", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear input fields and reset media preview
            LocationTextBox.Clear();
            CategoryComboBox.SelectedIndex = -1;
            DescriptionRichTextBox.Document.Blocks.Clear();
            ResetMediaPreview();
            UpdateProgress(null, null);
        }

        // Progress bar that dynamically fills up as user fills in their report
        private void UpdateProgress(object sender, EventArgs e)
        {
            int progress = 0;
            string encouragementMessage = "";

            if (!string.IsNullOrWhiteSpace(LocationTextBox.Text))
            {
                progress += 25;
            }

            if (CategoryComboBox.SelectedIndex != -1)
            {
                progress += 25;
            }

            if (!string.IsNullOrWhiteSpace(new TextRange(DescriptionRichTextBox.Document.ContentStart, DescriptionRichTextBox.Document.ContentEnd).Text))
            {
                progress += 25;
            }

            if (mediaBytes != null)
            {
                progress += 25;
            }

            EngagementProgressBar.Value = progress;

            switch (progress)
            {
                case 25:
                    encouragementMessage = "Great start! Keep going!";
                    break;
                case 50:
                    encouragementMessage = "You're halfway there!";
                    break;
                case 75:
                    encouragementMessage = "Almost done! Just one more step!";
                    break;
                case 100:
                    encouragementMessage = "Excellent! You're ready to submit your report.";
                    break;
                default:
                    encouragementMessage = "Let's report an issue to improve our community!";
                    break;
            }

            EncouragementTextBlock.Text = encouragementMessage;
        }

        private void ViewReportsButton_Click(object sender, RoutedEventArgs e)
        {
            ViewReports viewReportsPage = new ViewReports();
            viewReportsPage.Show();
            this.Close();
        }
    }
}
