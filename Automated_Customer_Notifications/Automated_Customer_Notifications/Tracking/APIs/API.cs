using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automated_Customer_Notifications.Tracking.APIs
{
    abstract class API
    {
        public string Status = "";
        public DateTime StatusDatetime = new DateTime(1753,1,1);
        public DateTime EstimatedDeliveryDate = new DateTime(1753, 1, 1);
        public abstract void GetTrackingInfo(string trackingNumber);
    }
}
