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
            this.response_status = (TVPApiModule.Objects.CRM.ResponseStatus)userResponseObject.resp_status;
            this.user_instance_id = userResponseObject.user_instance_id;

            if (userResponseObject.user != null)
            {
                this.user = new TVPApiModule.Objects.CRM.User();

                if (userResponseObject.user.basic_data != null)
                {
                    this.user.basic_data = new TVPApiModule.Objects.CRM.UserBasicData(userResponseObject.user.basic_data);
                }

                this.user.domain_id = userResponseObject.user.domain_id;

                if (userResponseObject.user.dynamic_data != null)
                {
                    this.user.dynamic_data = new TVPApiModule.Objects.CRM.UserDynamicData();

                    if (userResponseObject.user.dynamic_data.user_data != null)
                    {
                        List<TVPApiModule.Objects.CRM.UserDynamicDataContainer> temp = new List<TVPApiModule.Objects.CRM.UserDynamicDataContainer>();

                        foreach (var user_data in userResponseObject.user.dynamic_data.user_data)
                        {
                            TVPApiModule.Objects.CRM.UserDynamicDataContainer userDynamicDataContainer = new TVPApiModule.Objects.CRM.UserDynamicDataContainer();

                            userDynamicDataContainer.data_type = user_data.data_type;
                            userDynamicDataContainer.value = user_data.value;

                            temp.Add(userDynamicDataContainer);
                        }

                        this.user.dynamic_data.user_data = temp.ToArray();
                    }
                }

                this.user.is_domain_master = userResponseObject.user.is_domain_master;
                this.user.sso_opertaor_id = userResponseObject.user.sso_operator_id;
                this.user.user_State = (TVPApiModule.Objects.CRM.UserState)userResponseObject.user.user_state;
                this.user.site_guid = userResponseObject.user.site_guid;
            }
        }

    }
}
