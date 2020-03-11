using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TVPApiServices
{

    [DataContract]
    public class OrderObj
    {
        [DataMember]
        public OrderBy m_eOrderBy { get; set; }
        [DataMember]
        public OrderDir m_eOrderDir { get; set; }
        [DataMember]
        public string m_sOrderValue { get; set; }

        public ApiObjects.SearchObjects.OrderObj ToCore()
        {
            ApiObjects.SearchObjects.OrderObj result = new ApiObjects.SearchObjects.OrderObj()
            {
                m_sOrderValue = this.m_sOrderValue
            };

            result.m_eOrderBy = (ApiObjects.SearchObjects.OrderBy)Enum.Parse(typeof(ApiObjects.SearchObjects.OrderBy), this.m_eOrderBy.ToString());
            result.m_eOrderDir = (ApiObjects.SearchObjects.OrderDir)Enum.Parse(typeof(ApiObjects.SearchObjects.OrderDir), this.m_eOrderDir.ToString());

            return result;
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "OrderBy", Namespace = "http://schemas.datacontract.org/2004/07/ApiObjects.SearchObjects")]
    public enum OrderBy : int
    {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        ID = 0,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        VIEWS = -7,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        RATING = -8,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        VOTES_COUNT = -80,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        LIKE_COUNTER = -9,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        START_DATE = -10,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        NAME = -11,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        CREATE_DATE = -12,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        META = 100,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        RANDOM = -6,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        RELATED = 31,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        NONE = 101,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        RECOMMENDATION = -13,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "OrderDir", Namespace = "http://schemas.datacontract.org/2004/07/ApiObjects.SearchObjects")]
    public enum OrderDir : int
    {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        ASC = 0,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        DESC = 1,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        NONE = 2,
    }
}