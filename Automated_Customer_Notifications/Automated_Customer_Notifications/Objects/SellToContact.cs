using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automated_Customer_Notifications.Objects
{
    public class SellToContact
    {
        public SellToContact(string email, string spname, string spemail, string spphone)
        {
            TeamEmail = email;
            SalespersonEmail = spemail;
            SalespersonName = spname;
            SalespersonPhone = spphone;
        }
        public string TeamEmail { get; }
        public string SalespersonName { get; }
        public string SalespersonEmail { get; }
        public string SalespersonPhone { get; }
    }
}
