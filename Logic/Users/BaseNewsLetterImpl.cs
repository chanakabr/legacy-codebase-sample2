using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public abstract class BaseNewsLetterImpl
    {
        protected string m_apiKey;
        protected string m_listID;

        public BaseNewsLetterImpl()
        {
        }

        public BaseNewsLetterImpl(string apiKey, string listID)
        {
            m_apiKey = apiKey;
            m_listID = listID;
        }

        public virtual bool IsUserSubscribed(User user)
        {
            return false;
        }

        public abstract bool Subscribe(User user);

        public abstract bool UnSubscribe(User user);

    }
}
