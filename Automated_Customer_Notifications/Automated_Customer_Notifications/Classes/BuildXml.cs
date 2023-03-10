using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Automated_Customer_Notifications.Objects;
using System.Web;

namespace Automated_Customer_Notifications.Classes
{
    public abstract class BuildXml
    {
        public abstract XmlDocument XML { get; set; }
        public abstract int LineCount { get; set; }

        protected void CreateXml(OrderHeader order)
        {
            XML = new XmlDocument();
            XML.XmlResolver = null;

            if (order.Credentials.SystemID.Length > 0)
            {
                XmlDeclaration xmlDeclare = XML.CreateXmlDeclaration("1.0", "UTF-8", null);
                XML.AppendChild(xmlDeclare);

                XmlDocumentType docType = XML.CreateDocumentType("cXML", null, order.Credentials.SystemID, null);
                XML.AppendChild(docType);
            }

            XmlElement cxml = XML.CreateElement("cXML");
            if (order.Credentials.cXmlVersion.Length > 0)
                cxml.SetAttribute("version", order.Credentials.cXmlVersion);

            cxml.SetAttribute("timestamp", DateTime.Now.ToString(order.Credentials.TimestampFormat));
            cxml.SetAttribute("payloadID", DateTime.Now.ToString(order.Credentials.PayloadIdFormat.Replace("[orderID]", order.CustomerPONo)));

            if (order.Credentials.FromIdentity_1 != "AN01000041357-T" && order.Credentials.FromIdentity_1 != "AN01000041357")
                cxml.SetAttribute("xml:lang", "en-US");

            XML.AppendChild(cxml);
        }

