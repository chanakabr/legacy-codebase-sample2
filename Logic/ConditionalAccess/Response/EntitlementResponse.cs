using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class EntitlementResponse
    {
        #region Data Members

        private string m_sFullLifeCycle;
        private string m_sViewLifeCycle;
        private bool m_bIsOfflinePlayBack;

        #endregion

        #region Properties

        public string FullLifeCycle
        {
            get
            {
                return (m_sFullLifeCycle);
            }
            set
            {
                this.m_sFullLifeCycle = value;
            }
        }

        public string ViewLifeCycle
        {
            get
            {
                return (m_sViewLifeCycle);
            }
            set
            {
                this.m_sViewLifeCycle = value;
            }
        }

        public bool IsOfflinePlayBack
        {
            get
            {
                return (m_bIsOfflinePlayBack);
            }
            set
            {
                this.m_bIsOfflinePlayBack = value;
            }
        }

        #endregion

        #region Ctor

        public EntitlementResponse()
        {
            this.m_sFullLifeCycle = string.Empty;
            this.m_sViewLifeCycle = string.Empty;
        } 

        #endregion
    }
}
