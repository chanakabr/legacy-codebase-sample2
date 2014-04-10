using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Users
{
    public static class MailFactory
    {
        public static TvinciAPI.AddDeviceMailRequest GetAddDeviceMailRequest(int nGroupID, string sMasterFirstName, string sMasterUsername, string sMasterEmail, string sDeviceUdid, string sDeviceName, string sActivationToken)
        {
            TvinciAPI.AddDeviceMailRequest retVal = null;

            DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(nGroupID);

            if (dvMailParameters != null)
            {
                retVal = new TvinciAPI.AddDeviceMailRequest();

                object oAddUserMail        = dvMailParameters["DEVICE_REQUEST_MAIL"];
                object oMailFromName       = dvMailParameters["MAIL_FROM_NAME"];
                object oMailFromAdd        = dvMailParameters["MAIL_FROM_ADD"];
                object oAddUserMailSubject = dvMailParameters["DEVICE_REQUEST_MAIL_SUBJECT"];

                if (oAddUserMail != null && oAddUserMail != DBNull.Value)
                {
                    retVal.m_sTemplateName = oAddUserMail.ToString();
                }

                if (oAddUserMailSubject != null && oAddUserMailSubject != DBNull.Value)
                {
                    retVal.m_sSubject = oAddUserMailSubject.ToString();
                }

                if (oMailFromAdd != null && oMailFromAdd != DBNull.Value)
                {
                    retVal.m_sSenderFrom = oMailFromAdd.ToString();
                }
                if (oMailFromName != null && oMailFromName != DBNull.Value)
                {
                    retVal.m_sSenderName = oMailFromName.ToString();
                }

                retVal.m_eMailType = TvinciAPI.eMailTemplateType.AddDeviceToDomain;
                retVal.m_sSenderTo = sMasterEmail;
                retVal.m_sMasterUsername = sMasterUsername;
                retVal.m_sFirstName = sMasterFirstName;
                retVal.m_sNewDeviceUdid = sDeviceUdid;
                retVal.m_sNewDeviceName = sDeviceName;
                retVal.m_sToken = sActivationToken;
            }

            return retVal;
        }

        public static TvinciAPI.AddUserMailRequest GetAddUserMailRequest(int nGroupID, string sMasterFirstName, string sMasterUsername, string sMasterUserEmail, string sNewUsername, string sNewFirstName, string sActivationToken)
        {
            TvinciAPI.AddUserMailRequest retVal = null;

            DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(nGroupID);

            if (dvMailParameters != null)
            {
                retVal = new TvinciAPI.AddUserMailRequest();

                object oAddUserMail = dvMailParameters["ACTIVATION_MAIL"];
                object oMailFromName = dvMailParameters["MAIL_FROM_NAME"];
                object oMailFromAdd = dvMailParameters["MAIL_FROM_ADD"];
                object oAddUserMailSubject = dvMailParameters["ACTIVATION_MAIL_SUBJECT"];

                if (oAddUserMail != null && oAddUserMail != DBNull.Value)
                {
                    retVal.m_sTemplateName = oAddUserMail.ToString();
                }

                if (oAddUserMailSubject != null && oAddUserMailSubject != DBNull.Value)
                {
                    retVal.m_sSubject = oAddUserMailSubject.ToString();
                }

                if (oMailFromAdd != null && oMailFromAdd != DBNull.Value)
                {
                    retVal.m_sSenderFrom = oMailFromAdd.ToString();
                }
                if (oMailFromName != null && oMailFromName != DBNull.Value)
                {
                    retVal.m_sSenderName = oMailFromName.ToString();
                }

                retVal.m_eMailType = TvinciAPI.eMailTemplateType.AddUserToDomain;
                retVal.m_sSenderTo = sMasterUserEmail;
                retVal.m_sMasterUsername = sMasterUsername;
                retVal.m_sFirstName = sMasterFirstName;
                retVal.m_sNewFirstName = sNewFirstName;
                retVal.m_sNewUsername = sNewUsername;
                retVal.m_sToken = sActivationToken;
            }

            return retVal;
        }

        public static TvinciAPI.WelcomeMailRequest GetWelcomeMailRequest(int nGroupID, string sMasterFirstName, string sNewUsername, string sNewFirstName, string sEmail)
        {
            TvinciAPI.WelcomeMailRequest retVal = null;

            DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(nGroupID);

            if (dvMailParameters != null)
            {
                retVal = new TvinciAPI.WelcomeMailRequest();

                object oWelcomeMail = dvMailParameters["WELCOME_MAIL"];
                object oMailFromName = dvMailParameters["MAIL_FROM_NAME"];
                object oMailFromAdd = dvMailParameters["MAIL_FROM_ADD"];
                object oWelcomMailSubject = dvMailParameters["WELCOME_MAIL_SUBJECT"];

                if (oWelcomeMail != null && oWelcomeMail != DBNull.Value)
                {
                    retVal.m_sTemplateName = oWelcomeMail.ToString();
                }
                if (oWelcomMailSubject != null && oWelcomMailSubject != DBNull.Value)
                {
                    retVal.m_sSubject = oWelcomMailSubject.ToString();
                }
                if (oMailFromAdd != null && oMailFromAdd != DBNull.Value)
                {
                    retVal.m_sSenderFrom = oMailFromAdd.ToString();
                }
                if (oMailFromName != null && oMailFromName != DBNull.Value)
                {
                    retVal.m_sSenderName = oMailFromName.ToString();
                }

                retVal.m_eMailType = TvinciAPI.eMailTemplateType.Welcome;
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
