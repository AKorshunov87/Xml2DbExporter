using Xml2DbExporter.Data;
using System;
using System.ComponentModel;

namespace Xml2DbExporter.Export {
    /// <summary>
    /// Class that uses for reports about export progress change from Exporter to UI
    /// </summary>
    public class ExportProgressChangedEventArgs : ProgressChangedEventArgs {

        ExportProgressType progressType;

        /// <summary>
        /// Export progress type - XmlParseError, XmlParseWarning, RecordInsert, Duplicate, etc
        /// </summary>
        public ExportProgressType ExportProgressType { get { return progressType; } }

        /// <summary>
        /// Initializes a new instance of the ExportProgressChangedEventArgs class
        /// </summary>
        /// <param name="type">Export progress type</param>
        /// <param name="progressPercentage">Export progress percentage</param>
        public ExportProgressChangedEventArgs(ExportProgressType type, int progressPercentage, object userState)
            : base(progressPercentage, userState) {
            this.progressType = type;
        }
    }

    public delegate void ExportProgressChangedEventHandler(Object sender, ExportProgressChangedEventArgs e);
}
