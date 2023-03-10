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
    public class NAV_Inventory_Order_Confirmation
    {
        private List<AutoResponder> autoResponders = new List<AutoResponder>();
        public NAV_Inventory_Order_Confirmation()
        {
            GetReady();
            SendResponders();
        }

        public void GetReady()
        {
            AutoResponder auto = Database.GetResponder("112");
            List<OrderHeader> orders = Database.GetHeaderData("NAV INV ORDER HEADER");

            for (int i = 0; i < orders.Count; i++)
            {
                OrderHeader order = orders[i];
                Database.GetLineData(ref order, "NAV INV ORDER LINE");
                Database.GetOriginalPoData(ref order);

                order.Credentials = Database.GetCredentials(order.SellToCustomerNo);
                if (order.Credentials != null && order.Lines.Count > 0 && order.Credentials.OrderConfirm == 1)
                {
                    try
                    {
                        BuildXml build = new OrderConfirmation(order, "accept");
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
                        Email.SendErrorMessage(ex, "NAV_Inventory_Order_Confirmation", "GetReady", null);
                    }
                }

                AutoResponder responder = new AutoResponder(auto.Header, auto.Footer, auto.Style, auto.Subject, auto.BodyStyle, auto.Body, auto.Cart, auto.ExtraChargesFormat);
                responder.Subject = responder.Subject.Replace("[OrderNo]", order.SalesOrderNo)
                                .Replace("[Customer PO No]", order.CustomerPONo);

                string bpaCall = "";
                if (order.BPA.Length > 0) bpaCall = order.BPA;
                if (order.CallNo.Length > 0) bpaCall = bpaCall.Length > 0 ? bpaCall + " / " + order.CallNo : order.CallNo;

                responder.Body = responder.Body.Replace("[Contact Name]", order.ContactName)
                    .Replace("[Order No]", order.SalesOrderNo)
                    .Replace("[OrderDate]", order.OrderDate.ToShortDateString())
                    .Replace("[Customer PO No]", order.CustomerPONo)
                    .Replace("[BPACallNo]", bpaCall)
                    .Replace("[TrackingNo]", order.TrackingNo)
                    .Replace("[Comments]", "")
                    .Replace("[ContactPhone]", order.ContactPhone)
                    .Replace("[ContactFax]", order.ContactFax)
                    .Replace("[ContactEmail]", order.ContactEmail)
                    .Replace("[PayTerms]", order.PaymentTermsCode)
                    .Replace("[BillToName]", order.BillToAddress.Name)
                    .Replace("[BillToContact]", order.BillToAddress.Contact)
                    .Replace("[BillToAddress1]", " " + order.BillToAddress.Street)
                    .Replace("[BillToAddress2]", " " + order.BillToAddress.Street2)
                    .Replace("[BillToAddress3]", " ")
                    .Replace("[BillToCity]", order.BillToAddress.City)
                    .Replace("[BillToState]", order.BillToAddress.State)
                    .Replace("[BillToZip]", order.BillToAddress.PostCode)
                    .Replace("[ShipToName]", order.ShipToAddress.Name)
                    .Replace("[ShipToContact]", order.ShipToAddress.Contact)
                    .Replace("[ShipToAddress1]", order.ShipToAddress.Street)
                    .Replace("[ShipToAddress2]", " " + order.ShipToAddress.Street2)
                    .Replace("[ShipToAddress3]", " ")
                    .Replace("[ShipToCity]", order.ShipToAddress.City)
                    .Replace("[ShipToState]", order.ShipToAddress.State)
                    .Replace("[ShipToZip]", order.ShipToAddress.PostCode);

                responder.EmailTo = order.ContactEmail;

                responder.EmailCc = Database.GetAdditionalEmails("GET 2ND EMAIL", order.SellToCustomerNo, order.ShipToAddress.Contact);
                string emails = Database.GetAdditionalEmails("OrderConfirmation", order.SellToCustomerNo);
                if (emails.Length > 0)
                    responder.EmailCc += ";" + emails;

                SellToContact sellToContact = Database.GetTeamEmail(order.SellToCustomerNo);

                responder.Body = responder.Body.Replace("[SalespersonName]", sellToContact != null && sellToContact.SalespersonName.Length > 0 ? sellToContact.SalespersonName : order.SalespersonName)
                    .Replace("[SalespersonEmailAddress]", sellToContact != null && sellToContact.SalespersonEmail.Length > 0 ? sellToContact.SalespersonEmail : order.SalespersonEmail)
                    .Replace("[SalespersonPhoneNumber]", sellToContact != null && sellToContact.SalespersonPhone.Length > 0 ? sellToContact.SalespersonPhone : order.SalespersonPhone)
                    .Replace("[SalesTeamEmail]", sellToContact != null && sellToContact.TeamEmail.Length > 0 ? sellToContact.TeamEmail : order.SalesTeamEmail)
                    .Replace("[SalesTeamPhoneNumber]", order.SalesTeamPhone);

                responder.EmailBcc = sellToContact != null && sellToContact.TeamEmail.Length > 0 ? sellToContact.TeamEmail : order.SalespersonEmail;

                string cart = "";
                decimal subTotal = 0.0M, shippingTotal = 0.0M, taxTotal = 0.0M;

                foreach (OrderLine line in order.Lines)
                {
                    string estdate = "";
                    if (line.RequestedDeliveryDate != new DateTime(1753, 1, 1))
                        estdate = line.RequestedDeliveryDate.ToShortDateString();
                    else
                        estdate = "";

                    cart += responder.Cart.Replace("[SupplierPartNo]", line.VendorName + "<br>" + line.VendorItemNo)
                        .Replace("[Description]", line.Description)
                        .Replace("[PriceUOM]", line.UnitPrice.ToString("C") + "<br>" + line.UOM)
                        .Replace("[Quantity]", line.Qty.ToString("G29"))
                        .Replace("[EstimatedDeliveryDate]", line.RequestedDeliveryDate == new DateTime(1753, 01, 01) ? "TBD" : line.RequestedDeliveryDate.ToShortDateString())
                        .Replace("[ExtPrice]", line.ExtendedPrice.ToString("C"));

                    subTotal += line.ExtendedPrice;
                    taxTotal += line.TotalTax;
                }

                string extraCharges = "";
                foreach (ExtraCharge extra in order.ExtraCharges)
                {
                    extraCharges += auto.ExtraChargesFormat.Replace("[Name]", extra.Name)
                        .Replace("[Charge]", extra.Charge.ToString("C"));
                    shippingTotal += extra.Charge;
                }

                responder.Body = responder.Body.Replace("[Subtotal]", subTotal.ToString("C"))
                                .Replace("[ExtraCharges]", extraCharges)
                                .Replace("[tax]", taxTotal.ToString("C"))
                                .Replace("[orderTotal]", (subTotal + shippingTotal + taxTotal).ToString("C"));

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
                DateTime check = DateTime.Now.AddDays(-5);
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
                    Email.SendErrorMessage(ex, "NAV_Inventory_Order_Confirmation", "SendResponders", new SqlCommand(auto.EmailTo + ";" + auto.EmailCc + ";" + auto.EmailBcc));
                }
                Database.InsertAutoResponder(auto, body, "Order Confirmation");
            }
        }
    }
}
