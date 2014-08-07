using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Catalog
{
    [KnownType(typeof(MediaObj))]
    [KnownType(typeof(PicObj))]
    [KnownType(typeof(ProgramObj))]
    [KnownType(typeof(MediaFileObj))]

    [DataContract]
    [Serializable]
    /*Base Object to all kind of return Object 
     Media, Pic ,(and for Future needs like Channels ect*/  
    public class BaseObject
    {
        [DataMember]
        public int m_nID;

        [DataMember]
        public DateTime m_dUpdateDate;
        

        public BaseObject()
        {
            m_nID = 0;
            m_dUpdateDate = DateTime.MinValue;
        }
    }
}
