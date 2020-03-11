using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Comment
/// </summary>
/// 
namespace TVPApi
{
    public struct Comment
    {
        private string m_author;

        public string Author
        {
            get { return m_author; }
            set { m_author = value; }
        }
        private string m_header;

        public string Header
        {
            get { return m_header; }
            set { m_header = value; }
        }
        private string m_addedDate;

        public string AddedDate
        {
            get { return m_addedDate; }
            set { m_addedDate = value; }
        }

        private string m_content;

        public string Content
        {
            get { return m_content; }
            set { m_content = value; }
        }

        private string m_userPicURL;

        public string UserPicURL
        {
            get { return m_userPicURL; }
            set { m_userPicURL = value; }
        }

        public Comment(string author, string header, string addedDate, string content, string userPicURL)
        {
            m_author = author;
            m_header = header;
            m_addedDate = addedDate;
            m_content = content;
            m_userPicURL = userPicURL;
        }
    }
}
