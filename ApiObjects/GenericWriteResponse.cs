using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class GenericWriteResponse
    {
        public GenericWriteResponse()
        {
            m_sStatusDescription = "";
            m_nStatusCode = 0;
        }
        public void Initialize(string sStatusDescription, Int32 nStatus)
        {
            m_sStatusDescription = sStatusDescription;
            m_nStatusCode = nStatus;
        }

        public Int32 m_nStatusCode;
        public string m_sStatusDescription;
    }
}
