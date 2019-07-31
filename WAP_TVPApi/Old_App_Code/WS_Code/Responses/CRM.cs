using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using TVPApiModule.Objects.CRM;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiServices
{

    #region Responses

    public class CRMResponse
    {
        public CRMResponseStatus status_code;

        public CRMResponse()
        {
            status_code = CRMResponseStatus.OK;
        }
    }

    public class DummyChargeUserForMediaFileResponse : CRMResponse
    {
        public string result = string.Empty;
    }

    public class DummyChargeUserForSubscriptionResponse : CRMResponse
    {
        public string result = string.Empty;
    }

    public class GetUserByUsernameResponse : CRMResponse
    {
        #region Private Members

        private TVPApiModule.Objects.CRM.User m_userField;

        private string m_userInstanceIDField;

        #endregion

        #region Properties

        /// <remarks/>
        public TVPApiModule.Objects.CRM.User user
        {
            get
            {
                return this.m_userField;
            }
            set
            {
                this.m_userField = value;
            }
        }

        /// <remarks/>
        public string user_instance_id
        {
            get
            {
                return this.m_userInstanceIDField;
            }
            set
            {
                this.m_userInstanceIDField = value;
            }
        }

        #endregion

        #region Public Methods

        public void Initialize(TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject userResponseObject)
        {
            //this.response_status = (TVPApiModule.Objects.CRM.ResponseStatus)userResponseObject.m_RespStatus;
            this.user_instance_id = userResponseObject.m_userInstanceID;

            if (userResponseObject.m_user != null)
            {
                this.user = new TVPApiModule.Objects.CRM.User();

                if (userResponseObject.m_user.m_oBasicData != null)
                {
                    this.user.basic_data = new TVPApiModule.Objects.CRM.UserBasicData(userResponseObject.m_user.m_oBasicData);  
                }

                this.user.domain_id = userResponseObject.m_user.m_domianID;

                if (userResponseObject.m_user.m_oDynamicData != null)
                {
                    this.user.dynamic_data = new TVPApiModule.Objects.CRM.UserDynamicData();

                    if (userResponseObject.m_user.m_oDynamicData.m_sUserData != null)
                    {
                        List<TVPApiModule.Objects.CRM.UserDynamicDataContainer> temp = new List<TVPApiModule.Objects.CRM.UserDynamicDataContainer>();

                        foreach (var user_data in userResponseObject.m_user.m_oDynamicData.m_sUserData)
                        {
                            TVPApiModule.Objects.CRM.UserDynamicDataContainer userDynamicDataContainer = new TVPApiModule.Objects.CRM.UserDynamicDataContainer();

                            userDynamicDataContainer.data_type = user_data.m_sDataType;
                            userDynamicDataContainer.value = user_data.m_sValue;

                            temp.Add(userDynamicDataContainer);
                        }

                        this.user.dynamic_data.user_data = temp.ToArray();
                    }
                }

                this.user.is_domain_master = userResponseObject.m_user.m_isDomainMaster;
                this.user.sso_opertaor_id = userResponseObject.m_user.m_nSSOOperatorID;
                this.user.user_State = (TVPApiModule.Objects.CRM.UserState)userResponseObject.m_user.m_eUserState;
                this.user.site_guid = userResponseObject.m_user.m_sSiteGUID;
            }
        }

        #endregion
    }

    public class SearchUsersResponse : CRMResponse
    {
        public List<TVPApiModule.Objects.CRM.UserBasicData> result;
    }

    #endregion

}

