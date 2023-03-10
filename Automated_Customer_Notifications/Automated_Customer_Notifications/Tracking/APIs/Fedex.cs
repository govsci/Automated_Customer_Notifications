using System;
using Automated_Customer_Notifications.API_Fedex;

namespace Automated_Customer_Notifications.Tracking.APIs
{
    class Fedex : API
    {
        public override void GetTrackingInfo(string trackingNumber)
        {
            try
            {
                TrackRequest request = CreateTrackRequest(trackingNumber);
                TrackService service = new TrackService();

                // Call the Track web service passing in a TrackRequest and returning a TrackReply
                TrackReply reply = service.track(request);
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    if (reply.CompletedTrackDetails != null
                        && reply.CompletedTrackDetails[0].TrackDetails != null
                        && reply.CompletedTrackDetails[0].TrackDetails[0].StatusDetail != null
                        && reply.CompletedTrackDetails[0].TrackDetails[0].StatusDetail.Description != null
                        && reply.CompletedTrackDetails[0].TrackDetails[0].DatesOrTimes != null)
                    {
                        Status = reply.CompletedTrackDetails[0].TrackDetails[0].StatusDetail.Description;
                        if (!Status.Contains("cancelled"))
                            StatusDatetime = DateTime.Parse(reply.CompletedTrackDetails[0].TrackDetails[0].DatesOrTimes[0].DateOrTimestamp);
                        else
                            Status = "Tracking Not Available";
                    }
                    else
                        Status = "Tracking Not Available";
                    //ShowTrackReply(reply);
                }
                //ShowNotifications(reply);
            }
            catch(Exception e)
            {
                throw new Exception("Status : " + Status + " | Tracking Number: " + trackingNumber + " | Fedex Error : " + e.ToString());
            }
        }

        private static TrackRequest CreateTrackRequest(string trackingNum)
        {
            // Build the TrackRequest
            TrackRequest request = new TrackRequest();
            //
            /*
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = "5K1rKkcVPV8NDm0N"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = "vo1HmrX7xyeslHXdQq8zSrAfy"; // Replace "XXX" with the Password


            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = "XXXXXX537"; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = "113810977"; // Replace "XXX" with the client's meter number
            */

            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = "uiKRuEl0QQAPycn4"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = "pmKrAqs12IOvN4eZC70svBnjm"; // Replace "XXX" with the Password


            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = "214877600"; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = "110698950"; // Replace "XXX" with the client's meter number



            //
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "***Track Request using VC#***";  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            // Tracking information
            request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
            request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
            request.SelectionDetails[0].PackageIdentifier.Value = trackingNum; // Replace "XXX" with tracking number or door tag

            request.SelectionDetails[0].PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
            //
            // Date range is optional.
            // If omitted, set to false
            request.SelectionDetails[0].ShipDateRangeBegin = DateTime.Parse("06/18/2012"); //MM/DD/YYYY
            request.SelectionDetails[0].ShipDateRangeEnd = request.SelectionDetails[0].ShipDateRangeBegin.AddDays(0);
            request.SelectionDetails[0].ShipDateRangeBeginSpecified = false;
            request.SelectionDetails[0].ShipDateRangeEndSpecified = false;
            //
            // Include detailed scans is optional.
            // If omitted, set to false
            request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
            request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
            return request;
        }

        private static void ShowTrackReply(TrackReply reply)
        {
            // Track details for each package
            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
            {
                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    Console.WriteLine("Tracking details:");
                    Console.WriteLine("**************************************");
                    ShowNotification(trackDetail.Notification);
                    Console.WriteLine("Tracking number: {0}", trackDetail.TrackingNumber);
                    Console.WriteLine("Tracking number unique identifier: {0}", trackDetail.TrackingNumberUniqueIdentifier);
                    Console.WriteLine("Track Status: {0} ({1})", trackDetail.StatusDetail.Description, trackDetail.StatusDetail.Code);
                    Console.WriteLine("Carrier code: {0}", trackDetail.CarrierCode);

                    if (trackDetail.OtherIdentifiers != null)
                    {
                        foreach (TrackOtherIdentifierDetail identifier in trackDetail.OtherIdentifiers)
                        {
                            Console.WriteLine("Other Identifier: {0} {1}", identifier.PackageIdentifier.Type, identifier.PackageIdentifier.Value);
                        }
                    }
                    if (trackDetail.Service != null)
                    {
                        Console.WriteLine("ServiceInfo: {0}", trackDetail.Service.Description);
                    }
                    if (trackDetail.PackageWeight != null)
                    {
                        Console.WriteLine("Package weight: {0} {1}", trackDetail.PackageWeight.Value, trackDetail.PackageWeight.Units);
                    }
                    if (trackDetail.ShipmentWeight != null)
                    {
                        Console.WriteLine("Shipment weight: {0} {1}", trackDetail.ShipmentWeight.Value, trackDetail.ShipmentWeight.Units);
                    }
                    if (trackDetail.Packaging != null)
                    {
                        Console.WriteLine("Packaging: {0}", trackDetail.Packaging);
                    }
                    Console.WriteLine("Package Sequence Number: {0}", trackDetail.PackageSequenceNumber);
                    Console.WriteLine("Package Count: {0} ", trackDetail.PackageCount);
                    if (trackDetail.DatesOrTimes != null)
                    {
                        foreach (TrackingDateOrTimestamp timestamp in trackDetail.DatesOrTimes)
                        {
                            Console.WriteLine("{0}: {1}", timestamp.Type, timestamp.DateOrTimestamp);
                        }
                    }
                    if (trackDetail.DestinationAddress != null)
                    {
                        Console.WriteLine("Destination: {0}, {1}", trackDetail.DestinationAddress.City, trackDetail.DestinationAddress.StateOrProvinceCode);
                    }
                    if (trackDetail.AvailableImages != null)
                    {
                        foreach (AvailableImagesDetail ImageDetail in trackDetail.AvailableImages)
                        {
                            Console.WriteLine("Image availability: {0}", ImageDetail.Type);
                        }
                    }
                    if (trackDetail.NotificationEventsAvailable != null)
                    {
                        foreach (NotificationEventType notificationEventType in trackDetail.NotificationEventsAvailable)
                        {
                            Console.WriteLine("NotificationEvent type : {0}", notificationEventType);
                        }
                    }

                    //Events
                    Console.WriteLine();
                    if (trackDetail.Events != null)
                    {
                        Console.WriteLine("Track Events:");
                        foreach (TrackEvent trackevent in trackDetail.Events)
                        {
                            if (trackevent.TimestampSpecified)
                            {
                                Console.WriteLine("Timestamp: {0}", trackevent.Timestamp);
                            }
                            Console.WriteLine("Event: {0} ({1})", trackevent.EventDescription, trackevent.EventType);
                            Console.WriteLine("***");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("**************************************");
                }
            }

        }
        private static void ShowNotification(Notification notification)
        {
            Console.WriteLine(" Severity: {0}", notification.Severity);
            Console.WriteLine(" Code: {0}", notification.Code);
            Console.WriteLine(" Message: {0}", notification.Message);
            Console.WriteLine(" Source: {0}", notification.Source);
        }
        private static void ShowNotifications(TrackReply reply)
        {
            Console.WriteLine("Notifications");
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Notification no. {0}", i);
                ShowNotification(notification);
            }
        }
    }
}
