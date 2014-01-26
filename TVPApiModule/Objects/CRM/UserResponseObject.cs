using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.CRM
{
    public class UserResponseObject
    {
        public ResponseStatus response_status;
        public User user;
        public string user_instance_id;

        public UserResponseObject(TVPApiModule.Objects.Responses.UserResponseObject userResponseObject)
        {
            this.response_status = (TVPApiModule.Objects.CRM.ResponseStatus)userResponseObject.respStatus;
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

    }
}
