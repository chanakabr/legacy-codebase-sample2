using ApiLogic.Notification.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using Phx.Lib.Log;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("smsAdapterProfile")]
    public class SmsAdapterProfileController : IKalturaController
    {
        /// <summary>
        /// Generate Sms Adapter shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Sms Adapter id required = 2058, sms adapater not exist = 2056
        /// </remarks>
        /// <param name="smsAdapterId">Sms Adapter identifier</param>
        [Action("generateSharedSecret")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.IdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.NotExist)]
        static public KalturaSmsAdapterProfile GenerateSharedSecret(int smsAdapterId)
        {
            KalturaSmsAdapterProfile response = null;
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId;

            try
            {
                response = ClientsManager.NotificationClient().GenerateSmsAdapaterSharedSecret(groupId, smsAdapterId, int.Parse(userId));
                if (response == null) { throw new ClientException((int)eResponseStatus.AdapterNotExists); }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// SmsAdapterProfile add
        /// </summary>
        /// <param name="objectToAdd">SmsAdapterProfile details</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RequestFailed)]
        static public KalturaSmsAdapterProfile Add(KalturaSmsAdapterProfile objectToAdd)
        {
            var contextData = KS.GetContextData();
            objectToAdd.ValidateForAdd();

            Func<SmsAdapterProfile, GenericResponse<SmsAdapterProfile>> addFunc = (SmsAdapterProfile smsAdapterProfile) =>
                SmsManager.Instance.Add(contextData, smsAdapterProfile);

            var response = ClientUtils.GetResponseFromWS(objectToAdd, addFunc);
            return response;
        }

        /// <summary>
        /// SmsAdapterProfile update
        /// </summary>
        /// <param name="id">SmsAdapterProfile identifier</param>
        /// <param name="objectToUpdate">SmsAdapterProfile details</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public KalturaSmsAdapterProfile Update(long id, KalturaSmsAdapterProfile objectToUpdate)
        {
            objectToUpdate.Id = id;
            objectToUpdate.ValidateForUpdate();
            var contextData = KS.GetContextData();

            Func<SmsAdapterProfile, GenericResponse<SmsAdapterProfile>> addFunc = (SmsAdapterProfile smsAdapterProfile) =>
                SmsManager.Instance.Update(contextData, smsAdapterProfile);

            var response = ClientUtils.GetResponseFromWS(objectToUpdate, addFunc);
            return response;
        }

        /// <summary>
        /// Remove SmsAdapterProfile
        /// </summary>
        /// <param name="id">SmsAdapterProfile identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.AdapterIdentifierRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => SmsManager.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Get SmsAdapterProfile
        /// </summary>
        /// <param name="id">SmsAdapterProfile identifier</param>
        [Action("get")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaSmsAdapterProfile Get(long id)
        {
            var contextData = KS.GetContextData();
            var response = ClientUtils.GetResponseFromWS<KalturaSmsAdapterProfile, SmsAdapterProfile>(() => SmsManager.Instance.Get(contextData, id));
            return response;
        }

        /// <summary>
        /// Gets all SmsAdapterProfile items
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaSmsAdapterProfileListResponse List(KalturaSmsAdapterProfileFilter filter = null)
        {
            var contextData = KS.GetContextData();
            if (filter == null)
            {
                filter = new KalturaSmsAdapterProfileFilter();
            }

            var result = ListBySmsAdapterProfileFilter(contextData);

            var response = new KalturaSmsAdapterProfileListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }

        private static KalturaGenericListResponse<KalturaSmsAdapterProfile> ListBySmsAdapterProfileFilter(ContextData contextData)
        {
            SmsAdaptersResponse _response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    _response = SmsManager.Instance.GetSmsAdapters(contextData.GroupId);
                }
            }
            catch (Exception ex)
            {
                ErrorUtils.HandleWSException(ex);
            }

            if (_response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!_response.RespStatus.IsOkStatusCode())
            {
                throw new ClientException(_response.RespStatus);
            }

            var smsAdapterProfiles = _response.SmsAdapters.Select(x => new SmsAdapterProfile
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

            var response = new KalturaGenericListResponse<KalturaSmsAdapterProfile>();
            response.Objects = AutoMapper.Mapper.Map<List<KalturaSmsAdapterProfile>>(smsAdapterProfiles);
            response.TotalCount = response.Objects.Count;

            return response;
        }
    }
}