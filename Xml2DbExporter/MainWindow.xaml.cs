using Xml2DbExporter.Data;
using Xml2DbExporter.Export;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;

namespace Xml2DbExporter {
    public partial class MainWindow : Window {

        readonly string connectionString = ConfigurationManager.ConnectionStrings["TestDataBase"].ConnectionString;
        Exporter exporter;
        BindingList<OrderModel> duplicatesRows = new BindingList<OrderModel>();
        BindingList<OrderModel> ordersFromDBRows = new BindingList<OrderModel>();
        BindingList<OrderModel> insertedRows = new BindingList<OrderModel>();

        public MainWindow() {
            InitializeComponent();
            InitializeLists();
            dgOrders.ItemsSource = ordersFromDBRows;
            dgInserted.ItemsSource = insertedRows;
            dgDuplicates.ItemsSource = duplicatesRows;
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
                duplicatesRows.Clear();
                insertedRows.Clear();
                ordersFromDBRows.RaiseListChangedEvents = false;
                duplicatesRows.RaiseListChangedEvents = false;
                insertedRows.RaiseListChangedEvents = false;
                exporter.XmlFilePath = txtBoxXmlFilePath.Text;
                exporter.ConnectionString = connectionString;
                ExportButtonEnable(false);
                exporter.Export();
            }
        }

        void ExportProgressChanged(object sender, ExportProgressChangedEventArgs e) {
            this.Dispatcher.Invoke(() => {
                switch (e.ExportProgressType) {
                    case ExportProgressType.DuplicateRecordFound:
                        duplicatesRows.Add(e.UserState as OrderModel);
                        PBExport.Value = e.ProgressPercentage;
                        break;
                    case ExportProgressType.RecordInserted:
                        insertedRows.Add(e.UserState as OrderModel);
                        ordersFromDBRows.Add(e.UserState as OrderModel);
                        PBExport.Value = e.ProgressPercentage;
                        break;
                    case ExportProgressType.ExportCompleted:
                    case ExportProgressType.ExportCancelled:
                        txtBlockExportLog.Text = String.Format("{0}\n{1}", txtBlockExportLog.Text, e.UserState.ToString());
                        PBExport.Value = e.ProgressPercentage;
                        ExportButtonEnable(true);
                        UpdateUI();
                        break;
                    default:
                        PBExport.Value = e.ProgressPercentage;
                        txtBlockExportLog.Text = String.Format("{0}\n{1}", txtBlockExportLog.Text, e.UserState.ToString());
                        break;
                }
            });
        }

        void ExportButtonEnable(bool enable) {
            btnExport.Content = enable ? "Export To DB" : "Cancel Export";
            btnBrowse.IsEnabled = enable;
        }

        void UpdateUI() {
            ordersFromDBRows.RaiseListChangedEvents = true;
            duplicatesRows.RaiseListChangedEvents = true;
            insertedRows.RaiseListChangedEvents = true;
            ordersFromDBRows.ResetBindings();
            duplicatesRows.ResetBindings();
            insertedRows.ResetBindings();
        }

        void InitializeLists() {
            ordersFromDBRows.AllowNew = true;
            duplicatesRows.AllowNew = true;
            insertedRows.AllowNew = true;
            ordersFromDBRows.AllowEdit = false;
            duplicatesRows.AllowEdit = false;
            insertedRows.AllowEdit = false;
            ordersFromDBRows.AllowRemove = false;
            duplicatesRows.AllowRemove = false;
            insertedRows.AllowRemove = false;
        }

        void LoadOrdersFromDB() {
            ordersFromDBRows.Clear();
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
                        ordersFromDBRows.Add(order);
                    }
                }
            }
        }
    }
}
