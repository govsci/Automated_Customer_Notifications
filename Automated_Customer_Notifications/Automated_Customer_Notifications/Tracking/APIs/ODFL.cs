using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automated_Customer_Notifications.API_ODFL;

namespace Automated_Customer_Notifications.Tracking.APIs
{
    class ODFL : API
    {
        public override void GetTrackingInfo(string trackingNumber)
        {
            try
            { 
                TraceService service = new TraceService();
                TraceResult request = service.getTraceData(trackingNumber, "p");
                Status = request.status;
                if (Status == "Delivered")
                    StatusDatetime = DateTime.Parse(request.proDate);
            }
            catch (Exception e)
            {
                throw new Exception("ODFL : " + e.ToString());
            }
        }
    }
}
