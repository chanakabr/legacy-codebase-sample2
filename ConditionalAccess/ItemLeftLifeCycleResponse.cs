using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class ItemLeftLifeCycleResponse
    {
        #region Data Members

        private string m_sFullLifeCycle;
        private string m_sViewLifeCycle;
        private bool m_bIsOfflinePlayBack;

        #endregion

        #region Properties

        public string FullLifceCycle
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

        public string ViewLifceCycle
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

        public ItemLeftLifeCycleResponse()
        {
            this.m_sFullLifeCycle = string.Empty;
            this.m_sViewLifeCycle = string.Empty;
        } 

        #endregion
    }
}
