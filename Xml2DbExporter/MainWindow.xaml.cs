using Xml2DbExporter.Data;
using Xml2DbExporter.Export;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace Xml2DbExporter {
    public partial class MainWindow : Window {

        readonly string connectionString = ConfigurationManager.ConnectionStrings["TestDataBase"].ConnectionString;
        Exporter exporter;
        LinkedList<OrderModel> duplicatesRows = new LinkedList<OrderModel>();
        LinkedList<OrderModel> ordersFromDBRows = new LinkedList<OrderModel>();
        LinkedList<OrderModel> insertedRows = new LinkedList<OrderModel>();

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
                exporter.XmlFilePath = txtBoxXmlFilePath.Text;
                exporter.ConnectionString = connectionString;
                PrepareUIForExport();
                exporter.Export();
            }
        }

        void ExportProgressChanged(object sender, ExportProgressChangedEventArgs e) {
            this.Dispatcher.Invoke(() => {
                switch (e.ExportProgressType) {
                    case ExportProgressType.DuplicateRecordFound:
                        duplicatesRows.AddLast(e.UserState as OrderModel);
                        break;
                    case ExportProgressType.RecordInserted:
                        insertedRows.AddLast(e.UserState as OrderModel);
                        ordersFromDBRows.AddLast(e.UserState as OrderModel);
                        break;
                    case ExportProgressType.ExportCompleted:
                    case ExportProgressType.ExportCancelled:
                        txtBlockExportLog.Text = String.Format("{0}\n{1}", txtBlockExportLog.Text, e.UserState.ToString());
                        UpdateUI();
                        break;
                    default:
                        txtBlockExportLog.Text = String.Format("{0}\n{1}", txtBlockExportLog.Text, e.UserState.ToString());
                        break;
                }
                if (e.ProgressPercentage > 0)
                    PBExport.Value = e.ProgressPercentage;
            });
        }

        void ExportButtonEnable(bool enable) {
            btnExport.Content = enable ? "Export To DB" : "Cancel Export";
            btnBrowse.IsEnabled = enable;
        }

        void PrepareUIForExport() {
            dgOrders.ItemsSource = null;
            dgInserted.ItemsSource = null;
            dgDuplicates.ItemsSource = null;
            LoadOrdersFromDB();
            duplicatesRows.Clear();
            insertedRows.Clear();
            PBExport.Value = 0;
            ExportButtonEnable(false);
        }

        void UpdateUI() {
            ExportButtonEnable(true);
            dgOrders.ItemsSource = ordersFromDBRows;
            dgInserted.ItemsSource = insertedRows;
            dgDuplicates.ItemsSource = duplicatesRows;
        }

        void LoadOrdersFromDB() {
            ordersFromDBRows.Clear();
            LinkedList<OrderModel> loadedList = new LinkedList<OrderModel>();
            if (CheckConnection()) {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    string commandText = "SELECT * FROM Orders";
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
                            order.DateTimeUpdated = Convert.ToDateTime(dataReader["DateTimeUpdated"]);
                            order.OrderDate = Convert.ToDateTime(dataReader["OrderDate"]);
                            order.OrderStatus = Convert.ToInt32(dataReader["OrderStatus"]);
                            order.OrderType = Convert.ToInt32(dataReader["OrderType"]);
                            order.OrderValue = dataReader["OrderValue"].ToString();
                            loadedList.AddLast(order);
                        }
                    }
                }
                ordersFromDBRows = loadedList;
                dgOrders.ItemsSource = loadedList;
            }
        }

        bool CheckConnection() {
            bool isOk = true;
            try {
                using (SqlConnection connection = new SqlConnection(connectionString))
                    connection.Open();
            }
            catch (SqlException) {
                MessageBox.Show("Database connection string is incorrect.");
                isOk = false;
            }
            return isOk;
        }
    }
}
