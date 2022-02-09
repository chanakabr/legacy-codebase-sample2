using ApiLogic.Api.Managers;
using ApiLogic.Base;
using ApiObjects;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;
using WebAPI.Exceptions;
using ApiObjects.Response;
using ApiObjects.Base;
using WebAPI.App_Start;
using ApiObjects.BulkUpload;
using System.Linq;
using System;

namespace WebAPI.Models.General
{
    public partial class KalturaDynamicList : KalturaCrudObject<DynamicList, long>
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Create date of the DynamicList
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }
        
        /// <summary>
        /// Update date of the DynamicList
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }
        
        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        
        internal override ICrudHandler<DynamicList, long> Handler { get { return DynamicListManager.Instance; } }
        
        internal override void SetId(long id)
        {
            this.Id = id;
        }
        public KalturaDynamicList() { }
        public override void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
        }
    }

    public partial class KalturaDynamicListListResponse : KalturaListResponse<KalturaDynamicList>, IKalturaExcelableListResponse
    {
        public KalturaDynamicListListResponse() : base() { }

        public ExcelStructure GetExcelStructure(int groupId)
        {
            //Matan: Temp removal
            //var featureEnabled = FeatureFlag.PhoenixFeatureFlagInstance.Get().IsUdidDynamicListAsExcelEnabled(groupId);

            //if (!featureEnabled)
            //{
            //    throw new BadRequestException(BadRequestException.FORMAT_NOT_SUPPORTED, "Enable feature: [dynamicList.format]");
            //}

            var _objects = GetObjects();
            if (_objects.Count > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                                              $"KalturaDynamicList.id: {_objects[0].GetId()}",
                                              $"KalturaDynamicList.id: {_objects[1].GetId()}");
            }

            var _item = AutoMapper.Mapper.Map<IExcelStructureManager>(_objects.First());
            var excelStructure = _item.GetExcelStructure(groupId, _item.GetType());
            return excelStructure;
        }

        public List<IKalturaExcelableObject> GetObjects()
        {
            if (Objects == null || !Objects.Any())
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaDynamicListListResponse.objects");
            }

            return this.Objects.OfType<IKalturaExcelableObject>().ToList();
        }
    }

    public partial class KalturaUdidDynamicList : KalturaDynamicList, IKalturaExcelableObject, IKalturaExcelStructureManager
    {
        //udid (column name): List<udid>
        public Dictionary<string, object> GetExcelValues(int groupId)
        {
            var columnNameToUdids = DynamicListManager.Instance.GetUdidsFromDynamicListById(groupId, this.Id);
            return columnNameToUdids;
        }

        internal override GenericResponse<DynamicList> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<UdidDynamicList>(this);
            return DynamicListManager.Instance.AddDynamicList(contextData, coreObject);
        }

        internal override GenericResponse<DynamicList> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<UdidDynamicList>(this);
            return DynamicListManager.Instance.UpdateDynamicList(contextData, coreObject);
        }

        public long GetId()
        {
            return this.Id;
        }

        public ExcelStructure GetExcelStructure(int groupId)
        {
            //unsupported
            throw new NotImplementedException();
        }
    }
}