using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Category details
    /// </summary>
    [Serializable]
    public partial class KalturaCategoryVersion : KalturaCrudObject<CategoryVersion, long>
    {
        /// <summary>
        /// Unique identifier for the category version
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Category version name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MaxLength = 255)]
        public string Name { get; set; }

        /// <summary>
        /// Category tree identifier 
        /// </summary>
        [DataMember(Name = "treeId")]
        [JsonProperty(PropertyName = "treeId")]
        [XmlElement(ElementName = "treeId")]
        [SchemeProperty(ReadOnly = true)]
        public long TreeId { get; set; }

        /// <summary>
        /// The category version state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty(PropertyName = "state")]
        [XmlElement(ElementName = "state")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaCategoryVersionState State { get; set; }

        /// <summary>
        /// The version id that this version was created from
        /// </summary>
        [DataMember(Name = "baseVersionId")]
        [JsonProperty(PropertyName = "baseVersionId")]
        [XmlElement(ElementName = "baseVersionId")]
        [SchemeProperty(MinLong = 1, InsertOnly = true)]
        public long BaseVersionId { get; set; }

        /// <summary>
        /// The root of category item id that was created for this version
        /// </summary>
        [DataMember(Name = "categoryRootId")]
        [JsonProperty(PropertyName = "categoryRootId")]
        [XmlElement(ElementName = "categoryRootId")]
        [SchemeProperty(ReadOnly = true)]
        public long CategoryRootId { get; set; }

        /// <summary>
        /// The date that this version became default represented as epoch.
        /// </summary>
        [DataMember(Name = "defaultDate")]
        [JsonProperty("defaultDate")]
        [XmlElement(ElementName = "defaultDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? DefaultDate { get; set; }

        /// <summary>
        /// Last updater user id.
        /// </summary>
        [DataMember(Name = "updaterId")]
        [JsonProperty("updaterId")]
        [XmlElement(ElementName = "updaterId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UpdaterId { get; set; }

        /// <summary>
        /// Comment.
        /// </summary>
        [DataMember(Name = "comment")]
        [JsonProperty(PropertyName = "comment")]
        [XmlElement(ElementName = "comment")]
        [SchemeProperty(MaxLength = 255)]
        public string Comment { get; set; }

        /// <summary>
        /// The date that this version was created represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// The date that this version was last updated represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        internal override ICrudHandler<CategoryVersion, long> Handler
        {
            get
            {
                return CategoryVersionHandler.Instance;
            }
        }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        public override void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new Exceptions.BadRequestException(Exceptions.BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
        }

        internal override void ValidateForUpdate()
        {
        }

        public KalturaCategoryVersion() : base() { }

        internal override GenericResponse<CategoryVersion> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<CategoryVersion>(this);
            return CategoryVersionHandler.Instance.Add(contextData, coreObject);
        }

        internal override GenericResponse<CategoryVersion> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<CategoryVersion>(this);
            return CategoryVersionHandler.Instance.Update(contextData, coreObject);
        }
    }

    public partial class KalturaCategoryVersionListResponse : KalturaListResponse<KalturaCategoryVersion>
    {
        public KalturaCategoryVersionListResponse() : base() { }
    }

    public enum KalturaCategoryVersionState
    {
        DRAFT = 0, DEFAULT = 1, RELEASED = 2
    }
}