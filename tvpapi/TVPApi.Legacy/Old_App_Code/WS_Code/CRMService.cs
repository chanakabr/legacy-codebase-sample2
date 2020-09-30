using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using TVPApiModule.Services;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPApiModule.Helper;
using System.Web;
using TVPApiModule.Objects.CRM;
using KLogMonitor;
using System.Reflection;

namespace TVPApiServices
{
    /// <summary>
    /// Summary description for Service
    /// </summary>
    public class CRMService : ICRMService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string m_apiUserName = string.Empty;
        private string m_apiPassword = string.Empty;

        #region C'tor

        public CRMService(string sApiUserName, string sApiPassword)
        {
            m_apiUserName = sApiUserName;
            m_apiPassword = sApiPassword;
        }

        #endregion

        public DummyChargeUserForMediaFileResponse DummyChargeUserForMediaFile(DummyChargeUserForMediaFileRequest request)
        {
            DummyChargeUserForMediaFileResponse response = new DummyChargeUserForMediaFileResponse();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForMediaFile", m_apiUserName, m_apiPassword, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response.result = new ApiConditionalAccessService(groupId, PlatformType.Web).DummyChargeUserForMediaFile(request.price, request.currency, request.file_id, request.ppv_module_code, SiteHelper.GetClientIP(), request.site_guid, request.udid);
                }
                catch (Exception ex)
                {
                    response.status_code = CRMResponseStatus.UnexpectedError;
                    logger.Error("CRMGateway Exception", ex);
                }
            }
            else
            {
                response.status_code = CRMResponseStatus.UnknownGroup;
            }

            return response;
        }

        public DummyChargeUserForSubscriptionResponse DummyChargeUserForSubscription(DummyChargeUserForSubscriptionRequest request)
        {
            DummyChargeUserForSubscriptionResponse response = new DummyChargeUserForSubscriptionResponse();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForSubscription", m_apiUserName, m_apiPassword, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response.result = new ApiConditionalAccessService(groupId, PlatformType.Web).DummyChargeUserForSubscription(request.price, request.currency, request.subscription_id, request.coupon_code, request.user_ip, request.site_guid, request.extra_parameters, request.udid);
                }
                catch (Exception ex)
                {
                    response.status_code = CRMResponseStatus.UnexpectedError;
                    logger.Error("CRMGateway Exception", ex);
                }
            }
            else
            {
                response.status_code = CRMResponseStatus.UnknownGroup;
            }

            return response;
        }

        public GetUserByUsernameResponse GetUserByUsername(GetUserByUsernameRequest request)
        {
            GetUserByUsernameResponse response = new GetUserByUsernameResponse();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserByUsername", m_apiUserName, m_apiPassword, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    var userResponseObject = new ApiUsersService(groupId, PlatformType.Web).GetUserByUsername(request.user_name);

                    response.Initialize(userResponseObject);
                }
                catch (Exception ex)
                {
                    response.status_code = CRMResponseStatus.UnexpectedError;

                    logger.ErrorFormat("CRMGateway Exception, Error Message: {0}", ex.Message);
                }
            }
            else
            {
                response.status_code = CRMResponseStatus.UnknownGroup;
            }

            return response;
        }

        public SearchUsersResponse SearchUsers(SearchUsersRequest request)
        {
            SearchUsersResponse response = new SearchUsersResponse();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SearchUsers", m_apiUserName, m_apiPassword, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    var usersBasicDataTVM = CRMHelper.SearchUsers(groupId, request.text);

                    if (usersBasicDataTVM != null)
                    {
                        response.result = new List<UserBasicDataDTO>();

                        foreach (var userBasicDataTVM in usersBasicDataTVM)
                        {
                            UserBasicDataDTO userResponseObject = UserBasicDataDTO.ConvertToDTO(userBasicDataTVM);

                            response.result.Add(userResponseObject);
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.status_code = CRMResponseStatus.UnexpectedError;
                    logger.Error("CRMGateway Exception", ex);
                }
            }
            else
            {
                response.status_code = CRMResponseStatus.UnknownGroup;
            }

            return response;
        }
    }
}
