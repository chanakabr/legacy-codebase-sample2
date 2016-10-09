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

        private enum DMSCall
        {
            POST,
            PUT
        }

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Configuration Group
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

            DMSGroupConfigurationResponse groupConfigurationGetResponse = JsonConvert.DeserializeObject<DMSGroupConfigurationResponse>(result);

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
                configurationGroup.PartnerId = partnerId;
                DMSGroupConfiguration dmsGroupConfiguration = Mapper.Map<DMSGroupConfiguration>(configurationGroup);

                // call client
                dmsResult = CallDMSClient(DMSCall.POST, url, dmsGroupConfiguration);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddConfigurationGroup. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSGroupConfigurationResponse response = JsonConvert.DeserializeObject<DMSGroupConfigurationResponse>(dmsResult);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            configurationGroup = Mapper.Map<KalturaConfigurationGroup>(response.GroupConfiguration);

            return configurationGroup;
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
                dmsGroupConfiguration.PartnerId = partnerId;
                // call client               
                dmsResult = CallDMSClient(DMSCall.PUT, url, dmsGroupConfiguration);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while update ConfigurationGroup. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSStatusResponse statusResponse = JsonConvert.DeserializeObject<DMSStatusResponse>(dmsResult);

            DMSGroupConfigurationResponse response = JsonConvert.DeserializeObject<DMSGroupConfigurationResponse>(dmsResult);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            configurationGroup = Mapper.Map<KalturaConfigurationGroup>(response.GroupConfiguration);

            return configurationGroup;
        }

        internal static bool DeleteConfigurationGroup(int partnerId, string groupId)
        {
            KalturaConfigurationGroup result = null;

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

            return true;
        }

        #endregion

        #region DMs Calls
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

        private static string CallGetDMSClient(string url, string action = "api")
        {
            string result = string.Empty;

            string dmsServer = TCMClient.Settings.Instance.GetValue<string>("dms_url");
            if (string.IsNullOrWhiteSpace(dmsServer))
            {
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "dms_url");
            }
            var dmsRestUrl = string.Format("{0}/{1}/{2}", dmsServer, action, url);

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

        private static string CallDMSClient(DMSCall dmsCall, string url, Object data)
        {
            string result = string.Empty;

            string dmsServer = TCMClient.Settings.Instance.GetValue<string>("dms_url");
            if (string.IsNullOrWhiteSpace(dmsServer))
            {
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "dms_url");
            }
            var dmsRestUrl = string.Format("{0}/api/{1}", dmsServer, url);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(dmsRestUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = dmsCall.ToString();

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(data);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }
        #endregion

        #region Configuration Group
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

            DMSTagGetResponse dmsTagGetResponse = JsonConvert.DeserializeObject<DMSTagGetResponse>(result);

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
        #endregion

        internal static KalturaConfiguration GetConfiguration(int partnerId, string applicationName, string configurationVersion, string platform, string UDID, string tag)
        {
            string result = string.Empty;
            KalturaConfiguration configurationGroup = null;
            string url = string.Format("getconfig?username=dms&password=tvinci&appname={0}&cver={1}&platform={2}&udid={3}&partnerId={4}&tag={5}",
                applicationName, configurationVersion, platform, UDID, partnerId, tag);

            try
            {
                // call client
                result = CallGetDMSClient(url,"v2");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration. partnerId: {0}, applicationName: {1}, configurationVersion: {2}, platform: {3}, UDID: {4}, tag: {5}, exception: {6}",
                    partnerId, applicationName, configurationVersion, platform, UDID, tag, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSGetConfigResponse response = JsonConvert.DeserializeObject<DMSGetConfigResponse>(result);

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (!(response.Status == DMSeStatus.Success || response.Status == DMSeStatus.Registered))
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Status));
            }

            configurationGroup = Mapper.Map<KalturaConfiguration>(response);

            return configurationGroup;
        }
    }
}