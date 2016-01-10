namespace Xml2DbExporter.Export {
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
