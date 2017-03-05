using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    [Serializable]
    public class SearchGroupBy
    {
        [DataMember]
        public string Field
        {
            get;
            set;
        }

        [DataMember]
        public List<SearchGroupBy> Subs
        {
            get;
            set;
        }
    }
}
