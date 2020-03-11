using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facebook.Session;
using Facebook.Rest;
using System.Configuration;
using Facebook.Schema;
using TVPPro.SiteManager.Manager;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.Services
{
    public class FacebookService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static FacebookUser GetFacebookUser()
        {
            Api m_facebookAPI;
            ConnectSession m_ConnectSession;
            FacebookUser m_FacebookUser = null;

            if (FacebookAuthentication.isConnected())
            {
                try
                {
                    m_ConnectSession = new ConnectSession(TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FacebookConnect.API_Key,
                        TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FacebookConnect.Secret_Key);
                    m_facebookAPI = new Api(m_ConnectSession);

                    if (m_ConnectSession.IsConnected())
                    {
                        user user = m_facebookAPI.Users.GetInfo();

                        m_FacebookUser = new FacebookUser(user.first_name, user.last_name, user.birthday_date, user.pic, user.pic_small,
                        user.pic_big, user.sex, user.uid, user.name, user.proxied_email);

                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error occured in SignIn module GetFacebookUser, Error : {0}", ex.Message);
                }

            }
            return m_FacebookUser;
        }
    }

    public class FacebookUser
    {
        #region Members
        private string m_FirstName;
        private string m_LastName;
        private string m_BirthDate;
        private string m_Pic;
        private string m_PicBig;
        private string m_PicSmall;
        private string m_Gender;
        private long? m_FacebookUserId;
        private string m_Name;
        #endregion Members

        #region Properties
        public string FirstName
        {
            get
            {
                return m_FirstName;
            }
        }

        public string LastName
        {
            get
            {
                return m_LastName;
            }
        }

        public string BirthDate
        {
            get
            {
                return m_BirthDate;
            }
        }

        public string Pic
        {
            get
            {
                return m_Pic;
            }
        }

        public string PicSmall
        {
            get
            {
                return m_PicSmall;
            }
        }

        public string PicBig
        {
            get
            {
                return m_PicBig;
            }
        }

        public string Gender
        {
            get
            {
                return m_Gender;
            }
        }

        public long? FacebookUserId
        {
            get
            {
                return m_FacebookUserId;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        #endregion Properties

        #region Constractor
        public FacebookUser(string UserFirstName, string UserLastName, string UserBirthDate, string UserPic, string UserPicSmall, string UserPicBig,
            string UserGender, long? UserId, string UserName, string Email)
        {
            m_FirstName = UserFirstName;
            m_LastName = UserLastName;
            m_BirthDate = UserBirthDate;
            m_Pic = UserPic;
            m_PicSmall = UserPicSmall;
            m_PicBig = UserPicBig;
            m_Gender = UserGender;
            m_FacebookUserId = UserId;
            m_Name = UserName;
        }
        #endregion Constractor
    }
}
