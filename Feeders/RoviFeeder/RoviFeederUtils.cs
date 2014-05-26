using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RoviFeeder
{
    public class RoviFeederUtils
    {
        public static Dictionary<int, string> GetPresentationsDict(string sVodListUrl)
        {
            Dictionary<int, string> dPresentationUrls = new Dictionary<int, string>();
            
            try
            {                
                //string roviVodUrl = ContentURL;

                if (string.IsNullOrEmpty(sVodListUrl))
                {
                    return dPresentationUrls;
                }

                Uri vodListRes = new Uri(sVodListUrl);
                if ((!Uri.TryCreate(sVodListUrl, UriKind.Absolute, out vodListRes)) || ((vodListRes.Scheme != Uri.UriSchemeHttp) && (vodListRes.Scheme != Uri.UriSchemeHttps)))
                {
                    return dPresentationUrls;
                }

                DateTime dNow = DateTime.UtcNow;
                string roviXML = TVinciShared.WS_Utils.SendXMLHttpReq(sVodListUrl, "", "", "application/json", "", "", "", "", "get");
                double dTime = DateTime.UtcNow.Subtract(dNow).TotalMilliseconds;

                if (string.IsNullOrEmpty(roviXML))
                {
                    return dPresentationUrls;
                }


                var serializer = new XmlSerializer(typeof(RoviFeeder.ObjectList.RoviNowtilusVodApi));
                RoviFeeder.ObjectList.RoviNowtilusVodApi roviResult;

                using (TextReader reader = new StringReader(roviXML))
                {
                    roviResult = (RoviFeeder.ObjectList.RoviNowtilusVodApi)serializer.Deserialize(reader);
                }

                if (roviResult == null || roviResult.PresentationList == null || roviResult.PresentationList.Count() == 0)
                {
                    return dPresentationUrls;
                }

                for (int i = 0; i < roviResult.PresentationList.Count(); i++)
                {
                    RoviFeeder.ObjectList.RoviNowtilusVodApiPresentation item = roviResult.PresentationList[i];
                    //int id = item.id;
                    string url = item.href;
                    int id = int.Parse(url.Split('/').Last());

                    dPresentationUrls[id] = url;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("exception in GetPresentationsDict {0}", ex.Message), "RoviFeeder");
            }

            return dPresentationUrls;
        }
        public static bool Validate(RoviFeeder.MoviesXSD.RoviNowtilusVodApiPresentation roviTitle)
        {
            if (roviTitle == null)
            {
                return false;
            }

            if ((roviTitle.LicenseList == null) || (roviTitle.LicenseList.Length == 0))
            {
                return false;
            }

            return true;
        }
        public static bool Validate(RoviFeeder.SeriesXSD.RoviNowtilusVodApiPresentation roviTitle)
        {
            if (roviTitle == null)
            {
                return false;
            }

            return true;
        }
        public static bool Validate(RoviFeeder.EpisodeXSD.RoviNowtilusVodApiPresentation roviTitle)
        {
            if (roviTitle == null)
            {
                return false;
            }

            if ((roviTitle.LicenseList == null) || (roviTitle.LicenseList.Length == 0))
            {
                return false;
            }

            return true;
        }
        public static bool Validate(RoviFeeder.CMT_XSD.RoviNowtilusVodApiCampaign roviCampaign)
        {
            if (roviCampaign == null)
            {
                return false;
            }

            if ((roviCampaign.PlacementList == null) || (roviCampaign.PlacementList.Length == 0))
            {
                return false;
            }

            return true;
        }

        public static bool SendIngestNotification(IngestNotificationStatus status, string sURL, string logMsg)
        {
            try
            {
                Logger.Logger.Log("Start", string.Format("{0}, {1}, {2}", status.ToString(), sURL, logMsg), "RoviIngestNotification");
                string theNotificationXML = string.Empty;
                Notification.RoviNowtilusVodApi notificationClass = new Notification.RoviNowtilusVodApi();

                notificationClass.RequestType = "STATUS";
                notificationClass.Status = new Notification.RoviNowtilusVodApiStatus();
                notificationClass.Status.LogLevel = status.ToString();
                notificationClass.Status.Message = logMsg;


                using (StringWriter sw = new StringWriter())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Notification.RoviNowtilusVodApi));
                    serializer.Serialize(sw, notificationClass);
                    theNotificationXML = sw.ToString();
                }

                string response = TVinciShared.WS_Utils.SendXMLHttpReqWithHeaders(sURL, theNotificationXML, new Dictionary<string, string>() { });
                Logger.Logger.Log("Finish", string.Format("{0}", response), "RoviIngestNotification");
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", string.Format("{0}", ex.Message), "RoviIngestNotification");
            }



            return true;
        }
    }
}