        protected void CreateHeader(OrderHeader order)
        {
            XmlElement header = XML.CreateElement("Header");

            //From
            XmlElement from = XML.CreateElement("From");

            XmlElement fromCredential_1 = XML.CreateElement("Credential");
            fromCredential_1.SetAttribute("domain", order.Credentials.FromDomain_1);
            if (order.Credentials.FromType_1.Length > 0)
                fromCredential_1.SetAttribute("type", order.Credentials.FromType_1);

            XmlElement fromIdentity_1 = XML.CreateElement("Identity");
            fromIdentity_1.AppendChild(XML.CreateTextNode(order.Credentials.FromIdentity_1));
            fromCredential_1.AppendChild(fromIdentity_1);

            from.AppendChild(fromCredential_1);

            if (order.Credentials.FromDomain_2.Length > 0 || order.Credentials.FromIdentity_2.Length > 0)
            {
                XmlElement fromCredential_2 = XML.CreateElement("Credential");
                fromCredential_2.SetAttribute("domain", order.Credentials.FromDomain_2);
                if (order.Credentials.FromType_2.Length > 0)
                    fromCredential_2.SetAttribute("type", order.Credentials.FromType_2);

                XmlElement fromIdentity_2 = XML.CreateElement("Identity");
                fromIdentity_2.AppendChild(XML.CreateTextNode(order.Credentials.FromIdentity_2));
                fromCredential_2.AppendChild(fromIdentity_2);

                from.AppendChild(fromCredential_2);
            }

            if (order.Credentials.FromDomain_3.Length > 0 || order.Credentials.FromIdentity_3.Length > 0)
            {
                XmlElement fromCredential_3 = XML.CreateElement("Credential");
                fromCredential_3.SetAttribute("domain", order.Credentials.FromDomain_3);

                XmlElement fromIdentity_3 = XML.CreateElement("Identity");
                fromIdentity_3.AppendChild(XML.CreateTextNode(order.Credentials.FromIdentity_3));
                fromCredential_3.AppendChild(fromIdentity_3);

                from.AppendChild(fromCredential_3);
            }

            header.AppendChild(from);

            //To
            XmlElement to = XML.CreateElement("To");

            XmlElement toCredential = XML.CreateElement("Credential");
            toCredential.SetAttribute("domain", order.Credentials.ToDomain);

            XmlElement toIdentity = XML.CreateElement("Identity");
            toIdentity.AppendChild(XML.CreateTextNode(order.Credentials.ToIdentity));

            toCredential.AppendChild(toIdentity);
            to.AppendChild(toCredential);

            if (order.Credentials.ToDomain_2.Length > 0 || order.Credentials.ToIdentity_2.Length > 0)
            {
                XmlElement toCredential_2 = XML.CreateElement("Credential");
                toCredential_2.SetAttribute("domain", order.Credentials.ToDomain_2);

                XmlElement toIdentity_2 = XML.CreateElement("Identity");
                toIdentity_2.AppendChild(XML.CreateTextNode(order.Credentials.ToIdentity_2));
                toCredential_2.AppendChild(toIdentity_2);

                to.AppendChild(toCredential_2);
            }

            header.AppendChild(to);

            //Sender
            XmlElement sender = XML.CreateElement("Sender");

            XmlElement senderCredential_1 = XML.CreateElement("Credential");
            senderCredential_1.SetAttribute("domain", order.Credentials.SenderDomain_1);

            XmlElement senderIdentity_1 = XML.CreateElement("Identity");
            senderIdentity_1.AppendChild(XML.CreateTextNode(order.Credentials.SenderIdentity_1));
            senderCredential_1.AppendChild(senderIdentity_1);

            if (order.Credentials.SenderSharedSecret_1.Length > 0)
            {
                XmlElement sharedSecret_1 = XML.CreateElement("SharedSecret");
                sharedSecret_1.AppendChild(XML.CreateTextNode(order.Credentials.SenderSharedSecret_1));
                senderCredential_1.AppendChild(sharedSecret_1);
            }

            sender.AppendChild(senderCredential_1);

            if (order.Credentials.SenderDomain_2.Length > 0 || order.Credentials.SenderIdentity_2.Length > 0)
            {
                XmlElement senderCredential_2 = XML.CreateElement("Credential");
                senderCredential_2.SetAttribute("domain", order.Credentials.SenderDomain_2);

                XmlElement senderIdentity_2 = XML.CreateElement("Identity");
                senderIdentity_2.AppendChild(XML.CreateTextNode(order.Credentials.SenderIdentity_2));
                senderCredential_2.AppendChild(senderIdentity_2);

                if (order.Credentials.SenderSharedSecret_2.Length > 0)
                {
                    XmlElement sharedSecret = XML.CreateElement("SharedSecret");
                    sharedSecret.AppendChild(XML.CreateTextNode(order.Credentials.SenderSharedSecret_2));
                    senderCredential_2.AppendChild(sharedSecret);
                }

                sender.AppendChild(senderCredential_2);
            }

            if (order.Credentials.UserAgent.Length > 0)
            {
                XmlElement userAgent = XML.CreateElement("UserAgent");
                userAgent.AppendChild(XML.CreateTextNode(order.Credentials.UserAgent));
                sender.AppendChild(userAgent);
            }

            header.AppendChild(sender);
            XML.SelectSingleNode("cXML").AppendChild(header);
        }


        protected XmlElement CreateOrderReference(OrderHeader order)
        {
            XmlElement orderReference = XML.CreateElement("OrderReference");
            orderReference.SetAttribute("orderID", order.CustomerPONo);

            XmlElement documentReference = XML.CreateElement("DocumentReference");
            documentReference.SetAttribute("payloadID", order.OrgPayloadID);

            orderReference.AppendChild(documentReference);

            return orderReference;
        }
    }

    public class OrderConfirmation : BuildXml
    {
        public override XmlDocument XML { get; set; }
        public override int LineCount { get; set; }

        public OrderConfirmation(OrderHeader order, string type)
        {
            LineCount = 0;
            CreateXml(order);
            CreateHeader(order);
            CreateOrderConfirmation(order, type);
        }

        private void CreateOrderConfirmation(OrderHeader order, string type)
        {
            XmlElement request = XML.CreateElement("Request");
            request.SetAttribute("deploymentMode", "production");

            XmlElement confirmationRequest = XML.CreateElement("ConfirmationRequest");
            confirmationRequest.AppendChild(CreateConfirmationHeader(order, type));
            confirmationRequest.AppendChild(CreateOrderReference(order));

            foreach (OrderLine line in order.Lines)
            {
                XmlElement item = CreateConfirmationItem(order, line);
                if (item != null)
                {
                    confirmationRequest.AppendChild(item);
                    LineCount++;
                }
            }

            request.AppendChild(confirmationRequest);
            XML.SelectSingleNode("cXML").AppendChild(request);
        }

