using Xml2DbExporter.Data;
using Xml2DbExporter.Xml;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace Xml2DbExporter.Export {
    /// <summary>
    /// Provides ability to export data from xml to DataBase
    /// </summary>
    public class Exporter {

        #region Fields

        string xmlFilePath;
        XmlReaderSettings xmlReaderSettings;
        BackgroundWorker exportWorker;
        string connectionString;

        #endregion

        #region Properties
        /// <summary>
        /// Path to the Xml File
        /// </summary>
        public string XmlFilePath {
            get { return xmlFilePath; }
            set { xmlFilePath = value; }
        }

        /// <summary>
        /// Database connection string
        /// </summary>
        public string ConnectionString {
            get { return connectionString; }
            set { connectionString = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fires every time when export progress changed or exporter have message to report
        /// </summary>
        public event ExportProgressChangedEventHandler ExportProgressChanged;

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor without any settings
        /// </summary>
        public Exporter() {
            this.xmlFilePath = String.Empty;
            this.connectionString = String.Empty;
            InitializeXmlReaderSettings();
            InitializeExportWorker();
        }

        /// <summary>
        /// Constructor with the xml file path set
        /// </summary>
        /// <param name="xmlFilePath">Path to the Xml File for export</param>
        /// <param name="connectionString">Connection string to the DataBase</param>
        public Exporter(string xmlFilePath, string connectionString) : base() {
            this.xmlFilePath = xmlFilePath;
            this.connectionString = connectionString;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Export Xml file data to DataBase Order table
        /// </summary>
        public void Export() {
            if (FileAndConnectionIsOk())
                exportWorker.RunWorkerAsync();
        }

        #endregion

        #region Helpers
        
        #region Initialization
        void InitializeExportWorker() {
            this.exportWorker = new BackgroundWorker();
            this.exportWorker.WorkerReportsProgress = true;
            this.exportWorker.WorkerSupportsCancellation = true;
            this.exportWorker.DoWork += new DoWorkEventHandler(ExportXml);
            this.exportWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExportComplete);
            this.exportWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
        }

        void InitializeXmlReaderSettings() {
            this.xmlReaderSettings = new XmlReaderSettings();
            this.xmlReaderSettings.Schemas.Add(null, @"Xml\OrdersScheme.xsd");
            this.xmlReaderSettings.ValidationType = ValidationType.Schema;
            this.xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(XmlValidationHandler);
        }
        #endregion

        #region Export
        void ExportXml(object sender, DoWorkEventArgs e) {
            Orders ordersFromXml = ParseXml();
            ExportOrdersToDataBase(ordersFromXml);
        }

        void ExportOrdersToDataBase(Orders ordersFromXml) {
            if (ordersFromXml != null) {
                OrderDetails[] orderDetailsFromXml = ordersFromXml.OrderDetailsList;
                if (orderDetailsFromXml != null) {
                    for (int i = 0; i < orderDetailsFromXml.Length; i++) {
                        OrderModel order = ordersFromXml.ToOrder(orderDetailsFromXml[i]);
                        int progressPercentage = Convert.ToInt32(i / orderDetailsFromXml.Length * 70);
                        ExportProgressChangedEventArgs progressArgs = null;
                        OrderModel duplicateOrder = SelectDuplicateOrder(order.OrderValue);
                        if (duplicateOrder != null) {
                            progressArgs = new ExportProgressChangedEventArgs(ExportProgressType.DuplicateRecordFound, progressPercentage, duplicateOrder.ToString());
                        }
                        else {
                            InsertOrderRecord(order);
                            progressArgs = new ExportProgressChangedEventArgs(ExportProgressType.RecordInserted, progressPercentage);
                        }
                        exportWorker.ReportProgress(progressPercentage, progressArgs);
                    }
                }
            }
        }

        void ExportComplete(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Cancelled)
                OnExportProgressChanged(new ExportProgressChangedEventArgs(ExportProgressType.ExportCancelled, 100, "Export was cancelled!"));
            else
                OnExportProgressChanged(new ExportProgressChangedEventArgs(ExportProgressType.ExportCompleted, 100, "Export was completed successfully!"));
        }

        void ProgressChanged(object sender, ProgressChangedEventArgs e) {
            ExportProgressChangedEventArgs args = e.UserState as ExportProgressChangedEventArgs;
            if (args != null)
                OnExportProgressChanged(args);
        }

        bool FileAndConnectionIsOk() {
            string ext = Path.GetExtension(xmlFilePath);
            bool isOk = ext == ".xml";
            if (!isOk)
                OnExportProgressChanged(new ExportProgressChangedEventArgs(ExportProgressType.ExportCancelled, 0, "Xml File path is incorrect."));

            try {
                using (SqlConnection connection = new SqlConnection(connectionString))
                    connection.Open();
            }
            catch (SqlException) {
                OnExportProgressChanged(new ExportProgressChangedEventArgs(ExportProgressType.ExportCancelled, 0, "Database connection string is incorrect."));
                isOk = false;
            }
            return isOk;
        }
        #endregion

        #region Xml Parse
        Orders ParseXml() {
            XmlSerializer serializer = new XmlSerializer(typeof(Orders));
            Orders ordersFromXml = null;
            using (XmlReader reader = XmlReader.Create(xmlFilePath, xmlReaderSettings)) {
                ordersFromXml = serializer.Deserialize(reader) as Orders;
            }

            // Report to UI after parse and deserialization ends
            exportWorker.ReportProgress(30, new ExportProgressChangedEventArgs(ExportProgressType.ParseXmlCompleted, 30, "Xml parsing was completed successfully."));
            return ordersFromXml;
        }

        void XmlValidationHandler(object sender, ValidationEventArgs e) {
            if (e.Severity == XmlSeverityType.Warning)
                OnExportProgressChanged(new ExportProgressChangedEventArgs(ExportProgressType.ParseXmlWarning, 0, String.Format("Warning: {0}", e.Message)));
            else if (e.Severity == XmlSeverityType.Error)
                OnExportProgressChanged(new ExportProgressChangedEventArgs(ExportProgressType.ParseXmlError, 0, String.Format("Error: {0}", e.Message)));
        }
        #endregion

        #region DataBase
        void InsertOrderRecord(OrderModel order) {
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                string commandText = "INSERT INTO Order (CustomerID, OrderDate, OrderValue, OrderStatus, OrderType) VALUES (@CustomerID, @OrderDate, @OrderValue, @OrderStatus, @OrderType)";
                SqlCommand command = new SqlCommand(commandText);
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@CustomerID", order.CustomerID);
                command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                command.Parameters.AddWithValue("@OrderValue", order.OrderValue);
                command.Parameters.AddWithValue("@OrderStatus", order.OrderStatus);
                command.Parameters.AddWithValue("@OrderType", order.OrderType);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        OrderModel SelectDuplicateOrder(string orderValue) {
            OrderModel order = null;
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                string commandText = "SELECT TOP 1 * FROM Order WHERE OrderValue=@orderValue";
                SqlCommand command = new SqlCommand(commandText);
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@orderValue", orderValue);
                connection.Open();
                using (SqlDataReader dataReader = command.ExecuteReader()) {
                    while (dataReader.Read()) {
                        order = new OrderModel();
                        order.OrderID = Convert.ToInt64(dataReader["OrderID"]);
                        order.CustomerID = Convert.ToInt32(dataReader["CustomerID"]);
                        order.DateTimeAdded = Convert.ToDateTime(dataReader["DateTimeAdded"]);
                        order.DateTimeUpdated = Convert.ToDateTime(dataReader["DataTimeUpdated"]);
                        order.OrderDate = Convert.ToDateTime(dataReader["OrderDate"]);
                        order.OrderStatus = Convert.ToInt32(dataReader["OrderStatus"]);
                        order.OrderType = Convert.ToInt32(dataReader["OrderType"]);
                        order.OrderValue = dataReader["OrderValue"].ToString();
                    }
                }
            }
            return order;
        }
        #endregion

        #region Events
        protected virtual void OnExportProgressChanged(ExportProgressChangedEventArgs e) {
            ExportProgressChangedEventHandler handler = ExportProgressChanged;
            if (handler != null)
                handler(this, e);
        }
        #endregion

        #endregion
    }
}
