using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ps_notifiers
{
    public class MediaCorpMediaNotifier : BaseMediaNotifier
    {
        public MediaCorpMediaNotifier(int groupID)
            : base(groupID)
        {
        }

        override public bool NotifyIngest(int mediaID)
        {
            Logger.Log("NotifyIngest", string.Format("Sending media ingest notification for media '{0}'. GroupId: {1}", mediaID, m_GroupID));

            Dictionary<string, string> mediaCoGuidEntryId = Utils.GetMediaCoGuidEntryId(mediaID);

            if (mediaCoGuidEntryId == null || mediaCoGuidEntryId.Count() == 0)
            {
                Logger.Log("NotifyIngest", string.Format("ERROR> Couldn't find media with id '{0}'", mediaID));
                return false;
            }

            // Get request URL
            // http://[HostIP]:8080/syncer_v2/[Version]/apiService/notificationFromTvm
            // http://52.74.22.121:8080/syncer_v2/v1.0/apiService/notificationFromTvm
            string notificationUrlKey = "NotificationURL." + m_GroupID;
            string endpointURL = ps_mediahub_proxy.Utils.GetTcmConfigValue(notificationUrlKey);

            if (string.IsNullOrEmpty(endpointURL))
            {
                Logger.Log("NotifyIngest",
                    string.Format("ERROR> Missing configuration in TCM (key {0}), GroupID: {1}", notificationUrlKey, m_GroupID));

                return false;
            }

            // Build request body
            string mediaCoGuid = mediaCoGuidEntryId.Keys.First();
            string mediaEntryId = mediaCoGuidEntryId.Values.First();

            string endpointContent =
                string.Format(
                    "{{\"packageId\":\"{0}\", " +
                    "\"kalturaId\":\"{1}\", " +
                    "\"mediaId\":\"{2}\"}}",
                    mediaCoGuid, mediaEntryId, mediaID);

            string response = sendMediaIngestNotification(endpointURL, endpointContent);

            var jss = new JavaScriptSerializer();
            var statusJson = jss.Deserialize<dynamic>(response);
            string status = string.Empty;

            try
            {
                status = statusJson["Status"];
            }
            catch
            {
            }

            bool bIngestNotificationStatus = false;
            if (status.ToLower().Equals("ok"))
            {
                Logger.Log("NotifyIngest",
                    string.Format("Media ingest notification was successful. media: '{0}', GroupId: {1}", mediaID, m_GroupID));
                bIngestNotificationStatus = true;
            }
            else
            {
                Logger.Log("NotifyIngest",
                    string.Format("ERROR> Media ingest notification failed. media: '{0}', GroupId: {1}, Response: {2}", mediaID, m_GroupID, response));
            }

            Logger.Log("NotifyIngest", string.Format("Finished sending media ingest notification for media '{0}'. GroupId: {1}", mediaID, m_GroupID));

            return bIngestNotificationStatus;
        }

        private string sendMediaIngestNotification(string URL, string content)
        {
            string response = string.Empty;

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    Logger.Log("sendMediaIngestNotification", string.Format("Sending media ingest notification. URL: [{0}], request content: [{1}]", URL, content));

                    streamWriter.Write(content);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }
            }
            catch (Exception exp)
            {
                Logger.Log("sendMediaIngestNotification",
                    string.Format("ERROR> Failed to send media ingest notification. URL: [{0}], request content: [{1}], Error: {2}", URL, content, exp));
            }

            return response;
        }
    }
}
