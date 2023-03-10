using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automated_Customer_Notifications.Objects
{
    public class TrackingResults
    {
        public TrackingResults(string status, DateTime time)
        {
            Status = status;
            StatusDateTime = time;
        }

        public string Status { get; }
        public DateTime StatusDateTime { get; }
    }
}