        private XmlElement CreateConfirmationHeader(OrderHeader order, string type)
        {
            XmlElement confirmationHeader = XML.CreateElement("ConfirmationHeader");
            confirmationHeader.SetAttribute("type", type);
            confirmationHeader.SetAttribute("confirmID", order.SalesOrderNo);
            confirmationHeader.SetAttribute("noticeDate", DateTime.Now.ToString(order.Credentials.TimestampFormat));

            confirmationHeader.AppendChild(CreateShippingElement(order));

            foreach (Extrinsic extr in order.Extrinsics)
            {
                XmlElement extrinsic = XML.CreateElement("Extrinsic");
                extrinsic.SetAttribute("name", extr.Name);
                extrinsic.AppendChild(XML.CreateTextNode(extr.Value));
                confirmationHeader.AppendChild(extrinsic);
            }

            return confirmationHeader;
        }

        private XmlElement CreateShippingElement(OrderHeader order)
        {
            XmlElement shipping = XML.CreateElement("Shipping");
            shipping.SetAttribute("trackingDomain", order.TrackingCarrier);
            shipping.SetAttribute("trackingId", order.TrackingNo);
            shipping.SetAttribute("tracking", "");

            XmlElement shippingMoney = XML.CreateElement("Money");
            shippingMoney.SetAttribute("currency", "USD");
            shipping.AppendChild(shippingMoney);

            XmlElement shippingDescription = XML.CreateElement("Description");
            shippingDescription.SetAttribute("xml:lang", "en-US");
            shipping.AppendChild(shippingDescription);

            return shipping;
        }

