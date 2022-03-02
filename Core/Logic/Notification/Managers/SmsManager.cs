using APILogic.SmsAdapterService;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Notification;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Notification;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;

namespace ApiLogic.Notification.Managers
{
    public class SmsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private ServiceClient _AdapterClient;
        private static object locker = new object();
        private static readonly Lazy<SmsManager> lazy = new Lazy<SmsManager>(() => new SmsManager());
        public static SmsManager Instance { get { return lazy.Value; } }

        public GenericResponse<SmsAdapterProfile> Add(ContextData contextData, SmsAdapterProfile coreObject)
        {
            var response = new GenericResponse<SmsAdapterProfile>();
            try
            {
                response.Object = DAL.NotificationDal.AddSmsAdapter(contextData.GroupId, coreObject, (int)contextData.UserId.Value);

                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.RequestFailed, "Sms adapter Not Added");
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSmsAdapaterInvalidationKey(contextData.GroupId));
                log.Debug($"Adapter cache cleared, sending new configuration to SmsAdapater:[{coreObject.Id}]");
                var adapterClient = SMSAdapterManager.GetSMSAdapterServiceClient(response.Object.AdapterUrl);
                SMSAdapterManager.SetAdapaterConfiguration(adapterClient, response.Object);
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, "Failed Adding Sms adapter");
                log.Error($"Failed Add (SmsAdapter) groupID={contextData.GroupId}, ex:{ex}");
            }

            return response;
        }
        public GenericResponse<SmsAdapterProfile> Update(ContextData contextData, SmsAdapterProfile coreObject)
        {
            var response = new GenericResponse<SmsAdapterProfile>();
            try
            {
                response.Object = DAL.NotificationDal.UpdateSmsAdapter(contextData.GroupId, coreObject, (int)contextData.UserId.Value);

                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.AdapterNotExists, "Sms adapter Not Exists");
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSmsAdapaterInvalidationKey(contextData.GroupId));
                log.Debug($"Adapter cache cleared, sending new configuration to SmsAdapater:[{coreObject.Id}]");
                var adapterClient = SMSAdapterManager.GetSMSAdapterServiceClient(response.Object.AdapterUrl);
                SMSAdapterManager.SetAdapaterConfiguration(adapterClient, response.Object);
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, "Failed updating Sms adapter");
                log.Error($"Failed Update (SmsAdapter) groupID={contextData.GroupId}, ex:{ex}");
            }

            return response;
        }

        public Status Send(int groupId, int userId, SmsAdapter adapter, string message, string phoneNumber, List<ApiObjects.KeyValuePair> keyValuePair)
        {
            var status = new Status(eResponseStatus.Error);

            try
            {
                // get user notification data
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    var _phoneNumber = EngagementManager.GetPhoneNumberFromUser(groupId, userId);
                    if (!_phoneNumber.IsOkStatusCode())
                    {
                        status.Message = "Failed to get user data";
                        return status;
                    }
                    phoneNumber = _phoneNumber.Object;
                }

                log.Debug($"Sending SMS message via adapter Id:{adapter.Id}, url:{adapter.AdapterUrl}, group: {groupId}, message: {message} to phone number: {phoneNumber}");

                var sendSmsRequestModel = new SendSmsRequestModel()
                {
                    Message = message,
                    PhoneNumber = phoneNumber,
                    UserId = userId,
                    AdapterData = keyValuePair?.Select(x => new KeyValue { Key = x.key, Value = x.value }).ToArray()
                };

                _AdapterClient = SMSAdapterManager.GetSMSAdapterServiceClient(adapter.AdapterUrl);
                var result = _AdapterClient.SendAsync(adapter.Id.Value, groupId, sendSmsRequestModel).ExecuteAndWait();
                status.Set(result ? eResponseStatus.OK : eResponseStatus.Error, "Failed sending sms");
            }
            catch (Exception ex)
            {
                log.Error($"Error while trying to send SMS. ex {ex}");
                status.Set(eResponseStatus.Error, "Error while trying to send SMS");
            }

            return status;
        }

        public SmsAdapter GetDefaultAdapter(int groupId)
        {
            var adapters = GetSmsAdapters(groupId).SmsAdapters;
            if (adapters != null)
            {
                var defaultAdapter = adapters.FirstOrDefault(x => !string.IsNullOrEmpty(x.AdapterUrl));
                return defaultAdapter;
            }

            return null;
        }

        public SmsAdaptersResponse GetSmsAdapters(int groupId)
        {
            var response = new SmsAdaptersResponse();
            try
            {
                IEnumerable<SmsAdapter> adapters = null;
                var key = LayeredCacheKeys.GetSmsAdapaterByGroupKey(groupId);
                var cacheResult = LayeredCache.Instance.Get(
                    key,
                    ref adapters,
                    GetSmsAdapaterByGroupId,
                    new Dictionary<string, object>() { { "groupId", groupId } },
                    groupId,
                    LayeredCacheConfigNames.GET_SMS_ADAPATER_BY_GROUP_ID_CACHE_CONFIG_NAME,
                    new List<string>() { LayeredCacheKeys.GetSmsAdapaterInvalidationKey(groupId) });

                response.SmsAdapters = adapters;
                if (!cacheResult)
                {
                    response.RespStatus = new Status((int)eResponseStatus.Error, "Could not get sms adapters");
                }
                else if (response.SmsAdapters == null || !response.SmsAdapters.Any())
                {
                    response.RespStatus = new Status((int)eResponseStatus.OK, "no sms adapters related to group");
                }
                else
                {
                    response.RespStatus = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed groupID = {groupId}, ex:{ex}");
            }

            return response;
        }

        private static Tuple<IEnumerable<SmsAdapter>, bool> GetSmsAdapaterByGroupId(Dictionary<string, object> arg)
        {
            try
            {
                var groupId = (int)arg["groupId"];
                var adapter = DAL.NotificationDal.GetSmsAdapters(groupId);
                return new Tuple<IEnumerable<SmsAdapter>, bool>(adapter, true);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get Sms Adapter from DB group:[{arg["groupId"]}], ex: {ex}");
                return new Tuple<IEnumerable<SmsAdapter>, bool>(Enumerable.Empty<SmsAdapter>(), false);
            }
        }

        public SmsAdaptersResponse SetSmsAdapterSharedSecret(int groupId, int adapterId, string sharedSecret, int updaterId)
        {
            var response = new SmsAdaptersResponse { RespStatus = new Status((int)eResponseStatus.Error, "Could not generate shared secret.") };
            try
            {
                if (adapterId == 0)
                {
                    response.RespStatus = new Status(eResponseStatus.IdentifierRequired, "Sms adapter Id required");
                    return response;
                }

                var _response = DAL.NotificationDal.SetSharedSecret(groupId, adapterId, sharedSecret, updaterId);
                response.SmsAdapters = new List<SmsAdapter> { _response };
                if (response.SmsAdapters != null)
                {
                    response.RespStatus = new Status((int)eResponseStatus.OK);
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSmsAdapaterInvalidationKey(groupId));
                }
                else
                {
                    response.RespStatus = new Status(eResponseStatus.NotExist, "Sms adapter not exists");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed SetSmsAdapterSharedSecret adapterId={0} ex:{1}", adapterId, ex);
            }
            return response;

        }

        public Status Delete(ContextData contextData, long id)
        {
            var response = new Status(eResponseStatus.OK, "Could not delete Sms adapter profile.");
            try
            {
                if (id == 0)
                {
                    response = new Status(eResponseStatus.AdapterIdentifierRequired, "Adapter Identifier Required");
                    return response;
                }

                if (DAL.NotificationDal.DeleteSmsAdapter(contextData.GroupId,(int)id, (int)contextData.UserId))
                {
                    response = new Status((int)eResponseStatus.OK);
                }
                else
                {
                    response = new Status(eResponseStatus.AdapterNotExists, "Sms Adapter Not Exists");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed DeleteSmsAdapter groupID={0}, adapterId={1}, ex:{2}", contextData.GroupId, id, ex);
            }
            finally
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSmsAdapaterInvalidationKey(contextData.GroupId));
            }
            return response;
        }

        public GenericListResponse<SmsAdapterProfile> List(ContextData contextData, SmsAdapterProfileFilter filter)
        {
            var response = new GenericListResponse<SmsAdapterProfile>();
            if (filter == null)
            {
                filter = new SmsAdapterProfileFilter();
            }
            var adapterList = this.GetSmsAdapters(contextData.GroupId);
            if (adapterList != null && filter.Id.HasValue)
            {
                response.Objects = adapterList.SmsAdapters?.Where(adapter => adapter.Id == filter.Id).Select(x => new SmsAdapterProfile
                {
                    AdapterUrl = x.AdapterUrl,
                    ExternalIdentifier = x.ExternalIdentifier,
                    GroupId = x.GroupId,
                    Id = x.Id,
                    IsActive = x.IsActive == 1,
                    Name = x.Name,
                    Settings = x.Settings,
                    SharedSecret = x.SharedSecret
                }).ToList();
                response.SetStatus(eResponseStatus.OK);
            }
            return response;
        }

        public GenericResponse<SmsAdapterProfile> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }
    }
}
