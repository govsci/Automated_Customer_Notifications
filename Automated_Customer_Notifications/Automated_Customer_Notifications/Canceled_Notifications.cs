using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automated_Customer_Notifications.Objects;
using Automated_Customer_Notifications.Classes;
using System.Data;
using System.Data.SqlClient;

namespace Automated_Customer_Notifications
{
    public class Canceled_Notifications
    {
        private List<AutoResponder> autoResponders = new List<AutoResponder>();
        public Canceled_Notifications()
        {
            GetReady();
            SendResponders();
        }

        private void GetReady()
        {
            AutoResponder auto = Database.GetResponder("111");
            List<OrderHeader> orders = Database.GetInlineData("GET CANCELED LINES");

            foreach (OrderHeader o in orders)
            {
                OrderHeader order = o;
                Database.GetOriginalPoData(ref order);
                order.Credentials = Database.GetCredentials(order.SellToCustomerNo);
                if (order.Credentials != null)
                {
                    try
                    {
                        BuildXml build = new OrderConfirmation(order, "reject");
                        order.XML = build.XML;

                        if (order.XML != null && build.LineCount>0)
                        {
                            string file = Constants.SendCxml(order, "OrderConfirmation");
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
                        Email.SendErrorMessage(ex, "Canceled_Notifications", "GetReady", null);
                    }
                }

                AutoResponder responder = new AutoResponder(auto.Header, auto.Footer, auto.Style, auto.Subject, auto.BodyStyle, auto.Body, auto.Cart, auto.ExtraChargesFormat);
                responder.EmailTo = order.ContactEmail;

                responder.EmailCc = Database.GetAdditionalEmails("GET 2ND EMAIL", order.SellToCustomerNo, order.ShipToAddress.Contact);
                string emailcc = Database.GetAdditionalEmails("BackOrderMonitor", order.SellToCustomerNo);
                if (emailcc.Length > 0)
                    responder.EmailCc += ";" + emailcc;

                SellToContact sellToContact = Database.GetTeamEmail(order.SellToCustomerNo);

                responder.Subject = responder.Subject.Replace("[OrderNo]", order.SalesOrderNo).Replace("[Customer PO No]", order.CustomerPONo);

                responder.Body = responder.Body.Replace("[Contact Name]", order.ContactName)
                    .Replace("[OrderNo]", order.SalesOrderNo)
                    .Replace("[Customer PO No]", order.CustomerPONo)
                    .Replace("[SalespersonName]", sellToContact != null && sellToContact.SalespersonName.Length > 0 ? sellToContact.SalespersonName : order.SalespersonName)
                    .Replace("[SalespersonEmailAddress]", sellToContact != null && sellToContact.SalespersonEmail.Length > 0 ? sellToContact.SalespersonEmail : order.SalespersonEmail)
                    .Replace("[SalespersonPhoneNumber]", sellToContact != null && sellToContact.SalespersonPhone.Length > 0 ? sellToContact.SalespersonPhone : order.SalespersonPhone)
                    .Replace("[SalesTeamEmail]", sellToContact != null && sellToContact.TeamEmail.Length > 0 ? sellToContact.TeamEmail : order.SalesTeamEmail)
                    .Replace("[SalesTeamPhoneNumber]", order.SalesTeamPhone);

                if (sellToContact != null && sellToContact.TeamEmail.Length > 0)
                    responder.EmailBcc = sellToContact.TeamEmail;

                if (responder.EmailBcc.Length > 0)
                    responder.EmailBcc = responder.EmailBcc + ";" + order.SalespersonEmail;
                else
                    responder.EmailBcc = order.SalespersonEmail;

                string cart = "";
                foreach (OrderLine line in order.Lines)
                    cart += responder.Cart.Replace("[SupplierPartNo]", line.VendorName + "<br>" + line.VendorItemNo)
                                .Replace("[Description]", line.Description)
                                .Replace("[UOM]", line.UOM)
                                .Replace("[Quantity]", line.Qty.ToString("G29"));

                responder.Body = responder.Body.Replace("[cart]", cart);
                responder.Order = order;
                autoResponders.Add(responder);
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
                    Email.SendErrorMessage(ex, "Canceled_Notifications", "SendResponders", new SqlCommand(auto.EmailTo + ";" + auto.EmailCc + ";" + auto.EmailBcc));
                }
                Database.InsertAutoResponder(auto, body, "Canceled Notification");
            }
        }
    }
}
