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
        private string m_header;
        private string m_addedDate;
        private string m_content;

        public Comment(string author, string header, string addedDate, string content)
        {
            m_author = author;
            m_header = header;
            m_addedDate = addedDate;
            m_content = content;
        }
    }
}
