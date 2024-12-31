using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MunicipalityPortal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReportIssuesButton_Click(object sender, RoutedEventArgs e)
        {
            ReportIssues reportIssuesPage = new ReportIssues();
            reportIssuesPage.Show();
            this.Close();
        }

        private void LocalEventsButton_Click(object sender, RoutedEventArgs e)
        {
            LocalEvents localEventsPage = new LocalEvents();
            localEventsPage.Show();
            this.Close();
        }

        private void ServiceRequestStatusButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceRequestStatus serviceRequestStatus = new ServiceRequestStatus();
            serviceRequestStatus.Show();
            this.Close();
        }
    }
}