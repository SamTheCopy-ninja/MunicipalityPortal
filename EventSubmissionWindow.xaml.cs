using MunicipalityPortal.Managers;
using MunicipalityPortal.ViewModels;
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
using EventManager = MunicipalityPortal.Managers.EventManager;

namespace MunicipalityPortal
{
    /// <summary>
    /// Interaction logic for EventSubmissionWindow.xaml
    /// </summary>
    public partial class EventSubmissionWindow : Window
    {
        public EventSubmissionWindow(EventManager eventManager)
        {
            InitializeComponent();
            DataContext = new EventSubmissionViewModel(eventManager);
        }
    }
}
