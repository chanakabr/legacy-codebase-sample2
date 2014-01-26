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
            status_code = CRMResponseStatus.UnexpectedError;
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

        public void Initialize(TVPApiModule.Objects.Responses.UserResponseObject userResponseObject)
        {
            //this.response_status = (TVPApiModule.Objects.CRM.ResponseStatus)userResponseObject.m_RespStatus;
            this.user_instance_id = userResponseObject.userInstanceID;

            if (userResponseObject.user != null)
            {
                this.user = new TVPApiModule.Objects.CRM.User();

                if (userResponseObject.user.basicData != null)
                {
                    this.user.basic_data = new TVPApiModule.Objects.CRM.UserBasicData(userResponseObject.user.basicData);  
                }

                this.user.domain_id = userResponseObject.user.domianID;

                if (userResponseObject.user.dynamicData != null)
                {
                    this.user.dynamic_data = new TVPApiModule.Objects.CRM.UserDynamicData();

                    if (userResponseObject.user.dynamicData.userData != null)
                    {
                        List<TVPApiModule.Objects.CRM.UserDynamicDataContainer> temp = new List<TVPApiModule.Objects.CRM.UserDynamicDataContainer>();

                        foreach (var user_data in userResponseObject.user.dynamicData.userData)
                        {
                            TVPApiModule.Objects.CRM.UserDynamicDataContainer userDynamicDataContainer = new TVPApiModule.Objects.CRM.UserDynamicDataContainer();

                            userDynamicDataContainer.data_type = user_data.dataType;
                            userDynamicDataContainer.value = user_data.value;

                            temp.Add(userDynamicDataContainer);
                        }

                        this.user.dynamic_data.user_data = temp.ToArray();
                    }
                }

                this.user.is_domain_master = userResponseObject.user.domainMaster;
                this.user.sso_opertaor_id = userResponseObject.user.ssoOperatorID;
                this.user.user_State = (TVPApiModule.Objects.CRM.UserState)userResponseObject.user.userState;
                this.user.site_guid = userResponseObject.user.siteGUID;
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

