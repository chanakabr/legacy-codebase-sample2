using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace ApiObjects.MediaIndexingObjects
{
    public abstract class QueueObject
    {
        protected JavaScriptSerializer serializer;

        public QueueObject()
        {
            serializer = new JavaScriptSerializer();
        }
        
        [DataMember]
        public int GroupId { get; set; }

        public override string ToString()
        {
            return serializer.Serialize(this);
        }

    }
}
