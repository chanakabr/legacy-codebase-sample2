using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iucon.web.Controls
{
    public class PartialUpdatePanelSingleton
    {
        protected static PartialUpdatePanelSingleton m_instance = new PartialUpdatePanelSingleton();
        protected string m_sBaseUrl = string.Empty;

        public static PartialUpdatePanelSingleton Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PartialUpdatePanelSingleton();
                }
                
                return m_instance;
            }
        }

        public string BaseUrl
        {
            get { return m_sBaseUrl; }
            set { m_sBaseUrl = value; }
        }
    }
}
