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
    public class NAV_Order_Acknowledgement
    {
        private List<AutoResponder> autoResponders = new List<AutoResponder>();
        public NAV_Order_Acknowledgement()
        {
            GetReady();
            SendResponders();
        }

        private void GetReady()
        {
            AutoResponder auto = Database.GetResponder("103");
            List<OrderHeader> orders = Database.GetHeaderData("Order Ack");

            foreach(OrderHeader order in orders)
            {
                AutoResponder responder = new AutoResponder(auto.Header, auto.Footer, auto.Style, auto.Subject, auto.BodyStyle, auto.Body, auto.Cart, auto.ExtraChargesFormat);
                responder.Subject = responder.Subject.Replace("[Order No]", order.SalesOrderNo)
                    .Replace("[Customer PO No]", order.CustomerPONo);

                SellToContact sellToContact = Database.GetTeamEmail(order.SellToCustomerNo);

                responder.Body = responder.Body.Replace("[Contact Name]", order.ContactName)
                    .Replace("[Order Number]", order.SalesOrderNo)
                    .Replace("[Customer PO No]", order.CustomerPONo)
                    .Replace("[SalespersonName]", sellToContact != null && sellToContact.SalespersonName.Length > 0 ? sellToContact.SalespersonName : order.SalespersonName)
                    .Replace("[SalespersonEmailAddress]", sellToContact != null && sellToContact.SalespersonEmail.Length > 0 ? sellToContact.SalespersonEmail : order.SalespersonEmail)
                    .Replace("[SalespersonPhoneNumber]", sellToContact != null && sellToContact.SalespersonPhone.Length > 0 ? sellToContact.SalespersonPhone : order.SalespersonPhone)
                    .Replace("[SalesTeamEmail]", sellToContact != null && sellToContact.TeamEmail.Length > 0 ? sellToContact.TeamEmail : order.SalesTeamEmail)
                    .Replace("[SalesTeamPhoneNumber]", order.SalesTeamPhone);

                responder.EmailTo = order.ContactEmail;
                responder.EmailBcc = sellToContact != null && sellToContact.TeamEmail.Length > 0 ? sellToContact.TeamEmail : order.SalespersonEmail;
                string emailcc = Database.GetAdditionalEmails("OrderAcknowledgements", order.SellToCustomerNo);
                if (emailcc.Length > 0)
                    responder.EmailCc += ";" + emailcc;

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
                    //if (Email.TestEmail(auto.EmailTo, true))
                    // Email.SendMail(body, auto.Subject, auto.EmailTo, auto.EmailCc, auto.EmailBcc, "", true);
                    Email.SendMail(body, auto.Subject, "zlingelbach@govsci.com", "", "", "", true);
                }
                catch (Exception ex)
                {
                    Email.SendErrorMessage(ex, "NAV_Order_Acknowledgement", "SendResponders", new SqlCommand(auto.EmailTo + ";" + auto.EmailCc + ";" + auto.EmailBcc));
                }
                //Database.InsertAutoResponder(auto, body, "Order Acknowledgement");
            }
        }
    }
}
