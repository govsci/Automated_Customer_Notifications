using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automated_Customer_Notifications.Objects;
using System.Data.SqlClient;
using System.Data;


namespace Automated_Customer_Notifications.Classes
{
    public static class Database
    {
        public static AutoResponder GetResponder(string id)
        {
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection edb = new SqlConnection(Constants.connectionEcommerce))
                {
                    edb.Open();
                    cmd = new SqlCommand(Constants.EcomStoredProcedure, edb);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.Parameters.Add(new SqlParameter("@autoType", "GET RESPONDER BY ID"));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read()) { }
                        rs.NextResult();

                        if (rs.Read())
                        {
                            return new AutoResponder(GetValue(rs, "emailHeader")
                            , GetValue(rs, "emailFooter")
                            , GetValue(rs, "emailStyle")
                            , GetValue(rs, "subject")
                            , GetValue(rs, "style")
                            , GetValue(rs, "body")
                            , GetValue(rs, "cart")
                            , GetValue(rs, "extraChargesFormat"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetResponder", cmd);
            }

            return null;
        }

        public static List<OrderHeader> GetHeaderData(string headerMethod)
        {
            SqlCommand cmd = null;
            List<OrderHeader> orders = new List<OrderHeader>();

            try
            {
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", headerMethod));

                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            OrderHeader order = new OrderHeader();
                            if (headerMethod == "Nav Ship Notice Header")
                            {
                                order.ShipmentNo = GetValue(rs, "No_");
                                order.SalesOrderNo = GetValue(rs, "Order No_");

                                if (orders.Find(o => o.ShipmentNo == order.ShipmentNo) != null)
                                    continue;
                            }
                            else
                            {
                                order.SalesOrderNo = GetValue(rs, "No_");
                                if (orders.Find(o => o.SalesOrderNo == order.SalesOrderNo) != null)
                                    continue;
                            }

                            order.ContactName = GetValue(rs, "Contact Name");
                            order.CustomerPONo = GetValue(rs, "Your Reference");
                            if (order.CustomerPONo.Contains("/"))
                                order.CustomerPONo = order.CustomerPONo.Remove(order.CustomerPONo.IndexOf("/")).Trim();
                            else if (order.CustomerPONo.Contains("RMA"))
                                order.CustomerPONo = order.CustomerPONo.Remove(order.CustomerPONo.IndexOf("RMA")).Trim();

                            order.ContactEmail = GetValue(rs, "Contact E-Mail");
                            order.ContactPhone = GetValue(rs, "Contact Phone");
                            order.ContactFax = GetValue(rs, "Contact Fax");

                            order.SalespersonName = GetValue(rs, "SPName");
                            if (order.SalespersonName.Length == 0)
                                order.SalespersonName = GetValue(rs, "SalespersonName");

                            order.SalespersonEmail = GetValue(rs, "SPEmail");
                            if (order.SalespersonEmail.Length == 0)
                                order.SalespersonEmail = GetValue(rs, "SalespersonEmail");

                            order.SalespersonPhone = GetValue(rs, "SPPhone");
                            if (order.SalespersonPhone.Length == 0)
                                order.SalespersonPhone = GetValue(rs, "SalespersonPhone");

                            order.SalesTeamEmail = GetValue(rs, "Sales Team Email");
                            if (order.SalesTeamEmail.Length == 0)
                                order.SalesTeamEmail = GetValue(rs, "SalesTeamEmail");

                            order.SalesTeamPhone = GetValue(rs, "Sales Team Phone No_");

                            order.SellToCustomerNo = GetValue(rs, "Sell-to Customer No_");
                            order.SellToCustomerName = GetValue(rs, "Sell-to Customer Name");
                            order.LocationCode = GetValue(rs, "Location Code");
                            order.OrderDate = GetDateTimeValue(rs, "Order Date");
                            order.BPA = GetValue(rs, "BPA No");
                            order.CallNo = GetValue(rs, "Call No_");

                            order.TrackingNo = GetValue(rs, "Tracking No_");
                            if (order.TrackingNo.Length == 0)
                                order.TrackingNo = GetValue(rs, "Package Tracking No_");

                            order.TrackingCarrier = GetValue(rs, "Caption");
                            if (order.TrackingCarrier.Length == 0) order.TrackingCarrier = GetValue(rs, "Shipment Method");
                            if (order.TrackingCarrier.Length == 0) order.TrackingCarrier = "GSS";

                            order.PaymentTermsCode = GetValue(rs, "Payment Terms Code");
                            order.BillToAddress = new Address("",
                                GetValue(rs, "Bill-to Name")
                                , GetValue(rs, "Bill-to Contact")
                                , GetValue(rs, "Bill-to Address")
                                , GetValue(rs, "Bill-to Address 2")
                                , GetValue(rs, "Bill-to City")
                                , GetValue(rs, "Bill-to County")
                                , GetValue(rs, "Bill-to Post Code"));
                            order.ShipToAddress = new Address(
                                GetValue(rs, "Ship-To Code")
                                , GetValue(rs, "Ship-to Name")
                                , GetValue(rs, "Ship-to Contact")
                                , GetValue(rs, "Ship-to Address")
                                , GetValue(rs, "Ship-to Address 2")
                                , GetValue(rs, "Ship-to City")
                                , GetValue(rs, "Ship-to County")
                                , GetValue(rs, "Ship-to Post Code"));

                            order.ShipmentDate = GetDateTimeValue(rs, "Shipment Date");
                            if (order.ShipmentDate.Date < DateTime.Now.Date)
                                order.ShipmentDate = GetDateTimeValue(rs, "Posting Date");

                            if (order.ShipmentDate.Date == new DateTime(1753, 1, 1).Date)
                                order.ShipmentDate = DateTime.Now;

                            order.URL = GetValue(rs, "URL");
                            orders.Add(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetHeaderData", cmd);
            }

            return orders;
        }

        public static void GetLineData(ref OrderHeader order, string method)
        {
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", method));
                    cmd.Parameters.Add(new SqlParameter("@salesOrderNo", order.SalesOrderNo));
                    cmd.Parameters.Add(new SqlParameter("@shipmentNo", order.ShipmentNo));
                    cmd.Parameters.Add(new SqlParameter("@trackingNumber", order.TrackingNo));
                    cmd.Parameters.Add(new SqlParameter("@trackingCarrier", order.TrackingCarrier));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            string type = GetValue(rs, "Type");
                            if (GetValue(rs, "Type") == "Item" && !GetValue(rs, "No_").StartsWith("GSSTRANS"))
                            {
                                OrderLine line = new OrderLine();
                                line.GSSPartNo = GetValue(rs, "No_");
                                line.VendorNo = GetValue(rs, "Vendor No_");
                                line.VendorName = GetValue(rs, "Vendor Name");
                                if (line.VendorName.Length == 0) line.VendorName = GetValue(rs, "Name");
                                line.VendorItemNo = GetValue(rs, "Vendor Item No_");
                                line.Description = GetValue(rs, "Description");
                                line.UnitPrice = GetDecimalValue(rs, "Unit Price");
                                line.UOM = GetValue(rs, "Unit of Measure Code");
                                line.Qty = GetDecimalValue(rs, "Quantity");
                                line.RequestedDeliveryDate = GetDateTimeValue(rs, "Requested Delivery Date");
                                line.PurchaseOrderNo = GetValue(rs, "PurchaseOrderNo");
                                line.PurchaseOrderLineNo = GetIntegerValue(rs, "PurchaseOrderLineNo");
                                line.Type = GetValue(rs, "Type");
                                line.ExtendedPrice = GetDecimalValue(rs, "Extended Price");
                                line.TotalTax = GetDecimalValue(rs, "Total Tax");
                                line.LineNumber = GetIntegerValue(rs, "Line No_");
                                line.ShipmentDate = GetDateTimeValue(rs, "Shipment Date");
                                order.Lines.Add(line);
                            }
                            else if (type == "G/L Account" && method == "Nav Ship Notice Line" && order.SellToCustomerNo == "OAK RIDGE NTL LAB")
                            {
                                OrderLine line = new OrderLine();
                                line.GSSPartNo = GetValue(rs, "No_");
                                line.VendorNo = GetValue(rs, "Vendor No_");
                                line.VendorName = GetValue(rs, "Vendor Name");
                                if (line.VendorName.Length == 0) line.VendorName = GetValue(rs, "Name");
                                line.VendorItemNo = GetValue(rs, "Vendor Item No_");
                                line.Description = GetValue(rs, "Description");
                                line.UnitPrice = GetDecimalValue(rs, "Unit Price");
                                line.UOM = GetValue(rs, "Unit of Measure Code");
                                line.Qty = GetDecimalValue(rs, "Quantity");
                                line.RequestedDeliveryDate = GetDateTimeValue(rs, "Requested Delivery Date");
                                line.PurchaseOrderNo = GetValue(rs, "PurchaseOrderNo");
                                line.PurchaseOrderLineNo = GetIntegerValue(rs, "PurchaseOrderLineNo");
                                line.Type = GetValue(rs, "Type");
                                line.ExtendedPrice = GetDecimalValue(rs, "Extended Price");
                                line.TotalTax = GetDecimalValue(rs, "Total Tax");
                                line.LineNumber = GetIntegerValue(rs, "Line No_");
                                line.ShipmentDate = GetDateTimeValue(rs, "Shipment Date");
                                order.Lines.Add(line);
                            }
                            else if (GetDecimalValue(rs, "Extended Price") > 0.0M)
                                order.ExtraCharges.Add(new ExtraCharge(GetValue(rs, "Description"), GetDecimalValue(rs, "Extended Price")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetLineData", cmd);
            }
        }

        public static void GetOriginalPoData(ref OrderHeader order)
        {
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection edb = new SqlConnection(Constants.connectionEcommerce))
                {
                    edb.Open();
                    cmd = new SqlCommand(Constants.EcomStoredProcedure, edb);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@orderNumber", order.CustomerPONo));
                    cmd.Parameters.Add(new SqlParameter("@autoType", "GET ORIG ORDER"));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read()) { }
                        rs.NextResult();

                        if (rs.Read())
                        {
                            order.OrgPayloadID = rs["payloadId"].ToString();

                            string extrinsics = rs["extrinsics"].ToString();
                            if (extrinsics.Length > 0)
                            {
                                string[] rows = extrinsics.Split('|');
                                foreach (string r in rows)
                                {
                                    if (r.Length > 0)
                                    {
                                        string[] cols = r.Split(':');
                                        if (cols.Length > 1 && cols[0].Length > 0 && cols[1].Length > 0)
                                        {
                                            if (cols[0] == "AwardUniqueID")
                                            {
                                                Extrinsic extr = new Extrinsic();
                                                extr.Name = cols[0];
                                                extr.Value = cols[1];
                                                order.Extrinsics.Add(extr);
                                            }
                                        }
                                    }
                                }
                            }

                            if (order.SellToCustomerNo == "C14139")
                            {
                                Extrinsic extr2 = new Extrinsic();
                                extr2 = new Extrinsic();
                                extr2.Name = "requisitionID";
                                extr2.Value = rs["requisitionID"].ToString();
                                order.Extrinsics.Add(extr2);
                            }
                        }

                        rs.NextResult();
                        while (rs.Read())
                        {
                            foreach (OrderLine line in order.Lines.FindAll(l => l.VendorItemNo == rs["supplierPartID"].ToString()))
                            {
                                line.ManufacturerPartID = rs["manufacturerPartID"].ToString();
                                line.OriginalUom = rs["originalUOM"].ToString();
                                try { line.OriginalLineNumber = int.Parse(rs["lineNumber"].ToString()); }
                                catch { line.OriginalLineNumber = 0; }
                            }

                            foreach (OrderLine line in order.Lines.FindAll(l => l.GSSPartNo == rs["supplierPartID"].ToString()))
                            {
                                line.ManufacturerPartID = rs["manufacturerPartID"].ToString();
                                line.OriginalUom = rs["originalUOM"].ToString();
                                try { line.OriginalLineNumber = int.Parse(rs["lineNumber"].ToString()); }
                                catch { line.OriginalLineNumber = 0; }
                            }

                            foreach (OrderLine line in order.Lines.FindAll(l => l.GSSPartNo == rs["gssPartNumber"].ToString()))
                            {
                                line.ManufacturerPartID = rs["manufacturerPartID"].ToString();
                                line.OriginalUom = rs["originalUOM"].ToString();
                                try { line.OriginalLineNumber = int.Parse(rs["lineNumber"].ToString()); }
                                catch { line.OriginalLineNumber = 0; }
                            }

                            foreach (OrderLine line in order.Lines.FindAll(l => l.Type == "G/L Account" && rs["supplierPartID"].ToString().ToUpper().StartsWith("GSSTRANS")))
                            {
                                line.ManufacturerPartID = rs["manufacturerPartID"].ToString();
                                line.OriginalUom = rs["originalUOM"].ToString();
                                try { line.OriginalLineNumber = int.Parse(rs["lineNumber"].ToString()); }
                                catch { line.OriginalLineNumber = 0; }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetOriginalPoData", cmd);
            }
        }

        public static List<OrderHeader> GetInlineData(string method)
        {
            List<OrderHeader> orders = new List<OrderHeader>();
            SqlCommand cmd = null;

            try
            {
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", method));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        OrderHeader order = null;
                        while (rs.Read())
                        {
                            if (order == null || order.SalesOrderNo != GetValue(rs, "Order No_"))
                            {
                                if (order != null)
                                    orders.Add(order);

                                order = new OrderHeader();
                                order.SalesOrderNo = GetValue(rs, "Order No_");

                                order.CustomerPONo = GetValue(rs, "PO No_");
                                if (order.CustomerPONo.Contains("/"))
                                    order.CustomerPONo = order.CustomerPONo.Remove(order.CustomerPONo.IndexOf("/")).Trim();
                                else if (order.CustomerPONo.Contains("RMA"))
                                    order.CustomerPONo = order.CustomerPONo.Remove(order.CustomerPONo.IndexOf("RMA")).Trim();

                                order.ContactEmail = GetValue(rs, "Email");
                                order.SellToCustomerNo = GetValue(rs, "Sell-to Customer No_");
                                order.ShipToAddress = new Address("", "", GetValue(rs, "Ship-to Contact"), "", "", "", "", "");
                                order.ContactName = GetValue(rs, "Contact Name");
                                order.SalespersonName = GetValue(rs, "SPName");
                                order.SalespersonEmail = GetValue(rs, "SPEmail");
                                order.SalespersonPhone = GetValue(rs, "SPPhone");
                                order.SalespersonName = GetValue(rs, "SPName");
                                order.SalesTeamEmail = GetValue(rs, "STEmail");
                                order.SalesTeamPhone = GetValue(rs, "STPhone");
                                order.LocationCode = GetValue(rs, "Location Code");
                            }

                            OrderLine line = new OrderLine();
                            line.VendorNo = GetValue(rs, "Vendor No_");
                            line.VendorName = GetValue(rs, "Vendor Name");
                            line.VendorItemNo = GetValue(rs, "Vendor Item No_");
                            line.Description = GetValue(rs, "Description");
                            line.UOM = GetValue(rs, "Unit of Measure Code");
                            line.Qty = GetDecimalValue(rs, "Back Ordered Qty");
                            line.BackOrderLine = true;
                            line.BackOrderDate = GetDateTimeValue(rs, "Back Order Date");
                            line.RequestedDeliveryDate = GetDateTimeValue(rs, "Requested Delivery Date");
                            line.GSSPartNo = GetValue(rs, "No_");
                            line.PurchaseOrderLineNo = GetIntegerValue(rs, "purchOrderLineNo");
                            line.PurchaseOrderNo = GetValue(rs, "purchOrderNo");
                            line.PurchasingCode = GetValue(rs, "Purchasing Code");
                            line.LineNumber = GetIntegerValue(rs, "Line No_");
                            order.Lines.Add(line);
                        }

                        if (order != null)
                            orders.Add(order);
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetInlineData", cmd);
            }

            return orders;
        }

        public static List<AutoResponder> GetAutoResponders(string method)
        {
            List<AutoResponder> autoResponders = new List<AutoResponder>();

            SqlCommand cmd = null;
            try
            {
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", method));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            AutoResponder auto = new AutoResponder("", "", "", GetValue(rs, "subject"), "", GetValue(rs, "body"), "", "");
                            auto.ID = GetValue(rs, "id");
                            auto.EmailTo = GetValue(rs, "emailTo");
                            auto.EmailCc = GetValue(rs, "emailCc");
                            auto.EmailBcc = GetValue(rs, "emailBcc");

                            auto.Order = new OrderHeader();
                            auto.Order.SalesOrderNo = GetValue(rs, "salesOrderNo");
                            auto.Order.CustomerPONo = GetValue(rs, "customerPONo");
                            auto.Order.ShipmentNo = GetValue(rs, "shipmentNo");
                            auto.Order.TrackingNo = GetValue(rs, "trackingNumber");
                            auto.Order.TrackingCarrier = GetValue(rs, "trackingCarrier");
                            auto.Order.ShipmentDate = GetDateTimeValue(rs, "shipmentDate");

                            autoResponders.Add(auto);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetAutoResponders", cmd);
            }

            return autoResponders;
        }

        public static void InsertAutoResponder(AutoResponder auto, string body, string docType)
        {
            SqlCommand cmd = null;
            try
            {
                int id = 0;
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", "Insert AutoResp"));
                    cmd.Parameters.Add(new SqlParameter("@documentType", docType));
                    cmd.Parameters.Add(new SqlParameter("@salesOrderNo", auto.Order.SalesOrderNo));
                    cmd.Parameters.Add(new SqlParameter("@customerPONo", auto.Order.CustomerPONo));
                    cmd.Parameters.Add(new SqlParameter("@emailTo", auto.EmailTo));
                    cmd.Parameters.Add(new SqlParameter("@emailCc", auto.EmailCc));
                    cmd.Parameters.Add(new SqlParameter("@emailBcc", auto.EmailBcc));
                    cmd.Parameters.Add(new SqlParameter("@subject", auto.Subject));
                    cmd.Parameters.Add(new SqlParameter("@body", body));
                    cmd.Parameters.Add(new SqlParameter("@cxmlSent", auto.Order.CxmlSent ? "1" : "0"));
                    cmd.Parameters.Add(new SqlParameter("@cxmlDate", auto.Order.CxmlDateStamp));
                    cmd.Parameters.Add(new SqlParameter("@cxml", auto.Order.CxmlFile));

                    if (docType == "Shipment Notification")
                    {
                        cmd.Parameters.Add(new SqlParameter("@trackingNumber", auto.Order.TrackingNo));
                        cmd.Parameters.Add(new SqlParameter("@trackingCarrier", auto.Order.TrackingCarrier));
                        cmd.Parameters.Add(new SqlParameter("@shipmentDate", auto.Order.ShipmentDate));
                        cmd.Parameters.Add(new SqlParameter("@shipmentNo", auto.Order.ShipmentNo));
                    }

                    using (SqlDataReader rs = cmd.ExecuteReader())
                        if (rs.Read())
                            id = rs.GetInt32(0);

                    if (id > 0)
                    {
                        foreach (OrderLine line in auto.Order.Lines)
                        {
                            cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@method", "Insert AutoResp Line"));
                            cmd.Parameters.Add(new SqlParameter("@parent", id));
                            cmd.Parameters.Add(new SqlParameter("@gssPartNo", line.GSSPartNo));
                            cmd.Parameters.Add(new SqlParameter("@vendorNo", line.VendorNo));
                            cmd.Parameters.Add(new SqlParameter("@uom", line.UOM));
                            cmd.Parameters.Add(new SqlParameter("@qty", line.Qty));
                            cmd.Parameters.Add(new SqlParameter("@purchCode", line.PurchasingCode));
                            cmd.Parameters.Add(new SqlParameter("@estDeliveryDate", docType == "Shipment Notification" ? auto.Order.EstDelDate : line.RequestedDeliveryDate));
                            cmd.Parameters.Add(new SqlParameter("@lineNo", line.LineNumber));

                            if (docType == "Order Confirmation")
                            {
                                cmd.Parameters.Add(new SqlParameter("@purchaseOrderNo", line.PurchaseOrderNo));
                                cmd.Parameters.Add(new SqlParameter("@purchaseOrderLineNo", line.PurchaseOrderLineNo));
                            }
                            else if (docType == "Estimated Delivery Date Change")
                            {
                                cmd.Parameters.Add(new SqlParameter("@backOrderDate", line.BackOrderDate));
                                cmd.Parameters.Add(new SqlParameter("@purchaseOrderNo", line.PurchaseOrderNo));
                                cmd.Parameters.Add(new SqlParameter("@purchaseOrderLineNo", line.PurchaseOrderLineNo));
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.InsertAutoResponder", cmd);
            }
        }

        public static void InsertAutoResponder(AutoResponder auto, string body, string docType, string otherParent)
        {
            SqlCommand cmd = null;
            try
            {
                int id = 0;
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", "Insert AutoResp"));
                    cmd.Parameters.Add(new SqlParameter("@documentType", docType));
                    cmd.Parameters.Add(new SqlParameter("@salesOrderNo", auto.Order.SalesOrderNo));
                    cmd.Parameters.Add(new SqlParameter("@customerPONo", auto.Order.CustomerPONo));
                    cmd.Parameters.Add(new SqlParameter("@emailTo", auto.EmailTo));
                    cmd.Parameters.Add(new SqlParameter("@emailCc", auto.EmailCc));
                    cmd.Parameters.Add(new SqlParameter("@emailBcc", auto.EmailBcc));
                    cmd.Parameters.Add(new SqlParameter("@subject", auto.Subject));
                    cmd.Parameters.Add(new SqlParameter("@body", body));
                    cmd.Parameters.Add(new SqlParameter("@trackingNumber", auto.Order.TrackingNo));
                    cmd.Parameters.Add(new SqlParameter("@trackingCarrier", auto.Order.TrackingCarrier));
                    cmd.Parameters.Add(new SqlParameter("@shipmentDate", auto.Order.ShipmentDate));

                    using (SqlDataReader rs = cmd.ExecuteReader())
                        if (rs.Read())
                            id = rs.GetInt32(0);

                    if (id > 0)
                    {
                        foreach (OrderLine line in auto.Order.Lines)
                        {
                            cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@method", "Insert AutoResp Line"));
                            cmd.Parameters.Add(new SqlParameter("@parent", id));
                            cmd.Parameters.Add(new SqlParameter("@otherParent", otherParent));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.InsertAutoResponder", cmd);
            }
        }

        public static string GetAdditionalEmails(string name, string sellTo)
        {
            string emails = "";
            SqlCommand cmd = null;

            try
            {
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", "Get Emails"));
                    cmd.Parameters.Add(new SqlParameter("@documentType", name));
                    cmd.Parameters.Add(new SqlParameter("@sellTo", sellTo));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                        while (rs.Read())
                            emails += emails.Length > 0 ? ";" + rs["E-Mail Address"].ToString() : rs["E-Mail Address"].ToString();
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetAdditionalEmails(string, string)", cmd);
            }

            return emails;
        }

        public static string GetAdditionalEmails(string method, string sellTo, string name1)
        {
            SqlCommand cmd = null;
            try
            {
                if (sellTo == "RICHLAND")
                    return "";

                string contact = "";
                string contactName = "", name2 = "", name3 = "";

                if (method == "GET 2ND SHIP EMAIL")
                {
                    string[] ships = name1.Split('/');

                    if (ships.Length > 1)
                        contactName = ships[1].Trim();
                    else
                        contactName = ships[0];

                    if (sellTo.Equals("BERKELEY"))
                    {
                        string[] shipscomma = name1.Replace("eBuyer", "").Trim().Split(',');
                        if (shipscomma.Length == 2)
                            contactName = shipscomma[1].Trim() + " " + shipscomma[0];
                    }
                }
                else if (method == "GET 2ND EMAIL")
                {
                    string[] c = name1.Split('/');
                    if (name1.Length > 0 && c.Length > 1)
                    {
                        if (c[1].Contains(','))
                        {
                            name2 = c[1].Split(',')[0].Trim();
                            name3 = c[1].Split(',')[1].Trim();
                        }
                        else
                            contactName = c[1].Trim();
                    }
                }

                if ((contactName.Length == 0 && name2.Length == 0 && name3.Length == 0) || contactName.Length < 5 || !contactName.Trim().Contains(" "))
                    return "";


                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand("[dbo].[Navision.ScheduledTasks.Automated.Control]", dbcon);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@documentNo", sellTo));
                    cmd.Parameters.Add(new SqlParameter("@name1", contactName));
                    cmd.Parameters.Add(new SqlParameter("@name2", name2));
                    cmd.Parameters.Add(new SqlParameter("@name3", name3));
                    cmd.Parameters.Add(new SqlParameter("@method", method));

                    using (SqlDataReader rs = cmd.ExecuteReader())
                        while (rs.Read())
                            contact += contact.Length > 0 ? ";" + rs["Contact E-Mail"].ToString() : rs["Contact E-Mail"].ToString();
                }

                return contact;
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetAdditionalEmails(string, string, string)", cmd);
            }

            return "";
        }

        public static SellToContact GetTeamEmail(string sellTo)
        {
            SqlCommand cmd = null;
            try
            {
                string query = "[dbo].[Ecommerce.AutoResponder.New.Control]";
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionEcommerce))
                {
                    dbcon.Open();
                    cmd = new SqlCommand(query, dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@autoType", "GET TEAM EMAIL"));
                    cmd.Parameters.Add(new SqlParameter("@userID", sellTo));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        rs.Read();
                        rs.NextResult();

                        if (rs.Read())
                            return new SellToContact(rs["teamEmail"].ToString()
                                , rs["salespersonName"].ToString()
                                , rs["salespersonEmail"].ToString()
                                , rs["salespersonPhone"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetTeamEmail", cmd);
            }

            return null;
        }

        public static XmlCredentials GetCredentials(string sellTo)
        {
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection edb = new SqlConnection(Constants.connectionNavision))
                {
                    edb.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, edb);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@sellTo", sellTo));
                    cmd.Parameters.Add(new SqlParameter("@method", "GET CREDENTIALS"));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            XmlCredentials xml = new XmlCredentials();
                            xml.FromDomain_1 = rs["fromDomain1"].ToString();
                            xml.FromIdentity_1 = rs["fromIdentity1"].ToString();
                            xml.FromType_1 = rs["fromType1"].ToString();
                            xml.FromDomain_2 = rs["fromDomain2"].ToString();
                            xml.FromIdentity_2 = rs["fromIdentity2"].ToString();
                            xml.FromType_2 = rs["fromType2"].ToString();
                            xml.FromDomain_3 = rs["fromDomain3"].ToString();
                            xml.FromIdentity_3 = rs["fromIdentity3"].ToString();
                            xml.ToDomain = rs["toDomain"].ToString();
                            xml.ToIdentity = rs["toIdentity"].ToString();
                            xml.ToDomain_2 = rs["toDomain2"].ToString();
                            xml.ToIdentity_2 = rs["toIdentity2"].ToString();
                            xml.SenderDomain_1 = rs["senderDomain1"].ToString();
                            xml.SenderIdentity_1 = rs["senderIdentity1"].ToString();
                            xml.SenderSharedSecret_1 = rs["sharedSecret1"].ToString();
                            xml.SenderDomain_2 = rs["senderDomain2"].ToString();
                            xml.SenderIdentity_2 = rs["senderIdentity2"].ToString();
                            xml.SenderSharedSecret_2 = rs["sharedSecret2"].ToString();
                            xml.UserAgent = rs["userAgent"].ToString();
                            xml.URL = rs["url"].ToString();
                            xml.SystemID = rs["systemId"].ToString();
                            xml.cXmlVersion = rs["cxmlVersion"].ToString();
                            xml.TimestampFormat = rs["timestampFormat"].ToString();
                            xml.PayloadIdFormat = rs["payloadIdFormat"].ToString();
                            xml.TlsVersion = rs["tlsVersion"].ToString();
                            xml.LineNumber = rs["lineNumber"].ToString();
                            xml.DeliveryDate = int.Parse(rs["deliveryDate"].ToString());
                            xml.ShipmentDate = int.Parse(rs["shipmentDate"].ToString());
                            xml.ASN = int.Parse(rs["asn"].ToString());
                            xml.OrderConfirm = int.Parse(rs["orderConfirm"].ToString());
                            return xml;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetResponder", cmd);
            }

            return null;
        }

        public static void UpdateDeliveryCheck(string salesOrderNo, string customerPONo)
        {
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionNavision))
                {
                    dbcon.Open();
                    cmd = new SqlCommand("UPDATE [dbo].[Ecommerce$Automated Customer Notifications Header] "
                        + $"SET [deliveryNotice] = 1 WHERE [salesOrderNo] = '{salesOrderNo}' AND [customerPONo] = '{customerPONo}'", dbcon);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.UpdateDeliveryCheck", cmd);
            }
        }

        private static string GetValue(SqlDataReader rs, string column)
        {
            try { return rs[column].ToString(); }
            catch { return ""; }
        }

        private static DateTime GetDateTimeValue(SqlDataReader rs, string column)
        {
            try { return DateTime.Parse(rs[column].ToString()); }
            catch { return new DateTime(1753, 1, 1); }
        }

        private static decimal GetDecimalValue(SqlDataReader rs, string column)
        {
            try { return decimal.Parse(rs[column].ToString()); }
            catch { return 0.0M; }
        }

        private static int GetIntegerValue(SqlDataReader rs, string column)
        {
            try { return int.Parse(rs[column].ToString()); }
            catch { return 0; }
        }

        public static List<Cxml_Notice> GetCxmlNoticesNotSent()
        {
            List<Cxml_Notice> notices = new List<Cxml_Notice>();
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection edb = new SqlConnection(Constants.connectionNavision))
                {
                    edb.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, edb);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", "GET UNSENTCXML"));
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            notices.Add(new Cxml_Notice(
                                int.Parse(rs["id"].ToString()),
                                rs["documentType"].ToString(),
                                rs["salesOrderNo"].ToString(),
                                rs["customerPONo"].ToString(),
                                rs["shipmentNo"].ToString(),
                                rs["cxml"].ToString(),
                                rs["url"].ToString()
                                ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetResponder", cmd);
            }

            return notices;
        }

        public static void UpdateCxmlNotice(int id, DateTime cxmlSent)
        {
            SqlCommand cmd = null;
            try
            {
                using (SqlConnection edb = new SqlConnection(Constants.connectionNavision))
                {
                    edb.Open();
                    cmd = new SqlCommand(Constants.NavStoredProcedure, edb);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@method", "UPDATE UNSENTCXML"));
                    cmd.Parameters.Add(new SqlParameter("@parent", id));
                    cmd.Parameters.Add(new SqlParameter("@cxmlDate", cxmlSent));
                    cmd.Parameters.Add(new SqlParameter("@cxmlSent", 1));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Automated_Customer_Notifications", "Database.GetResponder", cmd);
            }
        }
    }
}
