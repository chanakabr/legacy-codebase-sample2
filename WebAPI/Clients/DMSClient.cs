using AutoMapper;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.DMS;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class DMSClient
    {
        private enum DMSControllers
        {
            GroupConfiguration,
            Tag
        }     
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());        

        public static KalturaConfigurationGroup GetConfigurationGroup(int partnerId, string groupId)
        {
            KalturaConfigurationGroup configurationGroup = null;
            string url = string.Format("{0}/{1}/{2}", DMSControllers.GroupConfiguration.ToString(), partnerId, groupId);            
            string result = string.Empty;

            try
            {
                // call client
                result = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetConfigurationGroup. partnerId: {0}, groupId: {1}, exception: {2}", partnerId, groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSGroupConfigurationGetResponse groupConfigurationGetResponse = JsonConvert.DeserializeObject<DMSGroupConfigurationGetResponse>(result);

            if (groupConfigurationGetResponse == null || groupConfigurationGetResponse.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (groupConfigurationGetResponse.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(groupConfigurationGetResponse.Result.Status), groupConfigurationGetResponse.Result.Message);
            }

            configurationGroup = Mapper.Map<KalturaConfigurationGroup>(groupConfigurationGetResponse.GroupConfiguration);

            return configurationGroup;
        }

        internal static KalturaConfigurationGroupListResponse GetConfigurationGroupList(int partnerId)
        {
            KalturaConfigurationGroupListResponse result = new KalturaConfigurationGroupListResponse() { TotalCount = 0 };
            string url = string.Format("{0}/{1}", DMSControllers.GroupConfiguration.ToString(), partnerId);
            string dmsResult = string.Empty;

            try
            {
                // call client
                dmsResult = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetConfigurationGroupList. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSGroupConfigurationListResponse groupConfigurationGetListResponse = JsonConvert.DeserializeObject<DMSGroupConfigurationListResponse>(dmsResult);

            if (groupConfigurationGetListResponse == null || groupConfigurationGetListResponse.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (groupConfigurationGetListResponse.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(groupConfigurationGetListResponse.Result.Status), groupConfigurationGetListResponse.Result.Message);
            }

            if (groupConfigurationGetListResponse.GroupConfigurations != null && groupConfigurationGetListResponse.GroupConfigurations.Count > 0)
            {
                result.TotalCount = groupConfigurationGetListResponse.GroupConfigurations.Count;
                // convert groupConfigurations            
                result.Objects = Mapper.Map<List<KalturaConfigurationGroup>>(groupConfigurationGetListResponse.GroupConfigurations);
            }

            return result;
        }

        internal static KalturaConfigurationGroup AddConfigurationGroup(int partnerId, KalturaConfigurationGroup configurationGroup)
        {
            KalturaConfigurationGroup result = null;

            string url = string.Format("{0}/{1}", DMSControllers.GroupConfiguration.ToString(), partnerId);
            string dmsResult = string.Empty;

            try
            {
                NameValueCollection pairs = new NameValueCollection();
                //TODO: convert configurationGroup to NameValueCollection
                // pairs.Add(configurationGroup.Id.g"id": "myId",

                // call client
                dmsResult = CallPostDMSClient(url, pairs);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddConfigurationGroup. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSStatusResponse statusResponse = JsonConvert.DeserializeObject<DMSStatusResponse>(dmsResult);

            if (statusResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (statusResponse.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(statusResponse.Status), statusResponse.Message);
            }

            result = new KalturaConfigurationGroup() { Id = statusResponse.ID };
            return result;
        }

        internal static KalturaConfigurationGroup UpdateConfigurationGroup(int partnerId, KalturaConfigurationGroup configurationGroup)
        {
            KalturaConfigurationGroup result = null;
            DMSGroupConfiguration dmsGroupConfiguration = null;

            string url = string.Format("{0}/{1}", DMSControllers.GroupConfiguration.ToString(), partnerId);
            string dmsResult = string.Empty;

            try
            {
                dmsGroupConfiguration = Mapper.Map<DMSGroupConfiguration>(configurationGroup);

                // call client               
                var data = JsonConvert.SerializeObject(dmsGroupConfiguration);
                dmsResult = CallPutDMSClient(url, data);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while update ConfigurationGroup. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSStatusResponse statusResponse = JsonConvert.DeserializeObject<DMSStatusResponse>(dmsResult);

            if (statusResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (statusResponse.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(statusResponse.Status), statusResponse.Message);
            }

            result = new KalturaConfigurationGroup() { Id = statusResponse.ID };
            return result;
        }

        internal static KalturaConfigurationGroup DeleteConfigurationGroup(int partnerId, string groupId)
        {
            KalturaConfigurationGroup result = null;
            DMSGroupConfiguration dmsGroupConfiguration = null;

            string url = string.Format("{0}/{1}/{2}", DMSControllers.GroupConfiguration.ToString(), partnerId, groupId);
            string dmsResult = string.Empty;

            try
            {
                // call client               
                dmsResult = CallDeleteDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while update ConfigurationGroup. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSStatusResponse statusResponse = JsonConvert.DeserializeObject<DMSStatusResponse>(dmsResult);

            if (statusResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (statusResponse.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(statusResponse.Status), statusResponse.Message);
            }

            result = new KalturaConfigurationGroup() { Id = statusResponse.ID };
            return result;
        }

        private static string CallDeleteDMSClient(string url)
        {
            string result = string.Empty;

            string dmsServer = TCMClient.Settings.Instance.GetValue<string>("dms_url");
            if (string.IsNullOrWhiteSpace(dmsServer))
            {
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "dms_url");
            }
            var dmsRestUrl = string.Format("{0}/api/{1}", dmsServer, url);

            var request = WebRequest.Create(dmsRestUrl);
            request.Method = "DELETE";

            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private static string CallPutDMSClient(string url, string data)
        {
            string result = string.Empty;

            string dmsServer = TCMClient.Settings.Instance.GetValue<string>("dms_url");
            if (string.IsNullOrWhiteSpace(dmsServer))
            {
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "dms_url");
            }
            var dmsRestUrl = string.Format("{0}/api/{1}", dmsServer, url);

            var request = WebRequest.Create(dmsRestUrl);
            request.Method = "PUT";
            request.ContentType = "application/json; charset=utf-8";
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(data);
            }

            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private static string CallGetDMSClient(string url)
        {
            string result = string.Empty;

            string dmsServer = TCMClient.Settings.Instance.GetValue<string>("dms_url");
            if (string.IsNullOrWhiteSpace(dmsServer))
            {
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "dms_url");
            }
            var dmsRestUrl = string.Format("{0}/api/{1}", dmsServer,url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dmsRestUrl);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }


        private static string CallPostDMSClient(string url, NameValueCollection pairs)
        {
            string result = string.Empty;

            string dmsServer = TCMClient.Settings.Instance.GetValue<string>("dms_url");
            if (string.IsNullOrWhiteSpace(dmsServer))
            {
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "dms_url");
            }
            var dmsRestUrl = string.Format("{0}/api/{1}", dmsServer, url);

            byte[] response = null;
            using (WebClient client = new WebClient())
            {
                response = client.UploadValues(dmsRestUrl,"POST", pairs);
            }

            result = Encoding.ASCII.GetString(response);
            return result;
        }


        internal static KalturaConfigurationGroupTag GetConfigurationGroupTag(int partnerId, string tag)
        {
            KalturaConfigurationGroupTag configurationGroupTag = null;
            string url = string.Format("{0}/{1}/{2}", DMSControllers.Tag.ToString(), partnerId, tag);
            string result = string.Empty;

            try
            {
                // call client
                result = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configurationGroup tag . partnerId: {0}, tag: {1}, exception: {2}", partnerId, tag, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSTagGetResponse dmsTagGetResponse  = JsonConvert.DeserializeObject<DMSTagGetResponse>(result);

            if (dmsTagGetResponse == null || dmsTagGetResponse.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (dmsTagGetResponse.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(dmsTagGetResponse.Result.Status), dmsTagGetResponse.Result.Message);
            }

            configurationGroupTag = Mapper.Map<KalturaConfigurationGroupTag>(dmsTagGetResponse.TagMap);

            return configurationGroupTag;

        }

        internal static KalturaConfigurationGroupTagListResponse GetConfigurationGroupTagList(int partnerId, string groupId)
        {
            KalturaConfigurationGroupTagListResponse result = new KalturaConfigurationGroupTagListResponse() { TotalCount = 0 };
            string url = string.Format("{0}/{1}/{2}", DMSControllers.GroupConfiguration.ToString(), partnerId, groupId);
            string dmsResult = string.Empty;

            try
            {
                // call client
                dmsResult = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration group tag list. partnerId: {0}, partnerId: {1}, exception: {2}", partnerId, groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSTagListResponse dmsTagListResponse = JsonConvert.DeserializeObject<DMSTagListResponse>(dmsResult);

            if (dmsTagListResponse == null || dmsTagListResponse.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (dmsTagListResponse.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(dmsTagListResponse.Result.Status), dmsTagListResponse.Result.Message);
            }

            if (dmsTagListResponse.TagMappingList != null && dmsTagListResponse.TagMappingList.Count > 0)
            {
                result.TotalCount = dmsTagListResponse.TagMappingList.Count;
                // convert tags            
                result.Objects = Mapper.Map<List<KalturaConfigurationGroupTag>>(dmsTagListResponse.TagMappingList);
            }

            return result;
        }
    }
}