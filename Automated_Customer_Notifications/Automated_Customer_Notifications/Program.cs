using System;
using Automated_Customer_Notifications.Classes;

namespace Automated_Customer_Notifications
{
    class Program
    {
        public Program()
        {
            try
            {
                new NAV_Order_Acknowledgement_New();
                new NAV_Order_Confirmation();
                new NAV_Inventory_Order_Confirmation();
                new Est_Delivery_Date_Change_Notifications();
                new Canceled_Notifications();
                new NAV_Shipping_Notifications();
                /*new NAV_Delivery_Notifications();*/
                new Send_Cxml_Notices_Not_Sent();
            }
            catch(Exception ex)
            {
                Email.SendErrorMessage(ex, "Main", "Program", null);
            }
        }
        public static void Main(string[] args)
        {
            new Program();
        }
    }
}
