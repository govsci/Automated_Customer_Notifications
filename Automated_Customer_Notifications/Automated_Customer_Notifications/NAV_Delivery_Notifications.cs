using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automated_Customer_Notifications.Objects;
using Automated_Customer_Notifications.Classes;
using Automated_Customer_Notifications.Tracking;
using System.Data;
using System.Data.SqlClient;

namespace Automated_Customer_Notifications
{
    public class NAV_Delivery_Notifications
    {
        private List<AutoResponder> autoResponders = new List<AutoResponder>();
        public NAV_Delivery_Notifications()
        {
            GetReady();
            SendResponders();
        }

        private void GetReady()
        {
            List<AutoResponder> autos = Database.GetAutoResponders("GET DELIVERY NOTICES");
            foreach(AutoResponder auto in autos)
            {
                try
                {
                    TrackingResults tracking = GetTrackingStatus.Get(auto.Order.TrackingCarrier, auto.Order.TrackingNo);
                    if (tracking != null && tracking.Status == "Delivered")
                    {
                        auto.Subject = auto.Subject.Replace("Shipment Notification", "Delivered!");
                        auto.Body = auto.Body.Replace("Shipment Notification", "Delivered!")
                            .Replace("The following has been shipped and is on the way.", $"The following has been delivered by <b>{auto.Order.TrackingCarrier}</b> on <b>{tracking.StatusDateTime.ToString(@"MM/dd/yyyy a\t hh:mm tt")}</b>.");
                        autoResponders.Add(auto);
                    }
                    else if (tracking!=null && tracking.Status == "Tracking Not Available")
                    {
                        Database.UpdateDeliveryCheck(auto.Order.SalesOrderNo, auto.Order.CustomerPONo);
                    }
                }
                catch(Exception ex)
                {
                    Email.SendErrorMessage(ex, "NAV_Delivery_Notifications", "GetReady", null);
                }
            }
        }

        private void SendResponders()
        {
            foreach (AutoResponder auto in autoResponders)
            {
                string body = auto.Header.Replace("[style]", auto.Style) + auto.Body.Replace("[style]", auto.BodyStyle) + auto.Footer;
                try
                {
                    if (Email.TestEmail(auto.EmailTo, true) && Constants.DeploymentMode == "production")
                        Email.SendMail(body, auto.Subject, auto.EmailTo, auto.EmailCc, auto.EmailBcc, "", true);
                    else if (Constants.DeploymentMode == "test")
                        Email.SendMail(body, auto.Subject, Constants.TestEmail, "", "", "", true);
                }
                catch (Exception ex)
                {
                    Email.SendErrorMessage(ex, "NAV_Delivery_Notifications", "SendResponders", new SqlCommand(auto.EmailTo + ";" + auto.EmailCc + ";" + auto.EmailBcc));
                }
                Database.InsertAutoResponder(auto, body, "Delivery Notification", auto.ID);
            }
        }
    }
}
