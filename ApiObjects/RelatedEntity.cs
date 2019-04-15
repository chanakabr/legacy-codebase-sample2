using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects
{
    public class RelatedEntity
    {
        public string Id { get; set; }
        public RelatedEntityType Type { get; set; }
    }

    public enum RelatedEntityType
    {
        Channel = 0,
        ExternalChannel = 1,
        Media = 2,
        Program = 3
    }

    public class RelatedEntities
    {
        //TODO anat:
        //[DataMember]
        //[JsonProperty("m_oTagMeta")]
        //public TagMeta m_oTagMeta;

        public List<RelatedEntity> Items { get; set; }

        public RelatedEntities()
        {
            Items = new List<RelatedEntity>();
        }
    }
}