        private XmlElement CreateConfirmationItem(OrderHeader order, OrderLine line)
        {
            decimal qty = line.Qty;

            if (qty > 0.0M)
            {
                int tempLineNumber = line.OriginalLineNumber;
                if(tempLineNumber == 0)
                    tempLineNumber = line.LineNumber / 10000;

                string lineNumber = "";
                switch (order.Credentials.LineNumber)
                {
                    case "partNumber":
                        lineNumber = line.ManufacturerPartID;
                        break;
                    case "lineNumber":
                        lineNumber = tempLineNumber.ToString();
                        break;
                }

                if (lineNumber.Length == 0)
                    throw new Exception($"{order.SalesOrderNo}, {order.CustomerPONo} Original Manufacturer Part ID is required!");

                XmlElement confirmationItem = XML.CreateElement("ConfirmationItem");
                confirmationItem.SetAttribute("lineNumber", lineNumber);
                confirmationItem.SetAttribute("quantity", qty.ToString("G29"));

                XmlElement uom = XML.CreateElement("UnitOfMeasure");
                if (line.OriginalUom.Length > 0)
                    uom.AppendChild(XML.CreateTextNode(line.OriginalUom));
                else
                    uom.AppendChild(XML.CreateTextNode(line.UOM));
                confirmationItem.AppendChild(uom);

                if (line.BackOrderLine)
                {
                    XmlElement confirmationStatus_2 = XML.CreateElement("ConfirmationStatus");
                    confirmationStatus_2.SetAttribute("quantity", qty.ToString("G29"));
                    confirmationStatus_2.SetAttribute("type", "backordered");
                    string backOrderDate = "";
                    if (line.BackOrderDate != new DateTime(1753, 1, 1) && line.BackOrderDate.Date != DateTime.Now.Date)
                        backOrderDate = line.BackOrderDate.ToString(order.Credentials.TimestampFormat);
                    else
                        backOrderDate = "";

                    if (order.Credentials.ShipmentDate == 1 && backOrderDate.Length > 0)
                        confirmationStatus_2.SetAttribute("shipmentDate", backOrderDate);

                    if (order.Credentials.DeliveryDate == 1 && backOrderDate.Length > 0)
                        confirmationStatus_2.SetAttribute("deliveryDate", backOrderDate);

                    XmlElement uom_3 = XML.CreateElement("UnitOfMeasure");
                    if (line.OriginalUom.Length > 0)
                        uom_3.AppendChild(XML.CreateTextNode(line.OriginalUom));
                    else
                        uom_3.AppendChild(XML.CreateTextNode(line.UOM));
                    confirmationStatus_2.AppendChild(uom_3);

                    confirmationItem.AppendChild(confirmationStatus_2);
                }
                else if (line.PurchasingCode == "CNCLD")
                {
                    XmlElement confirmationStatus_2 = XML.CreateElement("ConfirmationStatus");
                    confirmationStatus_2.SetAttribute("quantity", qty.ToString("G29"));
                    confirmationStatus_2.SetAttribute("type", "reject");

                    XmlElement uom_3 = XML.CreateElement("UnitOfMeasure");
                    if (line.OriginalUom.Length > 0)
                        uom_3.AppendChild(XML.CreateTextNode(line.OriginalUom));
                    else
                        uom_3.AppendChild(XML.CreateTextNode(line.UOM));
                    confirmationStatus_2.AppendChild(uom_3);

                    confirmationItem.AppendChild(confirmationStatus_2);
                }
                else
                {
                    XmlElement confirmationStatus = XML.CreateElement("ConfirmationStatus");
                    confirmationStatus.SetAttribute("quantity", qty.ToString("G29"));
                    confirmationStatus.SetAttribute("type", "accept");
                    string shipDate = "";
                    if (line.ShipmentDate != new DateTime(1753, 1, 1) && line.ShipmentDate.Date != DateTime.Now.Date)
                        shipDate = line.ShipmentDate.ToString(order.Credentials.TimestampFormat);
                    else
                        shipDate = "";

                    if (order.Credentials.ShipmentDate == 1 && shipDate.Length > 0)
                        confirmationStatus.SetAttribute("shipmentDate", shipDate);

                    if (order.Credentials.DeliveryDate == 1 && shipDate.Length > 0)
                        confirmationStatus.SetAttribute("deliveryDate", shipDate);

                    XmlElement uom_2 = XML.CreateElement("UnitOfMeasure");
                    if (line.OriginalUom.Length > 0)
                        uom_2.AppendChild(XML.CreateTextNode(line.OriginalUom));
                    else
                        uom_2.AppendChild(XML.CreateTextNode(line.UOM));
                    confirmationStatus.AppendChild(uom_2);

                    confirmationItem.AppendChild(confirmationStatus);
                }

                return confirmationItem;
            }
            else
                return null;
        }
    }
    public class ShipNotice : BuildXml
    {
        public override XmlDocument XML { get; set; }
        public override int LineCount { get; set; }

        public ShipNotice(OrderHeader order)
        {
            LineCount = 0;
            CreateXml(order);
            CreateHeader(order);
            CreateShipNotice(order);
        }

        private void CreateShipNotice(OrderHeader order)
        {
            XmlElement request = XML.CreateElement("Request");
            request.SetAttribute("deploymentMode", "production");

            XmlElement shipNoticeRequest = XML.CreateElement("ShipNoticeRequest");
            shipNoticeRequest.AppendChild(CreateShipNoticeHeader(order));
            shipNoticeRequest.AppendChild(CreateShipControl(order));
            shipNoticeRequest.AppendChild(CreateShipNoticePortion(order));

            request.AppendChild(shipNoticeRequest);
            XML.SelectSingleNode("cXML").AppendChild(request);
        }

