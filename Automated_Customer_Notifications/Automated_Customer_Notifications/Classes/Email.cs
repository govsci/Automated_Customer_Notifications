using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Automated_Customer_Notifications.Objects;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace Automated_Customer_Notifications.Classes
{
    public static class Email
    {
        public static void SendMail(string msg, string subject, string emailTo, string emailCC, string emailBCC, string file, bool html)
        {
            EmailConfig emailConfig = GetEmailConfiguration();
            if (emailTo.Length == 0)
                emailTo = emailConfig.AdminEmail;

            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = html;
            string error = "";

            if (msg.Length == 0)
                error += "\nBody of the email is empty";
            else
                mail.Body = msg;
            if (subject.Length == 0)
                error += "\nSubject of the email is blank";
            else
                mail.Subject = subject;

            //CC
            if (emailCC.Length > 0)
            {
                if (emailCC.Contains(';'))
                {
                    string[] emails = emailCC.Split(';');
                    foreach (string email in emails)
                    {
                        if (email.Length > 0)
                        {
                            if (TestEmail(email, false))
                                mail.CC.Add(new MailAddress(email));
                            else
                                error += "\nEmail Carbon Copy (CC) Address is not valid: " + email;
                        }
                    }
                }
                else
                {
                    if (TestEmail(emailCC, false))
                        mail.CC.Add(new MailAddress(emailCC));
                    else
                        error += "\nEmail Carbon Copy (CC) Address is not valid: " + emailCC;
                }
            }

            //From
            mail.From = new MailAddress("ecommercesystem@govsci.com");

            //To
            if (emailTo.Contains(';'))
            {
                string[] emails = emailTo.Split(';');
                for (int i = 0; i < emails.Length; i++)
                {
                    if (TestEmail(emails[i], true))
                        mail.To.Add(new MailAddress(emails[i]));
                    else
                        error += "\nEmail To Address is not valid: " + emails[i];
                }
            }
            else
            {
                if (TestEmail(emailTo, true))
                    mail.To.Add(new MailAddress(emailTo));
                else
                    error += "\nEmail To Address is not valid: " + emailTo;
            }

            //BCC
            if (emailBCC.Length > 0)
            {
                if (emailBCC.Contains(';'))
                {
                    string[] emails = emailBCC.Split(';');
                    foreach (string email in emails)
                    {
                        if (email.Length > 0)
                        {
                            if (TestEmail(email, false))
                                mail.Bcc.Add(new MailAddress(email));
                            else
                                error += "\nEmail Blind Carbon Copy (BCC) Address is not valid: " + email;
                        }
                    }
                }
                else
                {
                    if (TestEmail(emailBCC, false))
                        mail.Bcc.Add(new MailAddress(emailBCC));
                    else
                        error += "\nEmail Blind Carbon Copy (BCC) Address is not valid: " + emailBCC;
                }
            }

            //File
            if (file.Length > 0)
            {
                if (file.Contains(';'))
                {
                    string[] files = file.Split(';');
                    foreach (string fil in files)
                    {
                        if (fil.Length > 0 && File.Exists(fil))
                            mail.Attachments.Add(new Attachment(fil));
                    }
                }
                else if (file.Contains(','))
                {
                    string[] files = file.Split(',');
                    foreach (string fil in files)
                        if (fil.Length > 0 && File.Exists(fil))
                            mail.Attachments.Add(new Attachment(fil));
                }
                else if (File.Exists(file))
                    mail.Attachments.Add(new Attachment(file));
            }

            if (error.Length == 0)
                Send(mail, emailConfig);
            else
                throw new Exception("The following errors have occurred: " + error);
        }
        public static void SendErrorMessage(Exception ex, string cs, string method, SqlCommand cmd)
        {
            string msg = "The following errors have occurred in Automate_Customer_Notifications." + cs + "." + method + ":\n\n"
                + "Error: " + ex.ToString();
            if (cmd != null && cmd.CommandText.Length > 0)
            {
                msg += "\n\nQuery: " + cmd.CommandText + " ";
                foreach (SqlParameter param in cmd.Parameters)
                    msg += param.ParameterName + "='" + param.Value + "', ";
                msg = msg.Remove(msg.LastIndexOf(','));
            }
            Email.SendMail(msg, "Automate_Customer_Notifications Error", "dev_error@govsci.com", "", "", "", false);
        }
        public static void Send(MailMessage mail, EmailConfig emailConfig)
        {
            try
            {
                SmtpClient client = new SmtpClient(emailConfig.Host);
                client.Credentials = new NetworkCredential(emailConfig.Username, emailConfig.Password, emailConfig.Domain);
                client.Send(mail);
            }
            catch (Exception ex)
            {
                SendErrorMessage(ex, "Email", "Send", null);
            }
        }
        public static EmailConfig GetEmailConfiguration()
        {
            try
            {
                using (SqlConnection dbcon = new SqlConnection(Constants.connectionEcommerce))
                {
                    dbcon.Open();
                    SqlCommand cmd = new SqlCommand("[dbo].[Ecommerce.Get.Email.Configuration]", dbcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                            return new EmailConfig(rs["host"].ToString(), rs["username"].ToString(), rs["password"].ToString(), rs["domain"].ToString(), rs["admin"].ToString());
                    }
                }
            }
            catch (Exception)
            {
            }

            return new EmailConfig(ConfigurationManager.AppSettings["emailHost"], ConfigurationManager.AppSettings["emailUsername"], ConfigurationManager.AppSettings["emailPassword"], ConfigurationManager.AppSettings["emailDomain"], ConfigurationManager.AppSettings["emailAdmin"]);
        }
        public static bool TestEmail(string email, bool req)
        {
            try
            {
                if (email.Length > 0)
                    new MailAddress(email);
                else if (req)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
