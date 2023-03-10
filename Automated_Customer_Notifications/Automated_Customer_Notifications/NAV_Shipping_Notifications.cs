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
    public class NAV_Shipping_Notifications
    {
        private List<AutoResponder> autoResponders = new List<AutoResponder>();
        public NAV_Shipping_Notifications()
        {
            GetReady();
            SendResponders();
        }

        private void GetReady()
        {
            AutoResponder auto = Database.GetResponder("107");
            List<OrderHeader> orders = Database.GetHeaderData("Nav Ship Notice Header");

            for(int i = 0; i<orders.Count;i++)
            {
                OrderHeader order = orders[i];
                Database.GetLineData(ref order, "Nav Ship Notice Line");
                Database.GetOriginalPoData(ref order);

                order.Credentials = Database.GetCredentials(order.SellToCustomerNo);
                if (order.Credentials != null && order.Lines.Count > 0 && order.Credentials.ASN == 1)
                {
                    try
                    {
                        BuildXml build = new ShipNotice(order);
                        order.XML = build.XML;

                        if (order.XML != null && build.LineCount > 0)
                        {
                            string file = Constants.SendCxml(order, "ShipNotice");
                            string[] fileSplit = file.Split('|');

                            if (fileSplit[0] == "SENT")
                            {
                                order.CxmlSent = true;
                                order.CxmlDateStamp = DateTime.Now;
                            }
                            else
                            {
                                order.CxmlDateStamp = new DateTime(1753, 1, 1);
                                order.CxmlSent = false;
                            }

                            order.CxmlFile = fileSplit[1];
                        }
                    }
                    catch(Exception ex)
                    {
                        Email.SendErrorMessage(ex, "NAV_Shipping_Notifications", "GetReady", null);
                    }
                }

                AutoResponder responder = new AutoResponder(auto.Header, auto.Footer, auto.Style, auto.Subject, auto.BodyStyle, auto.Body, auto.Cart, auto.ExtraChargesFormat);

                string pseudoTrackingNumber = "" + order.TrackingNo, pseudoTrackingCarrier = "" + order.TrackingCarrier;

                if (pseudoTrackingCarrier == "Delivered" || pseudoTrackingCarrier == "GSS" || pseudoTrackingCarrier == "Direct")
                {
                    pseudoTrackingCarrier = "GSS";
                    pseudoTrackingNumber = order.ShipmentNo;
                    order.EstDelDate = DateTime.Now;
                }
                else
                {
                    //TrackingResults tracking = GetTrackingStatus.Get(order.TrackingCarrier, order.TrackingNo);
                    //if (tracking != null)
                        //order.EstDelDate = tracking.StatusDateTime;
                }

                responder.Subject = responder.Subject.Replace("[OrderNo]", order.SalesOrderNo).Replace("[Customer PO No]", order.CustomerPONo);

                string url = "";
                if (pseudoTrackingNumber.Length > 0 && order.URL.Length > 0)
                    url = "<a href='" + order.URL.Replace("[trackingNo]", pseudoTrackingNumber) + "'>" + pseudoTrackingNumber + "</a>";

                responder.Body = responder.Body.Replace("[Contact Name]", order.ContactName)
                    .Replace("[OrderNumber]", order.SalesOrderNo)
                    .Replace("[ShipDate]", order.ShipmentDate.ToShortDateString())
                    .Replace("[Customer PO No]", order.CustomerPONo)
                    .Replace("[Comments]", "")
                    .Replace("[ContactPhone]", order.ContactPhone)
                    .Replace("[ContactEmail]", order.ContactEmail)
                    .Replace("[Carrier]", pseudoTrackingCarrier)
                    .Replace("[TrackingNumber]", url.Length > 0 ? url : pseudoTrackingNumber)
                    .Replace("[EstDeliveryDate]", order.EstDelDate != new DateTime(1753, 01, 01) ? order.EstDelDate.ToShortDateString() : "")
                    .Replace("[ShipToName]", order.ShipToAddress.Name)
                    .Replace("[ShipToContact]", order.ShipToAddress.Contact)
                    .Replace("[ShipToAddress1]", order.ShipToAddress.Street)
                    .Replace("[ShipToAddress2]", " " + order.ShipToAddress.Street2)
                    .Replace("[ShipToCity]", order.ShipToAddress.City)
                    .Replace("[ShipToState]", order.ShipToAddress.State)
                    .Replace("[ShipToZip]", order.ShipToAddress.PostCode);

                responder.EmailTo = order.ContactEmail;

                responder.EmailCc = Database.GetAdditionalEmails("GET 2ND SHIP EMAIL", order.SellToCustomerNo, order.ShipToAddress.Contact);
                string emailcc = Database.GetAdditionalEmails("ShipmentNotifications", order.SellToCustomerNo);
                if (emailcc.Length > 0)
                    responder.EmailCc += ";" + emailcc;

                SellToContact sellToContact = Database.GetTeamEmail(order.SellToCustomerNo);

                responder.Body = responder.Body.Replace("[SalespersonName]", sellToContact != null && sellToContact.SalespersonName.Length > 0 ? sellToContact.SalespersonName : order.SalespersonName)
                   .Replace("[SalespersonEmailAddress]", sellToContact != null && sellToContact.SalespersonEmail.Length > 0 ? sellToContact.SalespersonEmail : order.SalespersonEmail)
                   .Replace("[SalespersonPhoneNumber]", sellToContact != null && sellToContact.SalespersonPhone.Length > 0 ? sellToContact.SalespersonPhone : order.SalespersonPhone)
                   .Replace("[SalesTeamEmail]", sellToContact != null && sellToContact.TeamEmail.Length > 0 ? sellToContact.TeamEmail : order.SalesTeamEmail)
                   .Replace("[SalesTeamPhoneNumber]", order.SalesTeamPhone);

                responder.EmailBcc = sellToContact != null && sellToContact.TeamEmail.Length > 0 ? sellToContact.TeamEmail : order.SalespersonEmail;

                string cart = "";

                foreach (OrderLine line in order.Lines)
                {
                    cart += responder.Cart.Replace("[SupplierPartNo]", $"{line.VendorName}<br />{line.VendorItemNo}")
                        .Replace("[Description]", line.Description)
                        .Replace("[UOM]", line.UOM)
                        .Replace("[Quantity]", line.Qty.ToString("G29"));
                }

                if (cart.Length > 0)
                {
                    responder.Body = responder.Body.Replace("[cart]", cart);
                    responder.Order = order;
                    autoResponders.Add(responder);
                }
            }
        }

        private void SendResponders()
        {
            foreach (AutoResponder auto in autoResponders)
            {
                if (auto.EmailTo.Length == 0)
                {
                    auto.EmailTo = auto.EmailCc;
                    auto.EmailCc = "";
                }

                if (auto.EmailTo.Length == 0)
                {
                    auto.EmailTo = auto.EmailBcc;
                    auto.EmailBcc = "";
                }

                string body = auto.Header.Replace("[style]", auto.Style) + auto.Body.Replace("[style]", auto.BodyStyle) + auto.Footer;
                try
                {
                    if (Email.TestEmail(auto.EmailTo, true) && Constants.DeploymentMode == "production" && auto.Order.ShipmentDate.Date >= DateTime.Now.Date)
                        Email.SendMail(body, auto.Subject, auto.EmailTo, auto.EmailCc, auto.EmailBcc, "", true);
                    else if (Constants.DeploymentMode == "test")
                        Email.SendMail(body, auto.Subject, Constants.TestEmail, "", "", "", true);
                }
                catch (Exception ex)
                {
                    Email.SendErrorMessage(ex, "NAV_Shipping_Notifications", "SendResponders", new SqlCommand(auto.EmailTo + ";" + auto.EmailCc + ";" + auto.EmailBcc));
                }
                Database.InsertAutoResponder(auto, body, "Shipment Notification");
            }
        }
    }
}
