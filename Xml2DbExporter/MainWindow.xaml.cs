using Xml2DbExporter.Data;
using Xml2DbExporter.Export;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace Xml2DbExporter {
    public partial class MainWindow : Window {

        readonly string connectionString = ConfigurationManager.ConnectionStrings["TestDataBase"].ConnectionString;
        Exporter exporter;
        
        public ObservableCollection<OrderModel> DuplicatesRows = new ObservableCollection<OrderModel>();
        public ObservableCollection<OrderModel> OrdersFromDBRows = new ObservableCollection<OrderModel>();
        public ObservableCollection<OrderModel> InsertedRows = new ObservableCollection<OrderModel>();

        public MainWindow() {
            InitializeComponent();
            LoadOrdersFromDB();
            exporter = new Exporter();
            exporter.ExportProgressChanged += new ExportProgressChangedEventHandler(ExportProgressChanged);
        }

        void btnBrowse_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.DefaultExt = "xml";
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 0;

            if (openFileDialog.ShowDialog() == true) {
                string filename = openFileDialog.FileName;
                txtBoxXmlFilePath.Text = filename;
            }
        }

        void btnExport_Click(object sender, RoutedEventArgs e) {
            if (exporter.IsBusy) {
                exporter.CancelExport();
                ExportButtonEnable(true);
            }
            else {
                PBExport.Value = 0;
                exporter.XmlFilePath = txtBoxXmlFilePath.Text;
                exporter.ConnectionString = connectionString;
                ExportButtonEnable(false);
                exporter.Export();
            }
        }

        void ExportProgressChanged(object sender, ExportProgressChangedEventArgs e) {
            switch (e.ExportProgressType) {
                case ExportProgressType.DuplicateRecordFound:
                    DuplicatesRows.Add(e.UserState as OrderModel);
                    PBExport.Value = e.ProgressPercentage;
                    break;
                case ExportProgressType.RecordInserted:
                    InsertedRows.Add(e.UserState as OrderModel);
                    PBExport.Value = e.ProgressPercentage;
                    break;
                case ExportProgressType.ExportCompleted:
                case ExportProgressType.ExportCancelled:
                    txtBlockExportLog.Text = String.Format("{0}\n{1}", txtBlockExportLog.Text, e.UserState.ToString());
                    ExportButtonEnable(true);
                    break;
                default:
                    PBExport.Value = e.ProgressPercentage;
                    txtBlockExportLog.Text = String.Format("{0}\n{1}", txtBlockExportLog.Text, e.UserState.ToString());
                    break;
            }
        }

        void ExportButtonEnable(bool enable) {
            btnExport.Content = enable ? "Export To DB" : "Cancel Export";
            btnBrowse.IsEnabled = enable;
        }

        void LoadOrdersFromDB() {
            OrdersFromDBRows.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                string commandText = "SELECT * FROM [testdb].[dbo].[Order]";
                SqlCommand command = new SqlCommand(commandText);
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                connection.Open();
                using (SqlDataReader dataReader = command.ExecuteReader()) {
                    while (dataReader.Read()) {
                        OrderModel order = new OrderModel();
                        order.OrderID = Convert.ToInt64(dataReader["OrderID"]);
                        order.CustomerID = Convert.ToInt32(dataReader["CustomerID"]);
                        order.DateTimeAdded = Convert.ToDateTime(dataReader["DateTimeAdded"]);
                        order.DateTimeUpdated = Convert.ToDateTime(dataReader["DataTimeUpdated"]);
                        order.OrderDate = Convert.ToDateTime(dataReader["OrderDate"]);
                        order.OrderStatus = Convert.ToInt32(dataReader["OrderStatus"]);
                        order.OrderType = Convert.ToInt32(dataReader["OrderType"]);
                        order.OrderValue = dataReader["OrderValue"].ToString();
                        OrdersFromDBRows.Add(order);
                    }
                }
            }
        }
    }
}
