using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automated_Customer_Notifications.Objects;
using Automated_Customer_Notifications.Classes;
using System.IO;

namespace Automated_Customer_Notifications
{
    public class Send_Cxml_Notices_Not_Sent
    {
        public Send_Cxml_Notices_Not_Sent()
        {
            foreach(Cxml_Notice notice in Database.GetCxmlNoticesNotSent())
                if (File.Exists(notice.CxmlFile))
                    if (Constants.SendCxml(File.ReadAllText(notice.CxmlFile), notice.DocumentType, notice.URL) == "SENT")
                        Database.UpdateCxmlNotice(notice.ID, DateTime.Now);
        }
    }
}