        private XmlElement CreateShipNoticeHeader(OrderHeader order)
        {
            XmlElement shipNoticeHeader = XML.CreateElement("ShipNoticeHeader");

            if (order.Credentials.FromIdentity_1 == "AN01000041357-T" || order.Credentials.FromIdentity_1 == "AN01000041357")
            {
                shipNoticeHeader.SetAttribute("shipmentType", "actual");
                shipNoticeHeader.SetAttribute("deliveryDate", DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
                shipNoticeHeader.SetAttribute("shipmentDate", DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
                shipNoticeHeader.SetAttribute("noticeDate", DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
                shipNoticeHeader.SetAttribute("operation", "new");
                shipNoticeHeader.SetAttribute("shipmentID", order.ShipmentNo);

                shipNoticeHeader.AppendChild(CreateShipFromContact(order));
                shipNoticeHeader.AppendChild(CreateShipToContact(order));
            }
            else
            {
                shipNoticeHeader.SetAttribute("shipmentID", order.ShipmentNo);
                shipNoticeHeader.SetAttribute("noticeDate", DateTime.Now.ToString("yyyyMMdd"));
                shipNoticeHeader.SetAttribute("shipmentDate", order.ShipmentDate.ToString("yyyyMMdd"));
            }
            return shipNoticeHeader;
        }

        private XmlElement CreateShipFromContact(OrderHeader order)
        {
            XmlElement contact = XML.CreateElement("Contact");
            contact.SetAttribute("addressID", $"GSS{order.LocationCode}");
            contact.SetAttribute("role", "shipFrom");

            XmlElement name = XML.CreateElement("Name");
            name.SetAttribute("xml:lang", "en");
            name.AppendChild(XML.CreateTextNode("Government Scientific Source"));
            contact.AppendChild(name);

            XmlElement postalAddress = XML.CreateElement("PostalAddress");

            XmlElement street = null
                , street2 = null
                , city = null
                , state = null
                , postalCode = null;

            switch (order.LocationCode)
            {
                case "VA":
                    street = XML.CreateElement("Street");
                    street.AppendChild(XML.CreateTextNode("13894 Redskin Drive"));

                    street2 = XML.CreateElement("Street");
                    street2.AppendChild(XML.CreateTextNode("Building 6"));

                    city = XML.CreateElement("City");
                    city.AppendChild(XML.CreateTextNode("Herndon"));

                    state = XML.CreateElement("State");
                    state.AppendChild(XML.CreateTextNode("VA"));

                    postalCode = XML.CreateElement("PostalCode");
                    postalCode.AppendChild(XML.CreateTextNode("20171"));
                    break;

                case "CA":
                    street = XML.CreateElement("Street");
                    street.AppendChild(XML.CreateTextNode("258 Lindbergh Avenue"));

                    city = XML.CreateElement("City");
                    city.AppendChild(XML.CreateTextNode("Livermore"));

                    state = XML.CreateElement("State");
                    state.AppendChild(XML.CreateTextNode("CA"));

                    postalCode = XML.CreateElement("PostalCode");
                    postalCode.AppendChild(XML.CreateTextNode("94551"));
                    break;

                case "NM":
                    street = XML.CreateElement("Street");
                    street.AppendChild(XML.CreateTextNode("2701 Broadway Blvd. NE"));

                    city = XML.CreateElement("City");
                    city.AppendChild(XML.CreateTextNode("Albuquerque"));

                    state = XML.CreateElement("State");
                    state.AppendChild(XML.CreateTextNode("NM"));

                    postalCode = XML.CreateElement("PostalCode");
                    postalCode.AppendChild(XML.CreateTextNode("87107"));
                    break;

                case "TN":
                    street = XML.CreateElement("Street");
                    street.AppendChild(XML.CreateTextNode("10903 McBride LN"));

                    city = XML.CreateElement("City");
                    city.AppendChild(XML.CreateTextNode("Knoxville"));

                    state = XML.CreateElement("State");
                    state.AppendChild(XML.CreateTextNode("TN"));

                    postalCode = XML.CreateElement("PostalCode");
                    postalCode.AppendChild(XML.CreateTextNode("37932"));
                    break;

                case "WH":
                    street = XML.CreateElement("Street");
                    street.AppendChild(XML.CreateTextNode("12355 Sunrise Valley Dr. Suite 400"));

                    city = XML.CreateElement("City");
                    city.AppendChild(XML.CreateTextNode("Reston"));

                    state = XML.CreateElement("State");
                    state.AppendChild(XML.CreateTextNode("VA"));

                    postalCode = XML.CreateElement("PostalCode");
                    postalCode.AppendChild(XML.CreateTextNode("20191"));
                    break;
            }

            if (street != null) postalAddress.AppendChild(street);
            if (street2 != null) postalAddress.AppendChild(street2);
            if (city != null) postalAddress.AppendChild(city);
            if (state != null) postalAddress.AppendChild(state);
            if (postalCode != null) postalAddress.AppendChild(postalCode);

            XmlElement country = XML.CreateElement("Country");
            country.SetAttribute("isoCountryCode", "US");
            country.AppendChild(XML.CreateTextNode("United States"));

            if (country != null) postalAddress.AppendChild(country);

            contact.AppendChild(postalAddress);

            return contact;
        }

        private XmlElement CreateShipToContact(OrderHeader order)
        {
            XmlElement contact = XML.CreateElement("Contact");
            contact.SetAttribute("addressID", order.ShipToAddress.Code);
            contact.SetAttribute("role", "shipTo");

            XmlElement name = XML.CreateElement("Name");
            name.SetAttribute("xml:lang", "en");
            name.AppendChild(XML.CreateTextNode(order.ShipToAddress.Name));
            contact.AppendChild(name);

            XmlElement postalAddress = XML.CreateElement("PostalAddress");

            XmlElement deliverTo = null
                , street = null
                , street2 = null
                , city = null
                , state = null
                , postalCode = null;

            deliverTo = XML.CreateElement("DeliverTo");
            deliverTo.AppendChild(XML.CreateTextNode(order.ShipToAddress.Contact));

            street = XML.CreateElement("Street");
            street.AppendChild(XML.CreateTextNode(order.ShipToAddress.Street));

            if (order.ShipToAddress.Street2.Length > 0)
            {
                street2 = XML.CreateElement("Street");
                street2.AppendChild(XML.CreateTextNode(order.ShipToAddress.Street2));
            }

            city = XML.CreateElement("City");
            city.AppendChild(XML.CreateTextNode(order.ShipToAddress.City));

            state = XML.CreateElement("State");
            state.AppendChild(XML.CreateTextNode(order.ShipToAddress.State));

            postalCode = XML.CreateElement("PostalCode");
            postalCode.AppendChild(XML.CreateTextNode(order.ShipToAddress.PostCode));

            if (street != null) postalAddress.AppendChild(street);
            if (street2 != null) postalAddress.AppendChild(street2);
            if (city != null) postalAddress.AppendChild(city);
            if (state != null) postalAddress.AppendChild(state);
            if (postalCode != null) postalAddress.AppendChild(postalCode);

            XmlElement country = XML.CreateElement("Country");
            country.SetAttribute("isoCountryCode", "US");
            country.AppendChild(XML.CreateTextNode("United States"));

            if (country != null) postalAddress.AppendChild(country);

            contact.AppendChild(postalAddress);

            return contact;
        }

        private XmlElement CreateShipControl(OrderHeader order)
        {
            XmlElement shipControl = XML.CreateElement("ShipControl");

            XmlElement carrierIdentifier = XML.CreateElement("CarrierIdentifier");
            if (order.Credentials.FromIdentity_1 == "AN01000041357-T" || order.Credentials.FromIdentity_1 == "AN01000041357")
                carrierIdentifier.SetAttribute("domain", "companyName");
            else
                carrierIdentifier.SetAttribute("domain", order.TrackingCarrier);
            carrierIdentifier.AppendChild(XML.CreateTextNode(order.TrackingCarrier));
            shipControl.AppendChild(carrierIdentifier);

            XmlElement shipmentIdentifier = XML.CreateElement("ShipmentIdentifier");
            if (order.Credentials.FromIdentity_1 == "AN01000041357-T" || order.Credentials.FromIdentity_1 == "AN01000041357")
                shipmentIdentifier.SetAttribute("domain", "trackingNumber");
            shipmentIdentifier.AppendChild(XML.CreateTextNode(order.TrackingNo));
            shipControl.AppendChild(shipmentIdentifier);

            return shipControl;
        }

        private XmlElement CreateShipNoticePortion(OrderHeader order)
        {
            XmlElement shipNoticePortion = XML.CreateElement("ShipNoticePortion");
            shipNoticePortion.AppendChild(CreateOrderReference(order));

            foreach (Extrinsic extr in order.Extrinsics)
            {
                XmlElement extrinsic = XML.CreateElement("Extrinsic");
                extrinsic.SetAttribute("name", extr.Name);
                extrinsic.AppendChild(XML.CreateTextNode(extr.Value));
                shipNoticePortion.AppendChild(extrinsic);
            }

            foreach (OrderLine line in order.Lines)
            {
                XmlElement item = CreateShipNoticeItem(order, line);
                if (item != null)
                {
                    shipNoticePortion.AppendChild(item);
                    LineCount++;
                }
            }

            return shipNoticePortion;
        }

        private XmlElement CreateShipNoticeItem(OrderHeader order, OrderLine line)
        {
            if (line.Qty > 0.0M)
            {
                int tempLineNumber = line.OriginalLineNumber > 0 ? line.OriginalLineNumber : line.LineNumber / 10000;

                string lineNumber = line.ManufacturerPartID;
                if (order.Credentials.LineNumber == "lineNumber")
                    lineNumber = tempLineNumber.ToString();

                if (lineNumber.Length == 0)
                    throw new Exception($"{order.SalesOrderNo}, {order.CustomerPONo} Original Manufacturer Part ID is required!");

                XmlElement shipNoticeItem = XML.CreateElement("ShipNoticeItem");
                if (order.Credentials.FromIdentity_1 == "AN01000041357-T" || order.Credentials.FromIdentity_1 == "AN01000041357")
                    shipNoticeItem.SetAttribute("shipNoticeLineNumber", lineNumber);
                shipNoticeItem.SetAttribute("lineNumber", lineNumber);
                shipNoticeItem.SetAttribute("quantity", line.Qty.ToString("G29"));

                if (order.Credentials.FromIdentity_1 == "AN01000041357-T" || order.Credentials.FromIdentity_1 == "AN01000041357")
                {
                    XmlElement itemId = XML.CreateElement("ItemID");
                    XmlElement supplierPartId = XML.CreateElement("SupplierPartID");
                    supplierPartId.AppendChild(XML.CreateTextNode(line.GSSPartNo));
                    itemId.AppendChild(supplierPartId);
                    shipNoticeItem.AppendChild(itemId);

                    XmlElement shipNoticeItemDetail = XML.CreateElement("ShipNoticeItemDetail");
                    XmlElement unitPrice = XML.CreateElement("UnitPrice");
                    XmlElement money = XML.CreateElement("Money");
                    money.SetAttribute("currency", "USD");
                    money.AppendChild(XML.CreateTextNode(line.UnitPrice.ToString("C").Replace("$", "")));
                    unitPrice.AppendChild(money);
                    shipNoticeItemDetail.AppendChild(unitPrice);

                    XmlElement description = XML.CreateElement("Description");
                    description.SetAttribute("xml:lang", "en-US");
                    description.AppendChild(XML.CreateTextNode(HttpUtility.UrlEncode(line.Description)));
                    shipNoticeItemDetail.AppendChild(description);

                    XmlElement uom = XML.CreateElement("UnitOfMeasure");
                    if(line.OriginalUom.Length>0)
                        uom.AppendChild(XML.CreateTextNode(line.OriginalUom));
                    else
                        uom.AppendChild(XML.CreateTextNode(line.UOM));
                    shipNoticeItemDetail.AppendChild(uom);

                    XmlElement manuPartId = XML.CreateElement("ManufacturerPartID");
                    manuPartId.AppendChild(XML.CreateTextNode(line.GSSPartNo));
                    shipNoticeItemDetail.AppendChild(manuPartId);

                    XmlElement manufacturer = XML.CreateElement("ManufacturerName");
                    manufacturer.AppendChild(XML.CreateTextNode("GSS"));
                    shipNoticeItemDetail.AppendChild(manufacturer);

                    shipNoticeItem.AppendChild(shipNoticeItemDetail);
                }

                XmlElement unitOfMeasure = XML.CreateElement("UnitOfMeasure");
                if (line.OriginalUom.Length > 0)
                    unitOfMeasure.AppendChild(XML.CreateTextNode(line.OriginalUom));
                else
                    unitOfMeasure.AppendChild(XML.CreateTextNode(line.UOM));
                shipNoticeItem.AppendChild(unitOfMeasure);

                return shipNoticeItem;
            }
            else
                return null;
        }
    }
}
