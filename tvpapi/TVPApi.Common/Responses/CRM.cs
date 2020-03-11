using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.Users;
using Newtonsoft.Json;
using TVPApiModule.Objects.CRM;

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


        #endregion

        #region Properties

        /// <remarks/>
        public UserDTO user;


        /// <remarks/>
        public string user_instance_id;
        
        #endregion

        #region Public Methods

        public void Initialize(Core.Users.UserResponseObject userResponseObject)
        {
            this.user_instance_id = userResponseObject.m_userInstanceID;
            this.user = UserDTO.ConvertToDTO(userResponseObject.m_user);
            
        }

        #endregion
    }

    public class SearchUsersResponse : CRMResponse
    {
        public List<TVPApiModule.Objects.CRM.UserBasicDataDTO> result;
    }

    #endregion

}

