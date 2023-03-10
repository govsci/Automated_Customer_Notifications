using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automated_Customer_Notifications.Objects
{
    public class AutoResponder
    {
        public AutoResponder(string header, string footer, string style, string subject, string bodystyle, string body, string cart, string extraCharges)
        {
            Header = header;
            Footer = footer;
            Style = style;
            Subject = subject;
            BodyStyle = bodystyle;
            Body = body;
            Cart = cart;
            ExtraChargesFormat = extraCharges;

            EmailTo = "";
            EmailCc = "";
            EmailBcc = "";
        }

        public string ID { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string Style { get; set; }
        public string Subject { get; set; }
        public string BodyStyle { get; set; }
        public string Body { get; set; }
        public string Cart { get; set; }
        public string ExtraChargesFormat { get; set; }

        public string EmailTo { get; set; }
        public string EmailCc { get; set; }
        public string EmailBcc { get; set; }

        public OrderHeader Order { get; set; }
    }
}
