using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Automated_Customer_Notifications.Objects
{
    public class OrderHeader
    {
        public OrderHeader()
        {
            SalesOrderNo =
                ShipmentNo =
                ContactName =
                CustomerPONo =
                ContactEmail =
                ContactPhone =
                ContactFax =
                SalespersonName =
                SalespersonEmail =
                SalespersonPhone =
                SalesTeamEmail =
                SalesTeamPhone =
                SellToCustomerNo =
                LocationCode =
                BPA =
                CallNo =
                TrackingNo =
                TrackingCarrier =
                PaymentTermsCode =
                URL =
                OrgPayloadID = "";

            Lines = new List<OrderLine>();
            ExtraCharges = new List<ExtraCharge>();
            Extrinsics = new List<Extrinsic>();
            ShipmentDate = OrderDate = EstDelDate = new DateTime(1753, 1, 1);

            CxmlSent = false;
            CxmlDateStamp = new DateTime(1753, 1, 1);
            CxmlFile = "";

        }
        public string SalesOrderNo { get; set; }
        public string ShipmentNo { get; set; }
        public string ContactName { get; set; }
        public string CustomerPONo { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactFax { get; set; }
        public string SalespersonName { get; set; }
        public string SalespersonEmail { get; set; }
        public string SalespersonPhone { get; set; }
        public string SalesTeamEmail { get; set; }
        public string SalesTeamPhone { get; set; }
        public string SellToCustomerNo { get; set; }
        public string SellToCustomerName { get; set; }
        public string LocationCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string BPA { get; set; }
        public string CallNo { get; set; }
        public string TrackingNo { get; set; }
        public string TrackingCarrier { get; set; }
        public string PaymentTermsCode { get; set; }
        public Address BillToAddress { get; set; }
        public Address ShipToAddress { get; set; }
        public DateTime ShipmentDate { get; set; }
        public string URL { get; set; }

        public DateTime EstDelDate { get; set; }
        public List<OrderLine> Lines { get; set; }
        public List<ExtraCharge> ExtraCharges { get; set; }

        public XmlCredentials Credentials { get; set; }
        public List<Extrinsic> Extrinsics { get; set; }
        public string OrgPayloadID { get; set; }
        public XmlDocument XML { get; set; }
        public string CxmlFile { get; set; }
        public bool CxmlSent { get; set; }
        public DateTime CxmlDateStamp { get; set; }
    }

    public class OrderLine
    {
        public OrderLine()
        {
            GSSPartNo =
                VendorNo =
                VendorItemNo =
                VendorName =
                Description =
                UOM =
                PurchaseOrderNo =
                Type =
                PurchasingCode =
                ManufacturerPartID = 
                OriginalUom = "";
            UnitPrice =
                Qty =
                ExtendedPrice =
                TotalTax = 0.0m;
            PurchaseOrderLineNo =
                LineNumber =
                OriginalLineNumber = 0;
            BackOrderDate = ShipmentDate = RequestedDeliveryDate = new DateTime(1753, 1, 1);
        }
        public string GSSPartNo { get; set; }
        public string VendorNo { get; set; }
        public string VendorItemNo { get; set; }
        public string VendorName { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public string UOM { get; set; }
        public decimal Qty { get; set; }
        public DateTime RequestedDeliveryDate { get; set; }
        public string PurchaseOrderNo { get; set; }
        public int PurchaseOrderLineNo { get; set; }
        public string Type { get; set; }
        public decimal ExtendedPrice { get; set; }
        public decimal TotalTax { get; set; }
        public int LineNumber { get; set; }
        public bool BackOrderLine { get; set; }

        public DateTime BackOrderDate { get; set; }

        public string PurchasingCode { get; set; }
        public DateTime ShipmentDate { get; set; }
        public string ManufacturerPartID { get; set; }
        public string UNSPSC { get; set; }
        public string OriginalUom { get; set; }
        public int OriginalLineNumber { get; set; }
    }

    public class ExtraCharge
    {
        public ExtraCharge(string name, decimal charge)
        {
            Name = name;
            Charge = charge;
        }

        public string Name { get; }
        public decimal Charge { get; }
    }

    public class Address
    {
        public Address(string code, string name, string contact, string street, string street2, string city, string state, string postCode)
        {
            Code = code;
            Name = name;
            Contact = contact;
            Street = street;
            Street2 = street2;
            City = city;
            State = state;
            PostCode = postCode;
        }

        public string Code { get; }
        public string Name { get; }
        public string Contact { get; }
        public string Street { get; }
        public string Street2 { get; }
        public string City { get; }
        public string State { get; }
        public string PostCode { get; }
    }

    public class XmlCredentials
    {
        public XmlCredentials()
        {
            FromDomain_1 =
                FromIdentity_1 =
                FromType_1 =
                FromDomain_2 =
                FromIdentity_2 =
                FromType_2 =
                FromDomain_3 =
                FromIdentity_3 =
                ToDomain =
                ToIdentity =
                ToDomain_2 =
                ToIdentity_2 =
                SenderDomain_1 =
                SenderIdentity_1 =
                SenderSharedSecret_1 =
                SenderDomain_2 =
                SenderIdentity_2 =
                SenderSharedSecret_2 =
                UserAgent =
                URL =
                SystemID =
                cXmlVersion =
                TimestampFormat =
                PayloadIdFormat = "";

            DeliveryDate = ShipmentDate = OrderConfirm = ASN = 0;
        }
        public string FromDomain_1 { get; set; }
        public string FromIdentity_1 { get; set; }
        public string FromType_1 { get; set; }
        public string FromDomain_2 { get; set; }
        public string FromIdentity_2 { get; set; }
        public string FromType_2 { get; set; }
        public string FromDomain_3 { get; set; }
        public string FromIdentity_3 { get; set; }
        public string ToDomain { get; set; }
        public string ToIdentity { get; set; }
        public string ToDomain_2 { get; set; }
        public string ToIdentity_2 { get; set; }
        public string SenderDomain_1 { get; set; }
        public string SenderIdentity_1 { get; set; }
        public string SenderSharedSecret_1 { get; set; }
        public string SenderDomain_2 { get; set; }
        public string SenderIdentity_2 { get; set; }
        public string SenderSharedSecret_2 { get; set; }
        public string UserAgent { get; set; }
        public string URL { get; set; }
        public string SystemID { get; set; }
        public string cXmlVersion { get; set; }
        public string TimestampFormat { get; set; }
        public string PayloadIdFormat { get; set; }
        public string TlsVersion { get; set; }
        public string LineNumber { get; set; }
        public int DeliveryDate { get; set; }
        public int ShipmentDate { get; set; }
        public int ASN { get; set; }
        public int OrderConfirm { get; set; }
    }

    public class Extrinsic
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
