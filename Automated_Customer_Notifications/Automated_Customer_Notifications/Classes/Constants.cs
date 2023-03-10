using System;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Security;
using Automated_Customer_Notifications.Objects;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Automated_Customer_Notifications.Classes
{
    public static class Constants
    {
        public static string EcomStoredProcedure = ConfigurationManager.AppSettings["EcommerceSP"];
        public static string NavStoredProcedure = ConfigurationManager.AppSettings["NavisionSP"];
        public static string OrderConfirmFolder = ConfigurationManager.AppSettings["orderConfirmFolder"];
        public static string ShipNoticeFolder = ConfigurationManager.AppSettings["shipNoticeFolder"];
        public static string DeploymentMode = ConfigurationManager.AppSettings["deploymentMode"];
        public static string TestEmail = ConfigurationManager.AppSettings["testEmail"];

        public static string connectionEcommerce = DeploymentMode == "test" ? ConfigurationManager.ConnectionStrings["TstEcomDb"].ConnectionString : ConfigurationManager.ConnectionStrings["Ecommerce"].ConnectionString;
        public static string connectionNavision = DeploymentMode == "test" ? ConfigurationManager.ConnectionStrings["TstNavDb"].ConnectionString : ConfigurationManager.ConnectionStrings["Navision"].ConnectionString;

        public static string WriteToFile(string path, string contents)
        {
            path = path + $@"{DateTime.Now.ToString("yyyy")}\{DateTime.Now.ToString("MM")}\{DateTime.Now.ToString("dd")}\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fileName = $"{DateTime.Now.ToString("yyyyMMddhhmmssffff")}.xml";
            File.WriteAllText(path + fileName, contents);

            return path + fileName;
        }

        public static string SendCxml(OrderHeader order, string method)
        {
            string file = "";
            try
            {
                string path = "";
                switch (method)
                {
                    case "OrderConfirmation":
                        path = OrderConfirmFolder;
                        break;
                    case "ShipNotice":
                        path = ShipNoticeFolder;
                        break;
                }

                switch (order.Credentials.TlsVersion)
                {
                    case "SSL3":
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                        break;
                    case "1.2":
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                        break;
                    case "1.1":
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                        break;
                    case "1.0":
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                        break;
                    case "Bypass":
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                            | SecurityProtocolType.Tls11
                            | SecurityProtocolType.Tls12
                            | SecurityProtocolType.Ssl3;
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                        break;
                }

                file = WriteToFile(path, order.XML.InnerXml);

                if (order.Credentials.URL.Contains("ariba"))
                {
                    MSXML2.ServerXMLHTTP xmlHttp = new MSXML2.ServerXMLHTTP();
                    xmlHttp.open("POST", order.Credentials.URL, false, null, null);
                    xmlHttp.setRequestHeader("Content-type", "text/xml");
                    xmlHttp.send(order.XML.InnerXml);
                    string result = xmlHttp.responseText;
                    WriteToFile(path.Replace("Outgoing","Confirmed"), result);
                    CheckCxmlResponse(order, method, result);
                }
                else
                {
                    byte[] requestBytes = System.Text.Encoding.ASCII.GetBytes(order.XML.InnerXml);

                    HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(order.Credentials.URL);
                    wreq.Timeout = 90000;
                    wreq.Method = "POST";
                    wreq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                    wreq.ContentType = "text/xml;charset=utf-8";
                    wreq.ContentLength = requestBytes.Length;

                    System.IO.Stream rs = wreq.GetRequestStream();
                    rs.Write(requestBytes, 0, requestBytes.Length);
                    rs.Close();

                    HttpWebResponse wr = (HttpWebResponse)wreq.GetResponse();

                    if (wr.StatusCode == HttpStatusCode.OK)
                    {
                        Stream s = wr.GetResponseStream();
                        StreamReader sr = new StreamReader(s);
                        string result = sr.ReadToEnd();

                        if (result.Length > 0)
                        {
                            WriteToFile(path.Replace("Outgoing", "Confirmed"), result);
                            CheckCxmlResponse(order, method, result);
                        }
                        else
                            WriteToFile(path.Replace("Outgoing", "Confirmed"), $"Code:{wr.StatusCode.ToString()}\r\nDescription:{wr.StatusDescription}");
                    }
                }
                //return "FAILED|" + file;
                return "SENT|" + file;
            }
            catch (Exception ex2)
            {
                Email.SendErrorMessage(ex2, "Constants", "SendCxml", null);
                return "FAILED|" + file;
            }

        }

        public static string SendCxml(string xml, string docType, string url)
        {
            try
            {
                string path = "";
                if (docType == "Shipment Notification")
                    path = ShipNoticeFolder;
                else
                    path = OrderConfirmFolder;

                if (url.Contains("ariba"))
                {
                    MSXML2.ServerXMLHTTP xmlHttp = new MSXML2.ServerXMLHTTP();
                    xmlHttp.open("POST", url, false, null, null);
                    xmlHttp.setRequestHeader("Content-type", "text/xml");
                    xmlHttp.send(xml);
                    string result = xmlHttp.responseText;
                    WriteToFile(path.Replace("Outgoing", "Confirmed"), result);
                }
                else
                {

                    byte[] requestBytes = System.Text.Encoding.ASCII.GetBytes(xml);

                    HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(url);
                    wreq.Timeout = 90000;
                    wreq.Method = "POST";
                    wreq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                    wreq.ContentType = "text/xml;charset=utf-8";
                    wreq.ContentLength = requestBytes.Length;

                    System.IO.Stream rs = wreq.GetRequestStream();
                    rs.Write(requestBytes, 0, requestBytes.Length);
                    rs.Close();

                    HttpWebResponse wr = (HttpWebResponse)wreq.GetResponse();

                    if (wr.StatusCode == HttpStatusCode.OK)
                    {
                        Stream s = wr.GetResponseStream();
                        StreamReader sr = new StreamReader(s);
                        string result = sr.ReadToEnd();

                        if (result.Length > 0)
                            WriteToFile(path.Replace("Outgoing", "Confirmed"), result);
                        else
                            WriteToFile(path.Replace("Outgoing", "Confirmed"), $"Code:{wr.StatusCode.ToString()}\r\nDescription:{wr.StatusDescription}");
                    }
                }

                return "SENT";
            }
            catch (Exception ex)
            {
                Email.SendErrorMessage(ex, "Constants", "SendCxml", null);
                return "FAILED";
            }
        }

        public static void CheckCxmlResponse(OrderHeader order, string method, string response)
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(response);

                string statusCode = xml.SelectSingleNode("//Response/Status/@code").InnerXml;
                if (statusCode != "200" && statusCode != "201")
                {
                    string textmsg = xml.SelectSingleNode("//Response/Status/@text").InnerXml;
                    string errormsg = xml.SelectSingleNode("//Response/Status").InnerXml;

                    string msg = $"cXML Notice {method} for {order.SalesOrderNo}, {order.CustomerPONo} sent to {order.Credentials.URL} failed with the following message:"
                        + $"{Environment.NewLine}{Environment.NewLine}Status Code: {statusCode}"
                        + $"{Environment.NewLine}Text: {textmsg}"
                        + $"{Environment.NewLine}Message: {errormsg}";

                    Exception ex = new Exception(msg);
                    Email.SendErrorMessage(ex, "cXML", "Response", null);
                }
            }
            catch { }
        }
    }

    
}