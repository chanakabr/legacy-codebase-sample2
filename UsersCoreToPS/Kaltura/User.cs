using ApiObjects;
using ApiObjects.Response;
using Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaltura
{    
    public class User : KalturaUsers
    {
        public User(int groupId)
            : base(groupId)
        {
            // activate/deactivate user features
            this.ShouldSubscribeNewsLetter = false;
            this.ShouldCreateDefaultRules = false;
            this.ShouldSendWelcomeMail = true;
        }



        // SignIn                                    
        public override UserResponseObject PreSignIn(ref Int32 siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId,
            ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {

            if (keyValueList.Where(x => x.key == "usePSDLL" && x.value == "true").Count() == 0)
            {
                return new UserResponseObject();
            }
            
            UserResponse user = Core.Users.Module.GetUserByName(this.GroupId, userName);

            if (user != null && user.user != null && user.resp.Code == (int)eResponseStatus.OK) // user already exsits
            {
                return user.user;
            }

            if (user.resp.Code == (int)eResponseStatus.UserDoesNotExist)
            {
                UserBasicData userBasicData = new UserBasicData()
                {
                    m_sFirstName = userName,
                    m_sUserName = userName,
                    m_sPassword = password,
                    m_sEmail = userName
                };
                UserDynamicData userDynamicData = new UserDynamicData();
                userDynamicData.m_sUserData = new UserDynamicDataContainer[1];
                userDynamicData.m_sUserData[0] = new UserDynamicDataContainer() {
                m_sDataType = "newsletter",
                m_sValue = "false"};
            

                UserResponse userResponse = Core.Users.Module.SignUp(this.GroupId, userBasicData, userDynamicData, password, string.Empty);
                if (userResponse != null && userResponse.resp != null && userResponse.resp.Code == (int)eResponseStatus.OK)
                {
                    // create pin code                     
                    try
                    {
                        PinCodeResponse pinResponse = Core.Users.Module.GenerateLoginPIN(this.GroupId, userResponse.user.m_user.m_sSiteGUID, string.Empty);
                        if (pinResponse != null && pinResponse.resp != null && pinResponse.resp.Code == (int)eResponseStatus.OK)
                        {
                            // send mail
                            ApiObjects.DynamicMailRequest request = new ApiObjects.DynamicMailRequest();
                            request.m_emailKey = ODBCWrapper.Utils.GetSafeStr(ODBCWrapper.Utils.GetTableSingleVal("groups", "mail_settings", "id", "=", this.GroupId, "MAIN_CONNECTION_STRING")); // get email key from group table
                            request.m_sFirstName = userName;
                            request.m_sLastName = userName;
                            request.m_sSenderFrom = "noreply@kaltura.com"; // get from group table sender_mail;
                            request.m_sSenderName = "Kaltura";
                            request.m_sSenderTo = userName;
                            request.m_sSubject = "test pin code generate mail for PS flow";
                            request.m_sTemplateName = "welcome_203.html"; // get this from where 
                            
                            request.values = new List<KeyValuePair>();
                            request.values.Add(new KeyValuePair() { key = "PASSWORD", value = "password" });
                            request.values.Add(new KeyValuePair() { key = "TOKEN", value = "977d03b4-55f0-4961-a5e2-3154ea165251" });
                            request.values.Add(new KeyValuePair() { key = "USERNAME", value = userName });
                            

                            bool retVal = Core.Api.Module.SendMailTemplate(this.GroupId, request);
                        }
                    }
                    catch (Exception ex)
                    {
                        //log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
                        //ErrorUtils.HandleWSException(ex);
                    }

                    return userResponse.user;
                }
            }
            return new UserResponseObject()
            {
                m_RespStatus = ResponseStatus.ExternalError,
                m_user = null
            };
        }

    }
}
