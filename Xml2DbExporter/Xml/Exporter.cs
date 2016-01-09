using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Xml2DbExporter.Data;

namespace Xml2DbExporter.Xml {
    /// <summary>
    /// Provides ability to export data from xml to DataBase
    /// </summary>
    public class Exporter {

        #region Fields

        string xmlFilePath;
        XmlReaderSettings xmlReaderSettings;
        BackgroundWorker exportWorker;

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
            // Report to UI that parsing of xml starts
            // exportWorker.ReportProgress(0, "some object with report message");
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
            this.exportWorker.ProgressChanged += new ProgressChangedEventHandler(ExportProgressChanged);
        }

        void InitializeXmlReaderSettings() {
            this.xmlReaderSettings = new XmlReaderSettings();
            this.xmlReaderSettings.Schemas.Add(null, @"Xml\OrdersScheme.xsd");
            this.xmlReaderSettings.ValidationType = ValidationType.Schema;
            this.xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(XmlValidationHandler);
        }

        #endregion

        #region Helpers
        void ExportXml(object sender, DoWorkEventArgs e) {
            Orders ordersFromXml = ParseXml();
            ExportOrdersToDataBase(ordersFromXml);
        }

        void ExportOrdersToDataBase(Orders ordersFromXml) {
            if (ordersFromXml != null) {
                OrderDetails[] orderDetailsFromXml = ordersFromXml.OrderDetailsList;
                if (orderDetailsFromXml != null) {
                    for (int i = 0; i < orderDetailsFromXml.Length; i++) {
                        // Creating order for Order Table using OrderModel
                        OrderModel order = ordersFromXml.ToOrder(orderDetailsFromXml[i]);

                        if (true) {// connect to db Order Table and check if there any records with the same OrderValue) {
                            // Connect to Database and Insert New Record with unique OrderValue
                            exportWorker.ReportProgress(i / orderDetailsFromXml.Length * 70, "Object with detailed info about inserted record");
                        }
                        else {
                            // report record with duplicate OrderValue to UI
                            exportWorker.ReportProgress(i / orderDetailsFromXml.Length * 70, "Object with detailed info about duplicate");
                        }
                    }
                }
            }
        }

        Orders ParseXml() {
            XmlSerializer serializer = new XmlSerializer(typeof(Orders));
            Orders ordersFromXml = null;
            using (XmlReader reader = XmlReader.Create(xmlFilePath, xmlReaderSettings)) {
                ordersFromXml = serializer.Deserialize(reader) as Orders;
            }
            // Report to UI after parse and deserialization ends
            exportWorker.ReportProgress(30, "some object with report message");
            return ordersFromXml;
        }

        void ExportComplete(object sender, RunWorkerCompletedEventArgs e) {
            // public ExportCompleteEvent fire - send Complete or Cancel Message -> on ui save duplicate to file(csv)
        }

        void ExportProgressChanged(object sender, ProgressChangedEventArgs e) {
            // public ExportProgressChangedEvent fire -> depends on object type(e.g. Duplicate or Inserted or any other) UI will changes
        }

        void XmlValidationHandler(object sender, ValidationEventArgs e) {
            string message;
            if (e.Severity == XmlSeverityType.Warning) {
                message = String.Format("Warning: {0}", e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error) {
                message = String.Format("Error: {0}", e.Message);
            }
            // some kind of report to UI
            // RaiseXmlValidationEvent(message);
        }

        #endregion

    }
}
