using ApiObjects.Catalog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects
{
    public enum RelatedEntityType
    {
        General = 0,
        Channel = 1,
        ExternalChannel = 2,
        Media = 3,
        Program = 4
    }

    [Serializable]
    public class RelatedEntity : IEquatable<RelatedEntity>
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Type")]
        public RelatedEntityType Type { get; set; }

        public bool Equals(RelatedEntity other)
        {
            if (other == null || string.IsNullOrEmpty(other.Id))
                return false;

            return (Id.ToLower() == other.Id.ToLower() && Type == other.Type);
        }
    }

    public class RelatedEntityComparer : IEqualityComparer<RelatedEntity>
    {
        public bool Equals(RelatedEntity x, RelatedEntity y)
        {
            if (x == null && y == null)
                return true;

            if (x == null)
                return false;

            return x.Equals(y);
        }

        public int GetHashCode(RelatedEntity obj)
        {
            return obj.GetHashCode();
        }
    }

    [Serializable]
    public class RelatedEntities : IEquatable<RelatedEntities>
    {
        [DataMember]
        [JsonProperty("tagMeta")]
        public TagMeta TagMeta;
        
        [JsonProperty(PropertyName = "Items",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<RelatedEntity> Items { get; set; }

        public RelatedEntities()
        {
            Items = new List<RelatedEntity>();
        }

        public bool Equals(RelatedEntities other)
        {
            if (other == null || !this.TagMeta.Equals(other.TagMeta))
                return false;

            int valueCount = Items?.Count ?? 0;
            int otherValueCount = other.Items?.Count ?? 0;

            if (valueCount != otherValueCount)
                return false;

            if (valueCount > 0)
            {
                var currentItemsDic = new HashSet<RelatedEntity>(Items, new RelatedEntityComparer());
                foreach (var otherItems in other.Items)
                {
                    if (!currentItemsDic.Contains(otherItems))
                        return false;
                }
            }

            return true;
        }
    }

    public class RelatedEntitiesComparer : IEqualityComparer<RelatedEntities>
    {
        public bool Equals(RelatedEntities x, RelatedEntities y)
        {
            if (x == null && y == null)
                return true;

            if (x == null)
                return false;

            return x.Equals(y);
        }

        public int GetHashCode(RelatedEntities obj)
        {
            return obj.GetHashCode();
        }
    }
}