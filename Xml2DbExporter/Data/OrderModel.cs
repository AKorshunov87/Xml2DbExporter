using System;

namespace Xml2DbExporter.Data {
    /// <summary>
    /// Model for Order table
    /// </summary>
    public class OrderModel {

        public long OrderID { get; set; }

        public int CustomerID { get; set; }

        public DateTime? OrderDate { get; set; }

        public string OrderValue { get; set; }

        public int OrderStatus { get; set; }

        public DateTime? DateTimeAdded { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public int OrderType { get; set; }

        public override string ToString() {
            return String.Format("CustomerID: {0}; OrderDate: {1}; OrderValue: {2}; OrderStatus: {3}; OrderType: {4};", CustomerID, OrderDate, OrderValue, OrderStatus, OrderType);
        }
    }
}
