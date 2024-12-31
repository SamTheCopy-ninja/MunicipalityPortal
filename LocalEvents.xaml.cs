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
using System.Windows.Shapes;
using MunicipalityPortal.Managers;
using MunicipalityPortal.Models;
using MunicipalityPortal.ViewModels;
using EventManager = MunicipalityPortal.Managers.EventManager;

namespace MunicipalityPortal
{
    /// <summary>
    /// Interaction logic for LocalEvents.xaml
    /// </summary>
    public partial class LocalEvents : Window
    {
        public LocalEvents()
        {
            InitializeComponent();

            var eventManager = new EventManager();

            DataContext = new EventsAndAnnouncementsViewModel(eventManager);
        }

        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
