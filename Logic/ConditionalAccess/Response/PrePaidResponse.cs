using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    [Serializable]
    public class PrePaidResponse
    {
        public PrePaidResponse()
        {
            m_oStatus = PrePaidResponseStatus.UnKnown;
            m_sStatusDescription = "";
        }

        public PrePaidResponseStatus m_oStatus;
        public string m_sStatusDescription;
    }
}
