using ApiObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Users
{
    public static class MailFactory
    {
        public static AddDeviceMailRequest GetAddDeviceMailRequest(int nGroupID, string sMasterFirstName, string sMasterUsername, string sMasterEmail, string sDeviceUdid, string sDeviceName, string sActivationToken)
        {
            AddDeviceMailRequest retVal = null;
             string key = string.Format("users_GetAddDeviceMailRequest_{0}", nGroupID);
             bool bRes = UsersCache.GetItem<AddDeviceMailRequest>(key, out retVal);
            if (!bRes)
            {
                DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(nGroupID);

                if (dvMailParameters != null)
                {
                    retVal = new AddDeviceMailRequest();

                    retVal.m_sTemplateName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "DEVICE_REQUEST_MAIL");
                    retVal.m_sSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "DEVICE_REQUEST_MAIL_SUBJECT");
                    retVal.m_sSenderFrom = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_ADD");
                    retVal.m_sSenderName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_NAME");
                    retVal.m_eMailType = eMailTemplateType.AddDeviceToDomain;

                    UsersCache.AddItem(key, retVal);
                }
            }
            // fill specific details for user
            if (retVal != null)
            {
                retVal.m_sSenderTo = sMasterEmail;
                retVal.m_sMasterUsername = sMasterUsername;
                retVal.m_sFirstName = sMasterFirstName;
                retVal.m_sNewDeviceUdid = sDeviceUdid;
                retVal.m_sNewDeviceName = sDeviceName;
                retVal.m_sToken = sActivationToken;
            }

            return retVal;
        }

        public static AddUserMailRequest GetAddUserMailRequest(int nGroupID, string sMasterFirstName, string sMasterUsername, string sMasterUserEmail, string sNewUsername, string sNewFirstName, string sActivationToken)
        {
            AddUserMailRequest retVal = null;
            string key = string.Format("users_GetAddUserMailRequest_{0}", nGroupID);
            bool bRes = UsersCache.GetItem<AddUserMailRequest>(key, out retVal);
            if (!bRes)
            {
                DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(nGroupID);
                if (dvMailParameters != null)
                {
                    retVal = new AddUserMailRequest();
                    retVal.m_sTemplateName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "ACTIVATION_MAIL");
                    retVal.m_sSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "ACTIVATION_MAIL_SUBJECT");
                    retVal.m_sSenderFrom = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_ADD");
                    retVal.m_sSenderName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_NAME");

                    retVal.m_eMailType = eMailTemplateType.AddUserToDomain;
                    UsersCache.AddItem(key, retVal);
                }
            }

            // fill specific details for user
            if (retVal != null)
            {
                retVal.m_sSenderTo = sMasterUserEmail;
                retVal.m_sMasterUsername = sMasterUsername;
                retVal.m_sFirstName = sMasterFirstName;
                retVal.m_sNewFirstName = sNewFirstName;
                retVal.m_sNewUsername = sNewUsername;
                retVal.m_sToken = sActivationToken;
            }
            
            return retVal;
        }

        public static WelcomeMailRequest GetWelcomeMailRequest(int nGroupID, string sMasterFirstName, string sNewUsername, string sNewFirstName, string sEmail)
        {
            WelcomeMailRequest retVal = null;

            string key = string.Format("users_GetWelcomeMailRequest_{0}", nGroupID);
            bool bRes = UsersCache.GetItem<WelcomeMailRequest>(key, out retVal);
            if (!bRes)
            {
                DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(nGroupID);

                if (dvMailParameters != null)
                {
                    retVal = new WelcomeMailRequest();

                    retVal.m_sTemplateName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_MAIL");
                    retVal.m_sSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_MAIL_SUBJECT");
                    retVal.m_sSenderFrom = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_ADD");
                    retVal.m_sSenderName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_NAME");
                    retVal.m_eMailType = eMailTemplateType.Welcome;

                    UsersCache.AddItem(key, retVal);
                }
            }

            // fill specific details for user
            if (retVal != null)
            {
                retVal.m_sFirstName = sMasterFirstName;
                retVal.m_sLastName = string.Empty;
                retVal.m_sSenderTo = sEmail;
                retVal.m_sUsername = sNewUsername;

                retVal.m_sToken = DAL.UsersDal.GetActivationToken(nGroupID, sNewUsername);
            }

            return retVal;
        }
    }
}
