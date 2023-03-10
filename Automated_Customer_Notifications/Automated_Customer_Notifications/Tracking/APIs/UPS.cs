using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automated_Customer_Notifications.API_UPS;
using System.Text.RegularExpressions;

namespace Automated_Customer_Notifications.Tracking.APIs
{
    class UPS : API
    {
        public override void GetTrackingInfo(string trackingNumber)
        {
            try
            {
                TrackService track = new TrackService();
                TrackRequest tr = new TrackRequest();
                UPSSecurity upss = new UPSSecurity();
                UPSSecurityServiceAccessToken upssSvcAccessToken = new UPSSecurityServiceAccessToken();
                upssSvcAccessToken.AccessLicenseNumber = "4D4D80672395540C";
                upss.ServiceAccessToken = upssSvcAccessToken;
                UPSSecurityUsernameToken upssUsrNameToken = new UPSSecurityUsernameToken();
                upssUsrNameToken.Username = "GOVSCI";
                upssUsrNameToken.Password = "GSS728";
                upss.UsernameToken = upssUsrNameToken;
                track.UPSSecurityValue = upss;
                RequestType requestu = new RequestType();
                String[] requestOption = { "15" };
                requestu.RequestOption = requestOption;
                tr.Request = requestu;
                tr.InquiryNumber = trackingNumber;
                //System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
                TrackResponse trackResponse = track.ProcessTrack(tr);
                if (trackResponse.Shipment[0].Package == null)
                    Status = "Tracking Not Available";
                else if (trackResponse.Shipment[0].Package[0].Activity[0].Status.Type == "D")
                {
                    Status = "Delivered";
                    StatusDatetime = DateTime.Parse(GetDateTime("Date", trackResponse.Shipment[0].Package[0].Activity[0].Date) + " " + GetDateTime("Time", trackResponse.Shipment[0].Package[0].Activity[0].Time));
                }
                else
                    Status = "In Transit";
            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                Status = "Tracking Not Available";
            }
            catch (Exception ex)
            {
                throw new Exception("Status : " + Status + " | Tracking Number: " + trackingNumber + " | UPS Error : " + ex.ToString());
            }
        }

        public string GetDateTime(string method, string val)
        {
            string value = "";
            Match match;

            switch (method)
            {
                case "Date":
                    match = Regex.Match(val, @"^(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})$");
                    if (match.Success)
                        value = match.Groups["month"].Value + "/" + match.Groups["day"].Value + "/" + match.Groups["year"].Value;
                    break;

                case "Time":
                    match = Regex.Match(val, @"^(?<hour>\d{2})(?<minute>\d{2})");
                    if (match.Success)
                        value = match.Groups["hour"].Value + ":" + match.Groups["minute"].Value;
                    break;
            }

            return value;
        }
    }
}
