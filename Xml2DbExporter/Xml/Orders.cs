using System;
using System.Xml.Serialization;
using Xml2DbExporter.Data;

namespace Xml2DbExporter.Xml {
    [SerializableAttribute]
    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Orders {

        int customerID;
        DateTime orderDate;
        OrderDetails[] orderDetailsList;

        [XmlElementAttribute]
        public int CustomerID {
            get { return customerID; }
            set { customerID = value; }
        }

        [XmlElementAttribute(DataType = "date")]
        public DateTime OrderDate {
            get { return orderDate; }
            set { orderDate = value; }
        }

        [XmlArrayItemAttribute("OrderDetails", IsNullable = false)]
        public OrderDetails[] OrderDetailsList {
            get { return orderDetailsList; }
            set { orderDetailsList = value; }
        }

        public OrderModel ToOrder(OrderDetails orderDetails) {
            OrderModel order = new OrderModel();

            order.CustomerID = this.CustomerID;
            order.OrderDate = this.OrderDate;

            order.OrderStatus = orderDetails.OrderStatus;
            order.OrderType = orderDetails.OrderType;
            order.OrderValue = orderDetails.OrderValue;
            return order;
        }
    }
}
