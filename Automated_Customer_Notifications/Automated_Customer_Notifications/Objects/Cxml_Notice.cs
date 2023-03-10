using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automated_Customer_Notifications.Objects
{
    public class Cxml_Notice
    {
        public Cxml_Notice(int id, string doc, string so, string co, string shipno, string cxml, string url)
        {
            ID = id;
            DocumentType = doc;
            SalesOrderNumber = so;
            CustomerPONumber = co;
            ShipmentNo = shipno;
            CxmlFile = cxml;
            URL = url;
        }
        public int ID { get; }
        public string DocumentType { get; }
        public string SalesOrderNumber { get; }
        public string CustomerPONumber { get; }
        public string ShipmentNo { get; }
        public string CxmlFile { get; }
        public string URL { get; }
    }
}
