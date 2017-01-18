using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UserComment
    {
        public UserComment() 
        {
            m_nID = 0;
            m_dDate = DateTime.UtcNow;
            m_sWriter = "";
            m_sHeader = "";
            m_sSubHeader = "";
            m_sContent = "";
            m_nMediaID = 0;
            m_sCommentType = "";
            m_sLanguageFullName = "";
        }

        public void Initialize(Int32 nMediaID, Int32 nID, DateTime dDate, string sWriter, string sHeader, string sSubHeader, string sContent, string sCommentType, string sLanguageFullName)
        {
            m_nID = nID;
            m_dDate = dDate;
            m_sWriter = sWriter;
            m_sHeader = sHeader;
            m_sSubHeader = sSubHeader;
            m_sContent = sContent;
            m_nMediaID = nMediaID;
            m_sCommentType = sCommentType;
            m_sLanguageFullName = sLanguageFullName;
        }

        public Int32 m_nID;
        public Int32 m_nMediaID;
        public DateTime m_dDate;
        public string m_sWriter;
        public string m_sHeader;
        public string m_sSubHeader;
        public string m_sContent;
        public string m_sCommentType;
        public string m_sLanguageFullName;
    }
}
