using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    [Serializable]
    public class TagIDValue
    {
        [DataMember]
        public string tagTypeName { get; set; }
        [DataMember]
        public int tagValueId { get; set; }
        [DataMember]
        public string tagValueName { get; set; }

        public TagIDValue()
        {

        }

        public TagIDValue(string tagTypeName, int tagValueId, string tagValueName)
        {
            this.tagTypeName = tagTypeName;
            this.tagValueId = tagValueId;
            this.tagValueName = tagValueName;
        }
    }
}
