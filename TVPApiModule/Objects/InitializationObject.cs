using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for InitializationObject
/// </summary>
/// 

namespace TVPApi
{
    public class InitializationObject
    {
        //User locale object
        public Locale Locale { get; set; }
        //User Platform
        private PlatformType m_Platform = PlatformType.Unknown;

        public PlatformType Platform
        {
            get
            {
                return m_Platform;
            }
            set
            {
                m_Platform = value;
            }
        }

        public InitializationObject()
        {

        }


    }
}
