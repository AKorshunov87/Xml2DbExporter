using System;

namespace Xml2DbExporter.Export {
    /// <summary>
    /// Class that uses for reports about export progress change from Exporter to UI
    /// </summary>
    public class ExportProgressChangedEventArgs : EventArgs {

        string message;
        ExportProgressType progressType;
        int progressPercentage;

        /// <summary>
        /// Message that needs to be shown in UI
        /// </summary>
        public string Message { get { return message; } }

        /// <summary>
        /// Export progress type - XmlParseError, XmlParseWarning, RecordInsert, Duplicate, etc
        /// </summary>
        public ExportProgressType ExportProgressType { get { return progressType; } }

        /// <summary>
        /// Export progress percentage
        /// </summary>
        public int ProgressPercentage { get { return progressPercentage; } }

        /// <summary>
        /// Initializes a new instance of the ExportProgressChangedEventArgs class
        /// </summary>
        /// <param name="type">Export progress type</param>
        /// <param name="progressPercentage">Export progress percentage</param>
        public ExportProgressChangedEventArgs(ExportProgressType type, int progressPercentage) {
            this.progressType = type;
            this.progressPercentage = progressPercentage;
        }

        /// <summary>
        /// Initializes a new instance of the ExportProgressChangedEventArgs with message to UI
        /// </summary>
        /// <param name="type">Export progress type</param>
        /// <param name="progressPercentage">Export progress percentage</param>
        /// <param name="message">Message that needs to be shown in UI</param>
        public ExportProgressChangedEventArgs(ExportProgressType type, int progressPercentage, string message) : this(type, progressPercentage) {
            this.message = message;
        }
    }

    public delegate void ExportProgressChangedEventHandler(Object sender, ExportProgressChangedEventArgs e);
}
