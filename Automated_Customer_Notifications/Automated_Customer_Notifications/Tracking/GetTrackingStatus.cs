using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automated_Customer_Notifications.Tracking.APIs;
using Automated_Customer_Notifications.Objects;

namespace Automated_Customer_Notifications.Tracking
{
    public static class GetTrackingStatus
    {
        public static TrackingResults Get(string carrier, string trackingNumber)
        {
            try
            {
                API api = null;
                switch (carrier.ToUpper())
                {
                    case "FEDEX":
                        api = new Fedex();
                        break;
                    case "UPS":
                        api = new UPS();
                        break;
                    case "ODFL":
                        api = new ODFL();
                        break;
                        //case "DHL":
                        //api = new DHL();
                        //break;
                }

                if (api != null)
                {
                    api.GetTrackingInfo(trackingNumber);
                    return new TrackingResults(api.Status, api.StatusDatetime);
                }
                else
                    return new TrackingResults("Sorry, the requested carrier is not supported.", new DateTime(1753, 1, 1));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
