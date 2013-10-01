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
        public CRMResponse()
        {
            status_code = CRMResponseStatus.UnexpectedError;
        }

        public CRMResponseStatus status_code;
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

        private TVPApiModule.Objects.CRM.ResponseStatus m_RespStatusField;

        private TVPApiModule.Objects.CRM.User m_userField;

        private string m_userInstanceIDField;

        #endregion

        #region Properties

        /// <remarks/>
        public TVPApiModule.Objects.CRM.ResponseStatus response_status
        {
            get
            {
                return this.m_RespStatusField;
            }
            set
            {
                this.m_RespStatusField = value;
            }
        }

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

        public void Initialize(UserResponseObject userResponseObject)
        {
            this.response_status = (TVPApiModule.Objects.CRM.ResponseStatus)userResponseObject.m_RespStatus;
            this.user_instance_id = userResponseObject.m_userInstanceID;

            if (userResponseObject.m_user != null)
            {
                this.user = new TVPApiModule.Objects.CRM.User();

                if (userResponseObject.m_user.m_oBasicData != null)
                {
                    this.user.basic_data = new TVPApiModule.Objects.CRM.UserBasicData();

                    this.user.basic_data.address = userResponseObject.m_user.m_oBasicData.m_sAddress;
                    this.user.basic_data.affiliate_code = userResponseObject.m_user.m_oBasicData.m_sAffiliateCode;
                    this.user.basic_data.city = userResponseObject.m_user.m_oBasicData.m_sCity;
                    this.user.basic_data.co_guid = userResponseObject.m_user.m_oBasicData.m_CoGuid;

                    if (userResponseObject.m_user.m_oBasicData.m_Country != null)
                    {
                        this.user.basic_data.country = new TVPApiModule.Objects.CRM.Country();

                        this.user.basic_data.country.country_code = userResponseObject.m_user.m_oBasicData.m_Country.m_sCountryCode;
                        this.user.basic_data.country.country_name = userResponseObject.m_user.m_oBasicData.m_Country.m_sCountryName;
                        this.user.basic_data.country.object_id = userResponseObject.m_user.m_oBasicData.m_Country.m_nObjecrtID;
                    }

                    this.user.basic_data.email = userResponseObject.m_user.m_oBasicData.m_sEmail;
                    this.user.basic_data.external_token = userResponseObject.m_user.m_oBasicData.m_ExternalToken;
                    this.user.basic_data.facebook_id = userResponseObject.m_user.m_oBasicData.m_sFacebookID;
                    this.user.basic_data.facebook_image = userResponseObject.m_user.m_oBasicData.m_sFacebookImage;
                    this.user.basic_data.facebook_token = userResponseObject.m_user.m_oBasicData.m_sFacebookToken;
                    this.user.basic_data.first_name = userResponseObject.m_user.m_oBasicData.m_sFirstName;
                    this.user.basic_data.is_facebook_image_permitted = userResponseObject.m_user.m_oBasicData.m_bIsFacebookImagePermitted;
                    this.user.basic_data.last_name = userResponseObject.m_user.m_oBasicData.m_sLastName;
                    this.user.basic_data.phone = userResponseObject.m_user.m_oBasicData.m_sPhone;

                    if (userResponseObject.m_user.m_oBasicData.m_State != null)
                    {
                        this.user.basic_data.state = new TVPApiModule.Objects.CRM.State();

                        if (userResponseObject.m_user.m_oBasicData.m_State.m_Country != null)
                        {
                            this.user.basic_data.state.country = new TVPApiModule.Objects.CRM.Country();

                            this.user.basic_data.state.country.country_code = userResponseObject.m_user.m_oBasicData.m_State.m_Country.m_sCountryCode;
                            this.user.basic_data.state.country.country_name = userResponseObject.m_user.m_oBasicData.m_State.m_Country.m_sCountryName;
                            this.user.basic_data.state.country.object_id = userResponseObject.m_user.m_oBasicData.m_State.m_Country.m_nObjecrtID;
                        }

                        this.user.basic_data.state.object_id = userResponseObject.m_user.m_oBasicData.m_State.m_nObjecrtID;
                        this.user.basic_data.state.state_code = userResponseObject.m_user.m_oBasicData.m_State.m_sStateCode;
                        this.user.basic_data.state.state_name = userResponseObject.m_user.m_oBasicData.m_State.m_sStateName;
                    }

                    this.user.basic_data.user_name = userResponseObject.m_user.m_oBasicData.m_sUserName;

                    if (userResponseObject.m_user.m_oBasicData.m_UserType != null)
                    {
                        this.user.basic_data.user_type = new TVPApiModule.Objects.CRM.UserType();

                        this.user.basic_data.user_type.description = userResponseObject.m_user.m_oBasicData.m_UserType.Description;
                        this.user.basic_data.user_type.id = userResponseObject.m_user.m_oBasicData.m_UserType.ID;
                        this.user.basic_data.user_type.is_default = userResponseObject.m_user.m_oBasicData.m_UserType.IsDefault;
                    }

                    this.user.basic_data.zip = userResponseObject.m_user.m_oBasicData.m_sZip;
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

    #endregion

}

