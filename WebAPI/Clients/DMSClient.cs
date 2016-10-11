using AutoMapper;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
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
            Tag,
            Device,
            Report,
            Configuration
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
            string url = string.Format("{0}/{1}", DMSControllers.GroupConfiguration.ToString(), partnerId);
            string dmsResult = string.Empty;

            try
            {
                configurationGroup.PartnerId = partnerId;
                DMSGroupConfiguration dmsGroupConfiguration = Mapper.Map<DMSGroupConfiguration>(configurationGroup);

                // call client
                string data = JsonConvert.SerializeObject(dmsGroupConfiguration);
                dmsResult = CallDMSClient(DMSCall.POST, url, data);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding configuration group. partnerId: {0}, exception: {1}", partnerId, ex);
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
                //dmsGroupConfiguration.id
                // call client  
                string data = JsonConvert.SerializeObject(dmsGroupConfiguration);
                dmsResult = CallDMSClient(DMSCall.PUT, url, data);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while update ConfigurationGroup. partnerId: {0}, exception: {1}", partnerId, ex);
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

        internal static bool DeleteConfigurationGroup(int partnerId, string groupId)
        {
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

        private static string CallDMSClient(DMSCall dmsCall, string url, string data)
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
                if (string.IsNullOrWhiteSpace(data))
                {
                    streamWriter.Write(string.Empty);
                }
                else
                {
                    streamWriter.Write(data);
                }
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }
        #endregion

        #region Configuration Group Tag
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

            DMSTagResponse dmsTagGetResponse = JsonConvert.DeserializeObject<DMSTagResponse>(result);

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
            string url = string.Format("{0}/List/{1}/{2}", DMSControllers.Tag.ToString(), partnerId, groupId);
            string dmsResult = string.Empty;

            try
            {
                // call client
                dmsResult = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration group tag list. partnerId: {0}, groupId: {1}, exception: {2}", partnerId, groupId, ex);
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

        internal static KalturaConfigurationGroupTag AddConfigurationGroupTag(int partnerId, KalturaConfigurationGroupTag configurationGroupTag)
        {
            string url = string.Format("{0}/{1}/{2}/{3}", DMSControllers.Tag.ToString(), partnerId, configurationGroupTag.ConfigurationGroupId, configurationGroupTag.Tag);
            string dmsResult = string.Empty;

            try
            {
                // call client
                dmsResult = CallDMSClient(DMSCall.POST, url, null);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddConfigurationGroup. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSTagResponse response = JsonConvert.DeserializeObject<DMSTagResponse>(dmsResult);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            configurationGroupTag = Mapper.Map<KalturaConfigurationGroupTag>(response.TagMap);

            return configurationGroupTag;
        }

        internal static bool DeleteConfigurationGroupTag(int partnerId, string configurationGroupId, string tag)
        {
            string url = string.Format("{0}/{1}/{2}/{3}", DMSControllers.Tag.ToString(), partnerId, configurationGroupId, tag);
            string dmsResult = string.Empty;

            try
            {
                // call client               
                dmsResult = CallDeleteDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while delete configuration group tag. partnerId: {0}, configurationGroupId: {1}, tag: {2}, exception: {3}", partnerId, configurationGroupId, tag, ex);
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

        #region Configuration

        internal static string Serve(int partnerId, string applicationName, string clientVersion, string platform, string UDID, string tag)
        {
            string result = string.Empty;
            string url = string.Format("getconfig?username=dms&password=tvinci&appname={0}&cver={1}&platform={2}&udid={3}&partnerId={4}&tag={5}",
                applicationName, clientVersion, platform, UDID, partnerId, tag);

            try
            {
                // call client
                result = CallGetDMSClient(url, "v2");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration. partnerId: {0}, applicationName: {1}, configurationVersion: {2}, platform: {3}, UDID: {4}, tag: {5}, exception: {6}",
                    partnerId, applicationName, clientVersion, platform, UDID, tag, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSGetConfigResponse response = JsonConvert.DeserializeObject<DMSGetConfigResponse>(result);

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (!(response.Status == DMSeStatus.Success || response.Status == DMSeStatus.Registered))
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Status), string.Empty);
            }

            return result;
        }

        internal static KalturaConfiguration GetConfiguration(int partnerId, string configurationId)
        {
            KalturaConfiguration configuration = null;
            string url = string.Format("{0}/{1}/{2}", DMSControllers.Configuration.ToString(), partnerId, configurationId);
            string result = string.Empty;

            try
            {
                // call client
                result = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration. partnerId: {0}, configurationId: {1}, exception: {2}", partnerId, configurationId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSConfigurationResponse response = JsonConvert.DeserializeObject<DMSConfigurationResponse>(result);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            configuration = Mapper.Map<KalturaConfiguration>(response.Configuration);

            return configuration;
        }

        internal static KalturaConfigurationListResponse GetConfigurationList(int partnerId, KalturaConfigurationType configurationType, string configurationId)
        {
            KalturaConfigurationListResponse result = new KalturaConfigurationListResponse() { TotalCount = 0 };

            if (configurationType != KalturaConfigurationType.NotDefault)
            {
                configurationId = "null";
            }
            string url = string.Format("{0}/{1}/{2}/{3}", DMSControllers.Configuration.ToString(), partnerId, configurationType.ToString(), configurationId);
            string dmsResult = string.Empty;

            try
            {
                // call client
                dmsResult = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration list. partnerId: {0}, configurationType: {1}, configurationId: {2}, exception: {3}", partnerId, configurationType.ToString(), configurationId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSConfigurationListResponse response = JsonConvert.DeserializeObject<DMSConfigurationListResponse>(dmsResult);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            if (response.Configurations != null && response.Configurations.Count > 0)
            {
                result.TotalCount = response.Configurations.Count;
                // convert configuration
                result.Objects = Mapper.Map<List<KalturaConfiguration>>(response.Configurations);
            }

            return result;
        }

        internal static KalturaConfiguration AddConfiguration(int partnerId, KalturaConfiguration configuration)
        {
            string url = string.Format("{0}/{1}", DMSControllers.Configuration.ToString(), partnerId);
            string dmsResult = string.Empty;

            try
            {
                configuration.PartnerId = partnerId;
                DMSAppVersion dmsAppVersion = Mapper.Map<DMSAppVersion>(configuration);

                // call client
                string data = JsonConvert.SerializeObject(dmsAppVersion);
                dmsResult = CallDMSClient(DMSCall.POST, url, data);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding configuration. partnerId: {0}, exception: {1}", partnerId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSConfigurationResponse response = JsonConvert.DeserializeObject<DMSConfigurationResponse>(dmsResult);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            configuration = Mapper.Map<KalturaConfiguration>(response.Configuration);

            return configuration;
        }

        internal static KalturaConfiguration UpdateConfiguration(int partnerId, string configurationId, KalturaConfiguration configuration)
        {
            DMSAppVersion dmsAppVersion = null;

            string url = string.Format("{0}/{1}/{2}", DMSControllers.Configuration.ToString(), partnerId, configurationId);
            string dmsResult = string.Empty;

            try
            {
                dmsAppVersion = Mapper.Map<DMSAppVersion>(configuration);
                dmsAppVersion.GroupId = partnerId;
                dmsAppVersion.Id = configurationId;

                // call client  
                string data = JsonConvert.SerializeObject(dmsAppVersion);
                dmsResult = CallDMSClient(DMSCall.PUT, url, data);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating configuration. partnerId: {0}, configurationId: {1}, exception: {2}", partnerId, configurationId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSConfigurationResponse response = JsonConvert.DeserializeObject<DMSConfigurationResponse>(dmsResult);


            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            configuration = Mapper.Map<KalturaConfiguration>(response.Configuration);

            return configuration;
        }

        internal static bool DeleteConfiguration(int partnerId, string id)
        {

            string url = string.Format("{0}/{1}/{2}", DMSControllers.Configuration.ToString(), partnerId, id);
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

            DMSStatusResponse response = JsonConvert.DeserializeObject<DMSStatusResponse>(dmsResult);

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Status), response.Message);
            }

            return true;
        }

        #endregion

        #region Configuration Group Device
        internal static KalturaConfigurationGroupDevice GetConfigurationGroupDevice(int partnerId, string udid)
        {
            KalturaConfigurationGroupDevice configurationGroupDevice = null;
            string url = string.Format("{0}/Get/{1}/{2}", DMSControllers.Device.ToString(), partnerId, udid);
            string result = string.Empty;

            try
            {
                // call client
                result = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration group device . partnerId: {0}, udid: {1}, exception: {2}", partnerId, udid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSDeviceResponse response = JsonConvert.DeserializeObject<DMSDeviceResponse>(result);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            configurationGroupDevice = Mapper.Map<KalturaConfigurationGroupDevice>(response.DeviceMap);

            return configurationGroupDevice;

        }

        internal static KalturaConfigurationGroupDeviceListResponse GetConfigurationGroupDeviceList(int partnerId, string groupId, int pageIndex, int pageSize)
        {
            KalturaConfigurationGroupDeviceListResponse result = new KalturaConfigurationGroupDeviceListResponse() { TotalCount = 0 };
            string url = string.Format("{0}/{1}/{2}/{3}/{4}", DMSControllers.Device.ToString(), partnerId, groupId, pageIndex, pageSize);
            string dmsResult = string.Empty;

            try
            {
                // call client
                dmsResult = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration group device list. partnerId: {0}, groupId: {1}, pageIndex: {2}, pageSize: {3}, exception: {4}", partnerId, groupId, pageIndex, pageSize, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSDeviceListResponse response = JsonConvert.DeserializeObject<DMSDeviceListResponse>(dmsResult);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            if (response.DeviceMapList != null && response.DeviceMapList.Count > 0)
            {
                result.TotalCount = response.DeviceMapList.Count;
                // convert device            
                result.Objects = Mapper.Map<List<KalturaConfigurationGroupDevice>>(response.DeviceMapList);
            }

            return result;
        }

        internal static bool AddConfigurationGroupDevice(int partnerId, string groupId, string udid)
        {
            string url = string.Format("{0}/{1}/{2}", DMSControllers.Device.ToString(), partnerId, groupId);
            string dmsResult = string.Empty;
            List<string> data = new List<string>();

            try
            {
                data.Add(udid);
                // call client
                dmsResult = CallDMSClient(DMSCall.POST, url, JsonConvert.SerializeObject(data));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while add configuration group device. partnerId: {0}, udid: {1}, exception: {2}", partnerId, udid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSStatusResponse response = JsonConvert.DeserializeObject<DMSStatusResponse>(dmsResult);

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Status), response.Message);
            }

            return true;
        }

        internal static bool DeleteConfigurationGroupDevice(int partnerId, string configurationGroupId, string udid)
        {
            string url = string.Format("{0}/{1}/{2}/{3}", DMSControllers.Device.ToString(), partnerId, configurationGroupId, udid);
            string dmsResult = string.Empty;

            try
            {
                // call client               
                dmsResult = CallDeleteDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while delete configuration group device. partnerId: {0}, configurationGroupId: {1}, udid: {2}, exception: {3}", partnerId, configurationGroupId, udid, ex);
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

        #region Report Device
        internal static KalturaReport GetDeviceReport(int partnerId, string udid)
        {
            KalturaReport device = null;
            string url = string.Format("{0}/{1}/{2}", DMSControllers.Report.ToString(), partnerId, udid);
            string result = string.Empty;

            try
            {
                // call client
                result = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting device report. partnerId: {0}, udid: {1}, exception: {2}", partnerId, udid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSReportDeviceGetResponse response = JsonConvert.DeserializeObject<DMSReportDeviceGetResponse>(result);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            device = Mapper.Map<KalturaReport>(response.Device);

            return device;

        }

        internal static KalturaReportListResponse GetDevicesReport(int partnerId, long fromDate, int pageIndex, int pageSize)
        {
            KalturaReportListResponse result = new KalturaReportListResponse() { TotalCount = 0 };
            string url = string.Format("{0}/{1}/{2}/{3}/{4}", DMSControllers.Report.ToString(), partnerId, fromDate, pageIndex, pageSize);
            string dmsResult = string.Empty;

            try
            {
                // call client
                dmsResult = CallGetDMSClient(url);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting configuration group device list. partnerId: {0}, fromDate: {1}, pageIndex: {2}, pageSize: {3}, exception: {4}", partnerId, fromDate, pageIndex, pageSize, ex);
                ErrorUtils.HandleWSException(ex);
            }

            DMSReportDeviceListResponse response = JsonConvert.DeserializeObject<DMSReportDeviceListResponse>(dmsResult);

            if (response == null || response.Result == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Result.Status != DMSeResponseStatus.OK)
            {
                throw new ClientException((int)DMSMapping.ConvertDMSStatus(response.Result.Status), response.Result.Message);
            }

            if (response.DeviceList != null && response.DeviceList.Count > 0)
            {
                result.TotalCount = response.DeviceList.Count;
                // convert kaltura device            
                result.Objects = Mapper.Map<List<KalturaReport>>(response.DeviceList);
            }

            return result;
        }

        #endregion
    }
}