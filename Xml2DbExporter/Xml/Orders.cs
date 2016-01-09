using System;
using System.Xml.Serialization;

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
    }
}
