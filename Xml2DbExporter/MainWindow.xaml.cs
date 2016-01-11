using Xml2DbExporter.Data;
using System.Windows;
using System.Collections.ObjectModel;

namespace Xml2DbExporter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public ObservableCollection<OrderModel> DuplicatesRows = new ObservableCollection<OrderModel>();

        public MainWindow() {
            InitializeComponent();
        }
    }
}
