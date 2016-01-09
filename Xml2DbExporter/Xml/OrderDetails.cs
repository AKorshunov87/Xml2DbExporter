using System;
using System.Xml.Serialization;

namespace Xml2DbExporter.Xml {
    [SerializableAttribute]
    [XmlTypeAttribute(AnonymousType = true)]
    public class OrderDetails {

        string orderValue;
        int orderStatus;
        int orderType;

        [XmlElementAttribute]
        public string OrderValue {
            get { return orderValue; }
            set { orderValue = value; }
        }

        [XmlElementAttribute]
        public int OrderStatus {
            get { return orderStatus; }
            set { orderStatus = value; }
        }

        [XmlElementAttribute]
        public int OrderType {
            get { return orderType; }
            set { orderType = value; }
        }
    }
}
