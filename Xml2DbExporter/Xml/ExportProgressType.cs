namespace Xml2DbExporter.Xml {
    public enum ExportProgressType {
        ParseXmlError,
        ParseXmlWarning,
        ParseXmlCompleted,
        DuplicateRecordFound,
        RecordInserted,
        ExportCompleted,
        ExportCancelled
    }
}
