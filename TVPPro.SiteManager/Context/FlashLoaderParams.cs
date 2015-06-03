using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Context
{
	public struct FlashLoadersParams
	{
		private string m_language;
		private string m_fileQuality;
		private string m_mainFileFormat;
		private string m_subFileFormat;
		private string m_pic1Size;
		private string m_pic2Size;
		private string m_pic3Size;
		private bool m_isPic1Poster;
		private bool m_isPic2Poster;
        private string m_UserIP;
        //private string m_AdminToken;        
        private string m_SiteGuid;        

        public string UserIP
        {
            get
            {
                if (String.IsNullOrEmpty(m_UserIP))
                    m_UserIP = SiteManager.Helper.SiteHelper.GetClientIP();
                return m_UserIP;
            }
            set
            {
                m_UserIP = value;
            }
        }

        private string SiteGuid
        {
            get
            {
                return m_SiteGuid;
            }
            set
            {
                m_SiteGuid = value;
            }
        }


		public string MainFileFormat
		{
			get
			{
				return m_mainFileFormat;
			}
			set
			{
				m_mainFileFormat = value;
			}
		}

		public string SubFileFormat
		{
			get
			{
				return m_subFileFormat;
			}
			set
			{
				m_subFileFormat = value;
			}
		}

		public string Pic1Size
		{
			get
			{
				return m_pic1Size;
			}
			set
			{
				m_pic1Size = value;
			}
		}

		public string Pic2Size
		{
			get
			{
				return m_pic2Size;
			}
			set
			{
				m_pic2Size = value;
			}
		}

		public string Pic3Size
		{
			get
			{
				return m_pic3Size;
			}
			set
			{
				m_pic3Size = value;
			}
		}

		public bool IsPic1Poster
		{
			get
			{
				return m_isPic1Poster;
			}
			set
			{
				m_isPic1Poster = value;
			}
		}

		public bool IsPic2Poster
		{
			get
			{
				return m_isPic2Poster;
			}
			set
			{
				m_isPic2Poster = value;
			}
		}

		public string Language
		{
			get
			{
				return m_language;
			}
			set
			{
				m_language = value;
			}
		}

		public string FileQuality
		{
			get
			{
				return m_fileQuality;
			}
			set
			{
				m_fileQuality = value;
			}
		}
	}
}
