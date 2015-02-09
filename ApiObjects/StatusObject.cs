using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class StatusObject
    {
        /// <summary>
        /// Http Status code: According to global rules and standarts
        /// </summary>
        public StatusObjectCode Status;

        /// <summary>
        /// Full status code
        /// </summary>
        public int Code;
        
        /// <summary>
        /// Full status message
        /// </summary>
        public string Message;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StatusObject()
            : this(StatusObjectCode.Unkown)
        {
            
        }

        /// <summary>
        /// Initialize the status object with all parameters
        /// </summary>
        /// <param name="p_eStatusObjectCode"></param>
        /// <param name="p_sMessage"></param>
        /// <param name="p_oBody"></param>
        public StatusObject(StatusObjectCode p_eStatusObjectCode = StatusObjectCode.Unkown, int p_nCode = 0, string p_sMessage = "")
        {
            this.Status = p_eStatusObjectCode;
            this.Code = p_nCode;
            this.Message = p_sMessage;
        }
    }
}
