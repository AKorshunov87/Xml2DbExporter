using Xml2DbExporter.Data;
using Xml2DbExporter.Xml;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

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

        #endregion

        #region Events

        /// <summary>
        /// Fires every time when export progress changed or exporter have message to report
        /// </summary>
        public event ExportProgressChangedEventHandler ExportProgressChanged;

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor without setting of the xml file path
        /// </summary>
        public Exporter() {
            this.xmlFilePath = String.Empty;
            InitializeXmlReaderSettings();
            InitializeExportWorker();
        }

        /// <summary>
        /// Constructor with the xml file path set
        /// </summary>
        /// <param name="xmlFilePath">Path to the Xml File for export</param>
        public Exporter(string xmlFilePath) : base() {
            this.xmlFilePath = xmlFilePath;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Export Xml file data to DataBase Order table
        /// </summary>
        public void Export() {
            exportWorker.RunWorkerAsync();
        }

        #endregion

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

        #region Helpers

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
                        if (IsDuplicateOrder(order.OrderValue)) {
                            progressArgs = new ExportProgressChangedEventArgs(ExportProgressType.DuplicateRecordFound, progressPercentage, order.ToString());
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

        bool IsDuplicateOrder(string orderValue) {
            bool isDuplicate = false;
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                string commandText = "SELECT * FROM Order WHERE OrderValue=@orderValue";
                SqlCommand command = new SqlCommand(commandText);
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@orderValue", orderValue);
                connection.Open();
                using (SqlDataReader dataReader = command.ExecuteReader()) {
                    isDuplicate = dataReader.HasRows;
                }
            }
            return isDuplicate;
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
