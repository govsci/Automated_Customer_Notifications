using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automated_Customer_Notifications
{
    public class Contact
    {
        public Contact(string email, string sendType, string contactName, string sellTo, int copy)
        {
            EmailAddress = email;
            SendType = sendType;
            ContactName = contactName;
            SellToCustomerNo = sellTo;
            CopyEmail = copy;
        }
        public string EmailAddress { get; }
        public string SendType { get; }
        public string ContactName { get; }
        public string SellToCustomerNo { get; }
        public int CopyEmail { get; }
    }
}
