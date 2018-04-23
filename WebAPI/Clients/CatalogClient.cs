using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using AutoMapper;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class CatalogClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string EPG_DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss";

        public string Signature { get; set; }
        public string SignString { get; set; }
        public string SignatureKey
        {
            set
            {
                SignString = Guid.NewGuid().ToString();
                Signature = GetSignature(SignString, value);
            }
        }

        #region New Catalog Management        

        public KalturaAssetStructListResponse GetAssetStructs(int groupId, List<long> ids, KalturaAssetStructOrderBy? orderBy, bool? isProtected, long metaId = 0)
        {
            KalturaAssetStructListResponse result = new KalturaAssetStructListResponse() { TotalCount = 0 };
            AssetStructListResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (metaId > 0)
                    {
                        response = Core.Catalog.CatalogManagement.CatalogManager.GetAssetStructsByTopicId(groupId, metaId, isProtected);
                    }
                    else
                    {
                        response = Core.Catalog.CatalogManagement.CatalogManager.GetAssetStructsByIds(groupId, ids, isProtected);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.AssetStructs != null && response.AssetStructs.Count > 0)
            {
                result.TotalCount = response.AssetStructs.Count;
                result.AssetStructs = new List<KalturaAssetStruct>();
                foreach (AssetStruct assetStruct in response.AssetStructs)
                {
                    result.AssetStructs.Add(AutoMapper.Mapper.Map<KalturaAssetStruct>(assetStruct));
                }
            }

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaAssetStructOrderBy.NAME_ASC:
                        result.AssetStructs = result.AssetStructs.OrderBy(x => x.Name.ToString()).ToList();
                        break;
                    case KalturaAssetStructOrderBy.NAME_DESC:
                        result.AssetStructs = result.AssetStructs.OrderByDescending(x => x.Name.ToString()).ToList();
                        break;
                    case KalturaAssetStructOrderBy.SYSTEM_NAME_ASC:
                        result.AssetStructs = result.AssetStructs.OrderBy(x => x.SystemName).ToList();
                        break;
                    case KalturaAssetStructOrderBy.SYSTEM_NAME_DESC:
                        result.AssetStructs = result.AssetStructs.OrderByDescending(x => x.SystemName).ToList();
                        break;
                    case KalturaAssetStructOrderBy.CREATE_DATE_ASC:
                        result.AssetStructs = result.AssetStructs.OrderBy(x => x.CreateDate).ToList();
                        break;
                    case KalturaAssetStructOrderBy.CREATE_DATE_DESC:
                        result.AssetStructs = result.AssetStructs.OrderByDescending(x => x.CreateDate).ToList();
                        break;
                    case KalturaAssetStructOrderBy.UPDATE_DATE_ASC:
                        result.AssetStructs = result.AssetStructs.OrderBy(x => x.UpdateDate).ToList();
                        break;
                    case KalturaAssetStructOrderBy.UPDATE_DATE_DESC:
                        result.AssetStructs = result.AssetStructs.OrderByDescending(x => x.UpdateDate).ToList();
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public KalturaAssetStruct AddAssetStruct(int groupId, KalturaAssetStruct assetStrcut, long userId)
        {
            KalturaAssetStruct result = null;
            AssetStructResponse response = null;

            try
            {
                AssetStruct assetStructToAdd = AutoMapper.Mapper.Map<AssetStruct>(assetStrcut);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.AddAssetStruct(groupId, assetStructToAdd, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaAssetStruct>(response.AssetStruct);
            return result;
        }

        public KalturaAssetStruct UpdateAssetStruct(int groupId, long id, KalturaAssetStruct assetStrcut, long userId)
        {
            KalturaAssetStruct result = null;
            AssetStructResponse response = null;

            try
            {
                bool shouldUpdateMetaIds = assetStrcut.MetaIds != null;
                AssetStruct assetStructToUpdate = AutoMapper.Mapper.Map<AssetStruct>(assetStrcut);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.UpdateAssetStruct(groupId, id, assetStructToUpdate, shouldUpdateMetaIds, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaAssetStruct>(response.AssetStruct);
            return result;
        }

        public bool DeleteAssetStruct(int groupId, long id, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.DeleteAssetStruct(groupId, id, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }       

        public KalturaMetaListResponse GetMetas(int groupId, List<long> ids, KalturaMetaDataType? type, KalturaMetaOrderBy? orderBy,
                                                bool? multipleValue = null, long assetStructId = 0)
        {
            KalturaMetaListResponse result = new KalturaMetaListResponse() { TotalCount = 0 };
            TopicListResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    ApiObjects.MetaType metaType = ApiObjects.MetaType.All;
                    if (type.HasValue)
                    {
                        metaType = CatalogMappings.ConvertToMetaType(type, multipleValue);
                    }

                    if (assetStructId > 0)
                    {
                        response = Core.Catalog.CatalogManagement.CatalogManager.GetTopicsByAssetStructId(groupId, assetStructId, metaType);
                    }
                    else
                    {
                        response = Core.Catalog.CatalogManagement.CatalogManager.GetTopicsByIds(groupId, ids, metaType);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Topics != null && response.Topics.Count > 0)
            {
                result.TotalCount = response.Topics.Count;
                result.Metas = new List<KalturaMeta>();
                foreach (Topic topic in response.Topics)
                {
                    result.Metas.Add(AutoMapper.Mapper.Map<KalturaMeta>(topic));
                }
            }

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaMetaOrderBy.NAME_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.Name.ToString()).ToList();
                        break;
                    case KalturaMetaOrderBy.NAME_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.Name.ToString()).ToList();
                        break;
                    case KalturaMetaOrderBy.SYSTEM_NAME_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.SystemName).ToList();
                        break;
                    case KalturaMetaOrderBy.SYSTEM_NAME_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.SystemName).ToList();
                        break;
                    case KalturaMetaOrderBy.CREATE_DATE_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.CreateDate).ToList();
                        break;
                    case KalturaMetaOrderBy.CREATE_DATE_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.CreateDate).ToList();
                        break;
                    case KalturaMetaOrderBy.UPDATE_DATE_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.UpdateDate).ToList();
                        break;
                    case KalturaMetaOrderBy.UPDATE_DATE_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.UpdateDate).ToList();
                        break;
                    default:
                        break;
                }
            }

            return result;
        }
        
        public KalturaMeta AddMeta(int groupId, KalturaMeta meta, long userId)
        {
            KalturaMeta result = null;
            TopicResponse response = null;

            try
            {
                Topic topicToAdd = AutoMapper.Mapper.Map<Topic>(meta);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.AddTopic(groupId, topicToAdd, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaMeta>(response.Topic);
            return result;
        }

        public KalturaMeta UpdateMeta(int groupId, long id, KalturaMeta meta, long userId)
        {
            KalturaMeta result = null;
            TopicResponse response = null;

            try
            {
                Topic topicToUpdate = AutoMapper.Mapper.Map<Topic>(meta);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.UpdateTopic(groupId, id, topicToUpdate, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaMeta>(response.Topic);
            return result;
        }

        public bool DeleteMeta(int groupId, long id, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.DeleteTopic(groupId, id, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        public KalturaAsset AddAsset(int groupId, KalturaAsset asset, long userId)
        {
            KalturaAsset result = null;
            AssetResponse response = null;
            Type kalturaMediaAssetType = typeof(KalturaMediaAsset);

            try
            {
                eAssetTypes assetType = eAssetTypes.UNKNOWN;
                Asset assetToAdd = null;
                kalturaMediaAssetType = typeof(KalturaMediaAsset);
                // in case asset is media
                if (kalturaMediaAssetType.IsAssignableFrom(asset.GetType()))
                {
                    assetToAdd = AutoMapper.Mapper.Map<MediaAsset>(asset);
                    assetType = eAssetTypes.MEDIA;
                }
                // add here else if for epg\recording when needed
                else
                {
                    throw new ClientException((int)StatusCode.Error, "Invalid assetType");                        
                }

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.AssetManager.AddAsset(groupId, assetType, assetToAdd, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            
            // in case asset is media
            if (kalturaMediaAssetType.IsAssignableFrom(asset.GetType()))
            {
                result = Mapper.Map<KalturaMediaAsset>(response.Asset);
                result.Images = CatalogMappings.ConvertImageListToKalturaMediaImageList(groupId, response.Asset.Images, ImageManager.GetImageTypeIdToRatioNameMap(groupId));
            }
            // add here else if for epg\recording when needed
            else
            {
                throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }

            return result;
        }

        public bool DeleteAsset(int groupId, long id, KalturaAssetReferenceType assetReferenceType, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    eAssetTypes assetType = CatalogMappings.ConvertToAssetTypes(assetReferenceType);                   

                    response = Core.Catalog.CatalogManagement.AssetManager.DeleteAsset(groupId, id, assetType, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }        

        public KalturaAsset GetAsset(int groupId, long id, KalturaAssetReferenceType assetReferenceType, string siteGuid, int domainId, string udid, string language, bool isOperatorSearch)
        {
            KalturaAsset result = null;
            AssetResponse response = null;
            eAssetTypes assetType = eAssetTypes.UNKNOWN;
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (doesGroupUsesTemplates)
                    {
                        assetType = CatalogMappings.ConvertToAssetTypes(assetReferenceType);
                        response = Core.Catalog.CatalogManagement.AssetManager.GetAsset(groupId, id, assetType, isOperatorSearch);
                    }
                    else
                    {
                        KalturaAssetListResponse assetListResponse = GetMediaByIds(groupId, siteGuid, domainId, udid, language, 0, 1, new List<int>() { (int)id }, KalturaAssetOrderBy.START_DATE_DESC);
                        if (assetListResponse != null && assetListResponse.TotalCount == 1)
                        {
                            return assetListResponse.Objects[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            switch (assetType)
            {
                case eAssetTypes.MEDIA:
                    result = Mapper.Map<KalturaMediaAsset>(response.Asset);
                    result.Images = CatalogMappings.ConvertImageListToKalturaMediaImageList(groupId, response.Asset.Images, ImageManager.GetImageTypeIdToRatioNameMap(groupId));
                    break;
                case eAssetTypes.EPG:
                    break;
                case eAssetTypes.NPVR:
                    break;
                case eAssetTypes.UNKNOWN:                    
                default:
                    throw new ClientException((int)StatusCode.Error, "Invalid assetType");
                    break;
            }   

            return result;
        }

        public KalturaAsset UpdateAsset(int groupId, long id, KalturaAsset asset, long userId)
        {
            KalturaAsset result = null;
            AssetResponse response = null;
            Type kalturaMediaAssetType = typeof(KalturaMediaAsset);
            try
            {
                eAssetTypes assetType = eAssetTypes.UNKNOWN;
                Asset assetToUpdate = null;                
                // in case asset is media
                if (kalturaMediaAssetType.IsAssignableFrom(asset.GetType()))
                {
                    assetToUpdate = AutoMapper.Mapper.Map<MediaAsset>(asset as KalturaMediaAsset);
                    assetType = eAssetTypes.MEDIA;
                }
                // add here else if for epg\recording when needed
                else
                {
                    throw new ClientException((int)StatusCode.Error, "Invalid assetType");
                }

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.AssetManager.UpdateAsset(groupId, id, assetType, assetToUpdate, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // in case asset is media
            if (kalturaMediaAssetType.IsAssignableFrom(asset.GetType()))
            {
                result = Mapper.Map<KalturaMediaAsset>(response.Asset);
                result.Images = CatalogMappings.ConvertImageListToKalturaMediaImageList(groupId, response.Asset.Images, ImageManager.GetImageTypeIdToRatioNameMap(groupId));
            }
            // add here else if for epg\recording when needed
            else
            {
                throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }

            return result;
        }

        public bool RemoveTopicsFromAsset(int groupId, long id, KalturaAssetReferenceType assetReferenceType, HashSet<long> topicIds, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    eAssetTypes assetType = eAssetTypes.UNKNOWN;
                    switch (assetReferenceType)
                    {
                        case KalturaAssetReferenceType.media:
                            assetType = eAssetTypes.MEDIA;
                            break;
                        case KalturaAssetReferenceType.epg_internal:
                            break;
                        case KalturaAssetReferenceType.epg_external:
                            break;
                        default:
                            throw new ClientException((int)StatusCode.Error, "Invalid assetType");
                            break;
                    }

                    response = Core.Catalog.CatalogManagement.AssetManager.RemoveTopicsFromAsset(groupId, id, assetType, topicIds, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }        

        public KalturaAssetListResponse GetAssetForGroupWithTemplates(int groupId, List<BaseObject> assetsBaseDataList, bool isOperatorSearch)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();
            if (assetsBaseDataList != null && assetsBaseDataList.Count > 0)
            {                
                AssetListResponse assetListResponse = AssetManager.GetOrderedAssets(groupId, assetsBaseDataList, isOperatorSearch);
                if (assetListResponse != null && assetListResponse.Status != null && assetListResponse.Status.Code == (int)eResponseStatus.OK)
                {
                    result.Objects = new List<KalturaAsset>();
                    // convert assets
                    Dictionary<long, string> imageTypeIdToRatioNameMap = ImageManager.GetImageTypeIdToRatioNameMap(groupId);
                    foreach (MediaAsset mediaAssetToConvert in assetListResponse.Assets.Where(x => x.AssetType == eAssetTypes.MEDIA))
                    {
                        KalturaMediaAsset kalturaMediaAsset = Mapper.Map<KalturaMediaAsset>(mediaAssetToConvert);
                        kalturaMediaAsset.Images = CatalogMappings.ConvertImageListToKalturaMediaImageList(groupId, mediaAssetToConvert.Images, imageTypeIdToRatioNameMap);
                        result.Objects.Add(kalturaMediaAsset);
                    }

                    //TODO : add support when needed for EPG\Recording
                    //List<KalturaProgramAsset> programAssets = null;
                    //List<KalturaRecordingAsset> recordingAssets = null;
                    //if (programAssets != null && programAssets.Count > 0)
                    //{
                    //    result.Objects.AddRange(programAssets);
                    //}

                    //if (recordingAssets != null && recordingAssets.Count > 0)
                    //{
                    //    result.Objects.AddRange(recordingAssets);
                    //}        
                }
                else if (assetListResponse != null && assetListResponse.Status != null)
                {
                    throw new ClientException((int)assetListResponse.Status.Code, assetListResponse.Status.Message.ToString());
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetAssetFromUnifiedSearchResponse(int groupId, UnifiedSearchResponse searchResponse, BaseRequest request, bool isOperatorSearch,
                                                                            bool managementData = false, KalturaBaseResponseProfile responseProfile = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            // check if aggragation result have values 
            if (searchResponse.aggregationResults != null && searchResponse.aggregationResults.Count > 0 &&
                searchResponse.aggregationResults[0].results != null && searchResponse.aggregationResults[0].results.Count > 0 && responseProfile != null)
            {
                if (doesGroupUsesTemplates)
                {
                    List<BaseObject> assetsBaseDataList = new List<BaseObject>();
                    foreach (Catalog.Response.AggregationResult aggregationResult in searchResponse.aggregationResults[0].results)
                    {
                        if (aggregationResult.topHits != null && aggregationResult.topHits.Count > 0)
                        {
                            assetsBaseDataList.Add(aggregationResult.topHits[0] as BaseObject);                            
                        }
                    }

                    result = GetAssetForGroupWithTemplates(groupId, assetsBaseDataList, isOperatorSearch);
                }
                else
                {
                    // build the assetsBaseDataList from the hit array 
                    result.Objects = CatalogUtils.GetAssets(searchResponse.aggregationResults[0].results, request, CacheDuration, managementData, responseProfile);
                }

                result.TotalCount = searchResponse.aggregationResults[0].totalItems;
            }
            else if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();
                if (doesGroupUsesTemplates)
                {                    
                    result = GetAssetForGroupWithTemplates(groupId, assetsBaseDataList, isOperatorSearch);
                }
                else
                {
                    // get base objects list                    
                    result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration, managementData);
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            return result;
        }

        #endregion        

        public int CacheDuration { get; set; }

        private string GetSignature(string signString, string signatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = signatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }

        private DateTime getServerTime()
        {
            return (DateTime)HttpContext.Current.Items[RequestParser.REQUEST_TIME];
        }

        [Obsolete]
        public KalturaAssetInfoListResponse SearchAssets(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            string filter, KalturaOrder? orderBy, List<int> assetTypes, string requestId, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.RELATED;
                order.m_eOrderDir = OrderDir.DESC;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                order = order,
                assetTypes = assetTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                requestId = requestId
            };


            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupId, pageSize, pageIndex, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            result.RequestId = searchResponse.requestId;

            return result;
        }

        public KalturaAssetListResponse SearchAssets(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            string filter, KalturaAssetOrderBy orderBy, List<int> assetTypes, List<int> epgChannelIds, bool managementData, KalturaDynamicOrderBy assetOrder = null,
            List<string> groupBy = null, KalturaBaseResponseProfile responseProfile = null, bool isOperatorSearch = false)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            OrderObj order = CatalogConvertor.ConvertOrderToOrderObj(orderBy, assetOrder);

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupId, pageSize, pageIndex, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);

            if (assetTypes != null && assetTypes.Count > 0)
            {
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));
            }

            if (epgChannelIds != null && epgChannelIds.Count > 0)
            {
                string strEpgChannelIds = string.Join(",", epgChannelIds.Select(at => at.ToString()).ToArray());
                key.AppendFormat("_ec={0}", strEpgChannelIds);
                filter = string.Format("(and {0} epg_channel_id:'{1}')", filter, strEpgChannelIds);
            }

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                order = order,
                assetTypes = assetTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                IsOperatorSearch = isOperatorSearch
            };

            if (groupBy != null && groupBy.Count > 0)
            {
                request.searchGroupBy = new SearchAggregationGroupBy()
                {
                    groupBy = groupBy,
                    distinctGroup = groupBy[0], // mabye will send string.empty - and Backend will fill it if nessecery
                    topHitsCount = 1
                };
            }

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            result = GetAssetFromUnifiedSearchResponse(groupId, searchResponse, request, isOperatorSearch, managementData, responseProfile);

            return result;
        }

        public KalturaAssetCount GetAssetCount(int groupId, string siteGuid, int domainId, string udid, string language,
            string filter, KalturaAssetOrderBy orderBy, List<int> assetTypes, List<int> epgChannelIds, List<string> groupBy)
        {
            KalturaAssetCount result = new KalturaAssetCount();

            OrderObj order = CatalogConvertor.ConvertOrderToOrderObj(orderBy);

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("asset_count__g={0}_ob={1}_od={2}_ov={3}_f={4}", groupId, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);

            if (assetTypes != null && assetTypes.Count > 0)
            {
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));
            }

            if (epgChannelIds != null && epgChannelIds.Count > 0)
            {
                string strEpgChannelIds = string.Join(",", epgChannelIds.Select(at => at.ToString()).ToArray());
                key.AppendFormat("_ec={0}", strEpgChannelIds);
                filter += string.Format(" epg_channel_id:'{0}'", strEpgChannelIds);
            }

            if (groupBy == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupBy");
            }

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = 0,
                m_nPageSize = 0,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                order = order,
                assetTypes = assetTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                searchGroupBy = new SearchAggregationGroupBy()
                {
                    groupBy = groupBy,
                    groupByOrder = AggregationOrder.Value_Asc
                }
            };

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            if (searchResponse.aggregationResults != null && searchResponse.aggregationResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // map counts
                result.SubCounts = Mapper.Map<List<KalturaAssetsCount>>(searchResponse.aggregationResults);
                result.Count = searchResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaSlimAssetInfoWrapper Autocomplete(int groupId, string siteGuid, string udid, string language, int? size, string query, KalturaOrder? orderBy, List<int> assetTypes, List<KalturaCatalogWith> with)
        {
            KalturaSlimAssetInfoWrapper result = new KalturaSlimAssetInfoWrapper();

            // Create our own filter - only search in title
            string filter = string.Format("(and name^'{0}')", query.Replace("'", "%27"));

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.RELATED;
                order.m_eOrderDir = OrderDir.DESC;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_sDeviceId = udid,
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = 0,
                m_nPageSize = size.Value,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                order = order,
                assetTypes = assetTypes,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Autocomplete_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupId, size, 0, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToSlimAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaBaseAssetInfo)a).ToList();
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaAssetHistoryListResponse getAssetHistory(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize, KalturaWatchStatus watchStatus, int days, List<int> assetTypes, List<string> assetIds, List<KalturaCatalogWith> withList = null)
        {
            KalturaAssetHistoryListResponse finalResults = new KalturaAssetHistoryListResponse();
            finalResults.Objects = new List<KalturaAssetHistory>();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSiteGuid = siteGuid,
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                AssetTypes = assetTypes,
                FilterStatus = CatalogMappings.ConvertKalturaWatchStatus(watchStatus),
                NumOfDays = days,
                OrderDir = OrderDir.DESC,
                AssetIds = assetIds
            };

            // fire history watched request
            WatchHistoryResponse watchHistoryResponse = new WatchHistoryResponse();
            if (!CatalogUtils.GetBaseResponse<WatchHistoryResponse>(request, out watchHistoryResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (watchHistoryResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(watchHistoryResponse.status.Code, watchHistoryResponse.status.Message);
            }

            if (watchHistoryResponse.result != null && watchHistoryResponse.result.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = watchHistoryResponse.result.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration, withList, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // combine asset info and watch history info
                finalResults.TotalCount = watchHistoryResponse.m_nTotalItems;

                UserWatchHistory watchHistory = new UserWatchHistory();
                foreach (KalturaIAssetable assetInfo in assetsInfo)
                {
                    watchHistory = watchHistoryResponse.result.FirstOrDefault(x => x.AssetId == ((KalturaAssetInfo)assetInfo).Id.ToString());

                    KalturaAssetType assetType = KalturaAssetType.media;

                    if (watchHistory.AssetType == eAssetTypes.NPVR)
                    {
                        assetType = KalturaAssetType.recording;
                    }

                    if (watchHistory != null)
                    {
                        finalResults.Objects.Add(new KalturaAssetHistory()
                        {
                            AssetId = ((KalturaAssetInfo)assetInfo).Id.Value,
                            Duration = watchHistory.Duration,
                            IsFinishedWatching = watchHistory.IsFinishedWatching,
                            LastWatched = watchHistory.LastWatch,
                            Position = watchHistory.Location,
                            AssetType = assetType
                        });
                    }
                }
            }

            return finalResults;
        }

        [Obsolete]
        public KalturaWatchHistoryAssetWrapper WatchHistory(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize, KalturaWatchStatus watchStatus, int days, List<int> assetTypes, List<string> assetIds, List<KalturaCatalogWith> withList)
        {
            KalturaWatchHistoryAssetWrapper finalResults = new KalturaWatchHistoryAssetWrapper();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSiteGuid = siteGuid,
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                AssetTypes = assetTypes,
                AssetIds = assetIds,
                FilterStatus = CatalogMappings.ConvertKalturaWatchStatus(watchStatus),
                NumOfDays = days,
                OrderDir = OrderDir.DESC
            };

            // fire history watched request
            WatchHistoryResponse watchHistoryResponse = new WatchHistoryResponse();
            if (!CatalogUtils.GetBaseResponse<WatchHistoryResponse>(request, out watchHistoryResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (watchHistoryResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(watchHistoryResponse.status.Code, watchHistoryResponse.status.Message);
            }

            if (watchHistoryResponse.result != null && watchHistoryResponse.result.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = watchHistoryResponse.result.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration, withList, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // combine asset info and watch history info
                finalResults.TotalCount = watchHistoryResponse.m_nTotalItems;

                UserWatchHistory watchHistory = new UserWatchHistory();
                foreach (var assetInfo in assetsInfo)
                {
                    watchHistory = watchHistoryResponse.result.FirstOrDefault(x => x.AssetId == ((KalturaAssetInfo)assetInfo).Id.ToString());

                    if (watchHistory != null)
                    {
                        finalResults.Objects.Add(new KalturaWatchHistoryAsset()
                        {
                            Asset = (KalturaAssetInfo)assetInfo,
                            Duration = watchHistory.Duration,
                            IsFinishedWatching = watchHistory.IsFinishedWatching,
                            LastWatched = watchHistory.LastWatch,
                            Position = watchHistory.Location
                        });
                    }
                }
            }

            return finalResults;
        }

        public List<KalturaAssetStatistics> GetAssetsStats(int groupID, string siteGuid, List<int> assetIds, KalturaAssetType assetType, long startTime = 0, long endTime = 0)
        {
            List<KalturaAssetStatistics> result = null;
            AssetStatsRequest request = new AssetStatsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupID,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nAssetIDs = assetIds,
                m_dStartDate = startTime != 0 ? SerializationUtils.ConvertFromUnixTimestamp(startTime) : DateTime.MinValue,
                m_dEndDate = endTime != 0 ? SerializationUtils.ConvertFromUnixTimestamp(endTime) : DateTime.MaxValue,
                m_type = CatalogMappings.ConvertAssetType(assetType)
            };

            AssetStatsResponse response = null;
            if (CatalogUtils.GetBaseResponse(request, out response))
            {
                result = response.m_lAssetStat != null ?
                    Mapper.Map<List<KalturaAssetStatistics>>(response.m_lAssetStat) : null;
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetRelatedMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, int mediaId, string filter, List<int> mediaTypes, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaRelatedRequest request = new MediaRelatedRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_sFilter = filter
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMedia(request, key.ToString(), CacheDuration, with);

            return result;
        }

        public KalturaAssetListResponse GetRelatedMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, int mediaId, string filter, List<int> mediaTypes,
            KalturaAssetOrderBy orderBy, KalturaDynamicOrderBy assetOrder = null, List<string> groupBy = null, KalturaBaseResponseProfile responseProfile = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // convert order by
            OrderObj order = CatalogConvertor.ConvertOrderToOrderObj(orderBy, assetOrder);

            // build request
            MediaRelatedRequest request = new MediaRelatedRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_sFilter = filter,
                OrderObj = order
            };
            if (groupBy != null && groupBy.Count > 0)
            {
                request.searchGroupBy = new SearchAggregationGroupBy()
                {
                    groupBy = groupBy,
                    distinctGroup = groupBy[0], // mabye will send string.empty - and Backend will fill it if nessecery
                    topHitsCount = 1
                };
            }
            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMedia(request, key.ToString(), CacheDuration, responseProfile);

            return result;
        }

        public KalturaAssetInfoListResponse GetRelatedMediaExternal(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
                                                                    int mediaId, List<int> mediaTypes, int utcOffset, List<KalturaCatalogWith> with, string freeParam)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaRelatedExternalRequest request = new MediaRelatedExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_nUtcOffset = utcOffset,
                m_sFreeParam = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(request, key.ToString(), CacheDuration, with);

            return result;
        }

        public KalturaAssetInfoListResponse GetSearchMediaExternal(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, string query, List<int> mediaTypes, int utcOffset, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaSearchExternalRequest request = new MediaSearchExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sQuery = query,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_nUtcOffset = utcOffset,
                m_sDeviceID = udid
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("search_q={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                query, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(request, key.ToString(), CacheDuration, with);

            return result;
        }

        public KalturaAssetInfoListResponse GetChannelMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            int channelId, KalturaOrder? orderBy, List<KalturaCatalogWith> with, List<KeyValue> filterTags,
            WebAPI.Models.Catalog.KalturaAssetInfoFilter.KalturaCutWith cutWith)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.NONE;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            ChannelRequestMultiFiltering request = new ChannelRequestMultiFiltering()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets,
                },
                m_lFilterTags = filterTags,
                m_eFilterCutWith = CatalogConvertor.ConvertCutWith(cutWith),
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nChannelID = channelId,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_oOrderObj = order,
                m_bIgnoreDeviceRuleID = false
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, siteGuid, language, orderBy);

            // fire request
            Core.Catalog.Response.ChannelResponse channelResponse = new Core.Catalog.Response.ChannelResponse();
            if (!CatalogUtils.GetBaseResponse<Core.Catalog.Response.ChannelResponse>(request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (channelResponse.m_nMedias != null && channelResponse.m_nMedias.Count > 0)
            {
                result.Objects = CatalogUtils.GetMediaByIds(channelResponse.m_nMedias, request, CacheDuration, with);
                result.TotalCount = channelResponse.m_nTotalItems;
            }
            return result;
        }

        public KalturaAssetInfoListResponse GetChannelAssets(int groupId, string siteGuid, int domainId, string udid, string language,
            int pageIndex, int? pageSize,
            List<KalturaCatalogWith> with,
            int channelId, KalturaOrder? orderBy, string filterQuery)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.NONE;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            InternalChannelRequest request = new InternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                order = order,
                internalChannelID = channelId.ToString(),
                filterQuery = filterQuery,
                m_dServerTime = getServerTime(),
                m_bIgnoreDeviceRuleID = false
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, siteGuid, language, orderBy);

            // fire request
            UnifiedSearchResponse channelResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (channelResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(channelResponse.status.Code, channelResponse.status.Message);
            }

            if (channelResponse.searchResults != null && channelResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = channelResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = channelResponse.m_nTotalItems;
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetMediaByIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> mediaIds, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaUpdateDateRequest request = new MediaUpdateDateRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_lMediaIds = mediaIds,
            };

            MediaIdsResponse mediaIdsResponse = new MediaIdsResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsResponse>(request, out mediaIdsResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {
                result.Objects = CatalogUtils.GetMediaByIds(mediaIdsResponse.m_nMediaIds, request, CacheDuration, with);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaAssetListResponse GetMediaByIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> mediaIds, KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaUpdateDateRequest request = new MediaUpdateDateRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_lMediaIds = mediaIds,
            };

            MediaIdsResponse mediaIdsResponse = new MediaIdsResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsResponse>(request, out mediaIdsResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {
                result.Objects = CatalogUtils.GetMediaByIds(mediaIdsResponse.m_nMediaIds, request, CacheDuration);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetEPGByInternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds,
            List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_lProgramsIds = epgIds,
            };

            EpgProgramResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(request, out epgProgramResponse) && epgProgramResponse != null)
            {

                var list = CatalogConvertor.ConvertBaseObjectsToAssetsInfo(groupId, epgProgramResponse.m_lObj, with);

                // build AssetInfoWrapper response
                if (list != null)
                {
                    result.Objects = list.Select(a => (KalturaAssetInfo)a).ToList();
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetEPGByInternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds, KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_lProgramsIds = epgIds,
            };

            EpgProgramResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(request, out epgProgramResponse) && epgProgramResponse != null)
            {
                result.Objects = Mapper.Map<List<KalturaAsset>>(epgProgramResponse.m_lObj);

                if (result.Objects != null)
                {
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetEPGByExternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<string> epgIds,
            List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EPGProgramsByProgramsIdentefierRequest request = new EPGProgramsByProgramsIdentefierRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                pids = epgIds.ToArray(),
                eLang = ApiObjects.Language.English,
                duration = 0
            };

            EpgProgramsResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(request, out epgProgramResponse) && epgProgramResponse != null)
            {

                var list = CatalogConvertor.ConvertEPGChannelProgrammeObjectToAssetsInfo(groupId, epgProgramResponse.lEpgList, with);

                // build AssetInfoWrapper response
                if (list != null)
                {
                    result.Objects = list.Select(a => (KalturaAssetInfo)a).ToList();
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetEPGByExternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<string> epgIds,
            KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EPGProgramsByProgramsIdentefierRequest request = new EPGProgramsByProgramsIdentefierRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                pids = epgIds.ToArray(),
                eLang = ApiObjects.Language.English,
                duration = 0
            };

            EpgProgramsResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(request, out epgProgramResponse) && epgProgramResponse != null && epgProgramResponse.lEpgList != null)
            {
                // get base objects list
                DateTime updateDate;
                List<BaseObject> assetsBaseDataList = epgProgramResponse.lEpgList.Select(x => new BaseObject()
                {
                    AssetId = x.EPG_ID.ToString(),
                    AssetType = eAssetTypes.EPG,
                    m_dUpdateDate = Utils.Utils.ConvertStringToDateTimeByFormat(x.UPDATE_DATE, EPG_DATETIME_FORMAT, out updateDate) ? updateDate : DateTime.MinValue
                }).ToList();

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration);
                result.TotalCount = epgProgramResponse.m_nTotalItems;

                if (result.Objects != null)
                {
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        internal List<KalturaEPGChannelAssets> GetEPGByChannelIds(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds, DateTime startTime, DateTime endTime, List<KalturaCatalogWith> with)
        {
            List<KalturaEPGChannelAssets> result = new List<KalturaEPGChannelAssets>();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EpgRequest request = new EpgRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_nChannelIDs = epgIds,
                m_dStartDate = startTime,
                m_dEndDate = endTime
            };

            EpgResponse epgProgramResponse = null;

            var isBaseResponse = CatalogUtils.GetBaseResponse<EpgResponse>(request, out epgProgramResponse);
            if (isBaseResponse && epgProgramResponse != null)
            {
                result = CatalogConvertor.ConvertEPGChannelAssets(groupId, epgProgramResponse.programsPerChannel, with);

                if (result == null)
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;

        }


        public WebAPI.Models.Catalog.KalturaChannel GetChannelInfo(int groupId, string siteGuid, int domainId, string udid, string language, int channelId)
        {
            WebAPI.Models.Catalog.KalturaChannel result = null;
            ChannelObjRequest request = new ChannelObjRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                ChannelId = channelId,
            };

            ChannelObjResponse response = null;
            if (CatalogUtils.GetBaseResponse(request, out response))
            {
                result = response.ChannelObj != null ?
                    Mapper.Map<WebAPI.Models.Catalog.KalturaChannel>(response.ChannelObj) : null;
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        public KalturaOTTCategory GetCategory(int groupId, string siteGuid, int domainId, string udid, string language, int categoryId)
        {
            KalturaOTTCategory result = null;
            CategoryRequest request = new CategoryRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                m_nCategoryID = categoryId,
            };

            CategoryResponse response = null;
            if (CatalogUtils.GetBaseResponse(request, out response) && response != null)
            {
                result = Mapper.Map<KalturaOTTCategory>(response);
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetsBookmarksResponse GetAssetsBookmarksOldStandard(string siteGuid, int groupId, int domainId, string udid, List<KalturaSlimAsset> assets)
        {
            List<KalturaAssetBookmarks> result = null;
            List<AssetBookmarkRequest> assetsToRequestPositions = new List<AssetBookmarkRequest>();

            foreach (KalturaSlimAsset asset in assets)
            {
                AssetBookmarkRequest assetInfo = new AssetBookmarkRequest();
                assetInfo.AssetID = asset.Id;
                bool addToRequest = true;
                switch (asset.Type)
                {
                    case KalturaAssetType.media:
                        assetInfo.AssetType = eAssetTypes.MEDIA;
                        break;
                    case KalturaAssetType.recording:
                        assetInfo.AssetType = eAssetTypes.NPVR;
                        break;
                    case KalturaAssetType.epg:
                        assetInfo.AssetType = eAssetTypes.EPG;
                        break;
                    default:
                        assetInfo.AssetType = eAssetTypes.UNKNOWN;
                        addToRequest = false;
                        break;
                }
                if (addToRequest)
                {
                    assetsToRequestPositions.Add(assetInfo);
                }
            }

            AssetsBookmarksRequest request = new AssetsBookmarksRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid
                },
                Data = new AssetsBookmarksRequestData()
                {
                    Assets = assetsToRequestPositions
                }
            };

            AssetsBookmarksResponse response = null;
            if (!CatalogUtils.GetBaseResponse(request, out response) || response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = Mapper.Map<List<KalturaAssetBookmarks>>(response.AssetsBookmarks);

            return new KalturaAssetsBookmarksResponse() { AssetsBookmarks = result, TotalCount = response.m_nTotalItems };

        }

        public KalturaBookmarkListResponse GetAssetsBookmarks(string siteGuid, int groupId, int domainId, string udid, List<KalturaSlimAsset> assets, KalturaBookmarkOrderBy orderBy)
        {
            List<KalturaBookmark> result = null;
            List<AssetBookmarkRequest> assetsToRequestPositions = new List<AssetBookmarkRequest>();

            foreach (KalturaSlimAsset asset in assets)
            {
                AssetBookmarkRequest assetInfo = new AssetBookmarkRequest();
                assetInfo.AssetID = asset.Id;
                bool addToRequest = true;
                switch (asset.Type)
                {
                    case KalturaAssetType.media:
                        assetInfo.AssetType = eAssetTypes.MEDIA;
                        break;
                    case KalturaAssetType.recording:
                        assetInfo.AssetType = eAssetTypes.NPVR;
                        break;
                    case KalturaAssetType.epg:
                        assetInfo.AssetType = eAssetTypes.EPG;
                        break;
                    default:
                        assetInfo.AssetType = eAssetTypes.UNKNOWN;
                        addToRequest = false;
                        break;
                }
                if (addToRequest)
                {
                    assetsToRequestPositions.Add(assetInfo);
                }
            }

            AssetsBookmarksRequest request = new AssetsBookmarksRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid
                },
                Data = new AssetsBookmarksRequestData()
                {
                    Assets = assetsToRequestPositions
                }
            };

            AssetsBookmarksResponse response = null;
            if (!CatalogUtils.GetBaseResponse(request, out response) || response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = CatalogMappings.ConvertBookmarks(response.AssetsBookmarks, orderBy);

            return new KalturaBookmarkListResponse() { AssetsBookmarks = result, TotalCount = response.m_nTotalItems };

        }

        public KalturaAssetInfoListResponse GetExternalChannelAssets(int groupId, string channelId,
            string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            KalturaOrder? orderBy, List<KalturaCatalogWith> with,
            string deviceType = null, string utcOffset = null, string freeParam = null)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.NONE;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            ExternalChannelRequest request = new ExternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                deviceId = udid,
                deviceType = deviceType,
                domainId = domainId,
                internalChannelID = channelId,
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sSiteGuid = siteGuid,
                m_sUserIP = Utils.Utils.GetClientIP(),
                utcOffset = utcOffset,
                free = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("external_channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, siteGuid, language, orderBy);

            // fire search request
            UnifiedSearchExternalResponse searchResponse = new UnifiedSearchExternalResponse();

            if (!CatalogUtils.GetBaseResponse<UnifiedSearchExternalResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse == null || searchResponse.status == null)
            {
                // Bad response received from WS
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo =
                    CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            result.RequestId = searchResponse.requestId;

            return result;
        }

        internal bool AddBookmark(int groupId, string siteGuid, int householdId, string udid, string assetId, KalturaAssetType assetType, long fileId, int Position, string action, int averageBitRate, int totalBitRate, int currentBitRate)
        {
            int t;

            if (assetType != KalturaAssetType.recording)
                if (string.IsNullOrEmpty(assetId) || !int.TryParse(assetId, out t))
                    throw new ClientException((int)StatusCode.BadRequest, "Invalid Asset id");

            eAssetTypes CatalogAssetType = eAssetTypes.UNKNOWN;
            switch (assetType)
            {
                case KalturaAssetType.epg:
                    CatalogAssetType = eAssetTypes.EPG;
                    break;
                case KalturaAssetType.media:
                    CatalogAssetType = eAssetTypes.MEDIA;
                    break;
                case KalturaAssetType.recording:
                    CatalogAssetType = eAssetTypes.NPVR;
                    break;
            }

            // build request
            MediaMarkRequest request = new MediaMarkRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                domainId = householdId,
                m_nGroupID = groupId,
                m_sSiteGuid = siteGuid,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_oMediaPlayRequestData = new MediaPlayRequestData()
                {
                    m_eAssetType = CatalogAssetType,
                    m_nLoc = Position,
                    m_nMediaFileID = (int)fileId,
                    m_sAssetID = assetId,
                    m_sAction = action,
                    m_sSiteGuid = siteGuid,
                    m_sUDID = udid,
                    m_nAvgBitRate = averageBitRate,
                    m_nCurrentBitRate = currentBitRate,
                    m_nTotalBitRate = totalBitRate
                }
            };

            // fire search request
            MediaMarkResponse response = new MediaMarkResponse();

            if (!CatalogUtils.GetBaseResponse<MediaMarkResponse>(request, out response))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.status.Code, response.status.Message);
            }

            return true;
        }

        internal List<KalturaSlimAsset> GetAssetsFollowing(string userID, int groupId, List<KalturaPersonalAssetRequest> assets, List<string> followPhrases)
        {
            List<KalturaSlimAsset> result = new List<KalturaSlimAsset>();

            // Create our own filter - only search in title
            string filter = "(or";
            followPhrases.ForEach(x => filter += string.Format(" {0}", x));
            filter += ")";

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                specificAssets = assets.Select(asset => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, asset.getId())).ToList()
                //assetTypes = assetTypes,
            };

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, null))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                foreach (var searchRes in searchResponse.searchResults)
                {
                    result.Add(Mapper.Map<KalturaSlimAsset>(searchRes));
                }
            }

            return result;
        }

        internal KalturaCountry GetCountryByIp(int groupId, string ip)
        {
            KalturaCountry result = null;
            CountryRequest request = new CountryRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter(),
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                Ip = ip
            };

            Core.Catalog.Response.CountryResponse response = null;
            if (CatalogUtils.GetBaseResponse(request, out response) && response != null && response.Status != null)
            {
                if (response.Status.Code == (int)StatusCode.OK)
                {
                    result = Mapper.Map<KalturaCountry>(response.Country);
                }
                else
                {
                    throw new ClientException(response.Status.Code, response.Status.Message);
                }
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        internal KalturaAssetListResponse GetExternalChannelAssets(int groupId, string channelId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize,
            KalturaAssetOrderBy orderBy, string deviceType, string utcOffset, string freeParam, KalturaDynamicOrderBy assetOrder = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // convert order by
            OrderObj order = CatalogConvertor.ConvertOrderToOrderObj(orderBy, assetOrder);

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            ExternalChannelRequest request = new ExternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                deviceId = udid,
                deviceType = deviceType,
                domainId = domainId,
                internalChannelID = channelId,
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sSiteGuid = userID,
                m_sUserIP = Utils.Utils.GetClientIP(),
                utcOffset = utcOffset,
                free = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("external_channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, userID, language, orderBy);

            // fire search request
            UnifiedSearchExternalResponse searchResponse = new UnifiedSearchExternalResponse();

            if (!CatalogUtils.GetBaseResponse<UnifiedSearchExternalResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse == null || searchResponse.status == null)
            {
                // Bad response received from WS
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }
            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration);

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            //result..RequestId = searchResponse.requestId;

            return result;
        }

        internal KalturaAssetListResponse GetRelatedMediaExternal(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, int mediaId,
            List<int> mediaTypes, int utcOffset, string freeParam)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaRelatedExternalRequest request = new MediaRelatedExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_nUtcOffset = utcOffset,
                m_sFreeParam = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(request, key.ToString(), CacheDuration);

            return result;
        }

        internal KalturaAssetListResponse GetSearchMediaExternal(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, string query,
            List<int> mediaTypes, int utcOffset)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaSearchExternalRequest request = new MediaSearchExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sQuery = query,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_nUtcOffset = utcOffset,
                m_sDeviceID = udid
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("search_q={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                query, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            MediaIdsStatusResponse mediaIdsResponse = new MediaIdsStatusResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsStatusResponse>(request, out mediaIdsResponse, true, key.ToString())
                || mediaIdsResponse == null || mediaIdsResponse.Status.Code != (int)StatusCode.OK)
            {
                if (mediaIdsResponse == null)
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());

                // general error
                throw new ClientException((int)mediaIdsResponse.Status.Code, mediaIdsResponse.Status.Message);
            }

            if (mediaIdsResponse.assetIds != null && mediaIdsResponse.assetIds.Count > 0)
            {
                result.Objects = CatalogUtils.GetAssets(mediaIdsResponse.assetIds.Select(a => (BaseObject)a).ToList(), request, CacheDuration);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }
            return result;
        }

        internal KalturaAssetListResponse GetChannelAssets(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, int id,
                                                            KalturaAssetOrderBy? orderBy, string filterQuery, bool shouldUseChannelDefault, KalturaDynamicOrderBy assetOrder = null,
                                                            KalturaBaseResponseProfile responseProfile = null, bool isOperatorSearch = false)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if ((assetOrder == null && orderBy == null) || shouldUseChannelDefault)
            {
                order.m_eOrderBy = OrderBy.NONE;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value, assetOrder);
            }


            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            InternalChannelRequest request = new InternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userID,
                domainId = domainId,
                order = order,
                internalChannelID = id.ToString(),
                filterQuery = filterQuery,
                m_dServerTime = getServerTime(),
                m_bIgnoreDeviceRuleID = false,
                IsOperatorSearch = isOperatorSearch
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                id, pageIndex, pageSize, groupId, userID, language, orderBy);

            // fire request
            UnifiedSearchResponse channelResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (channelResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(channelResponse.status.Code, channelResponse.status.Message);
            }

            result = GetAssetFromUnifiedSearchResponse(groupId, channelResponse, request, isOperatorSearch, false, responseProfile);

            return result;
        }

        internal KalturaAssetListResponse GetBundleAssets(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, int id,
            KalturaAssetOrderBy? orderBy, List<int> mediaTypes, KalturaBundleType bundleType, KalturaDynamicOrderBy assetOrder = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (assetOrder != null || orderBy != null)
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }
            else
            {
                order.m_eOrderBy = OrderBy.NONE;
            }

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            BundleAssetsRequest request = new BundleAssetsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_oOrderObj = order,
                m_sMediaType = mediaTypes != null ? string.Join(";", mediaTypes.ToArray()) : null,
                m_dServerTime = getServerTime(),
                m_eBundleType = bundleType == KalturaBundleType.collection ? eBundleType.COLLECTION : eBundleType.SUBSCRIPTION,
                m_nBundleID = id
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("bundle_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}_type={6}",
                id, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty, bundleType.ToString());

            result = CatalogUtils.GetMedia(request, key.ToString(), CacheDuration);

            return result;
        }

        internal KalturaAssetCommentListResponse GetAssetCommentsList(int groupId, string language, int id, KalturaAssetType AssetType, string userId, int domainId, string udid,
            int pageIndex, int? pageSize, KalturaAssetCommentOrderBy? orderBy)
        {
            KalturaAssetCommentListResponse result = new KalturaAssetCommentListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);
            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.NONE;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // build request
            AssetCommentsRequest request = new AssetCommentsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userId,
                domainId = domainId,
                m_dServerTime = getServerTime(),
                assetId = id,
                assetType = CatalogMappings.ConvertToAssetType(AssetType),
                orderObj = order,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("asset_id={0}_pi={1}_pz={2}_g={3}_l={4}_type={5}",
                id, pageIndex, pageSize, groupId, language, eAssetType.PROGRAM.ToString());
            AssetCommentsListResponse commentResponse = new AssetCommentsListResponse();
            if (CatalogUtils.GetBaseResponse<AssetCommentsListResponse>(request, out commentResponse))
            {
                if (commentResponse.status.Code != (int)StatusCode.OK)
                {
                    // Bad response received from WS
                    throw new ClientException(commentResponse.status.Code, commentResponse.status.Message);
                }
                else
                {
                    result.Objects = commentResponse.Comments != null ?
                        Mapper.Map<List<KalturaAssetComment>>(commentResponse.Comments) : null;
                    if (result.Objects != null)
                    {
                        result.TotalCount = commentResponse.m_nTotalItems;
                    }
                }
            }
            else
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            return result;
        }

        internal KalturaAssetComment AddAssetComment(int groupId, int assetId, KalturaAssetType assetType, string userId, int domainId, string writer, string header,
                                                     string subHeader, string contextText, string udid, string language)
        {
            KalturaAssetComment result = new KalturaAssetComment();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            AssetCommentAddRequest request = new AssetCommentAddRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_sSiteGuid = userId,
                domainId = domainId,
                m_dServerTime = getServerTime(),
                assetId = assetId,
                assetType = CatalogMappings.ConvertToAssetType(assetType),
                writer = writer,
                header = header,
                subHeader = subHeader,
                contentText = contextText,
                udid = udid
            };

            AssetCommentResponse assetCommentResponse = null;
            if (CatalogUtils.GetBaseResponse<AssetCommentResponse>(request, out assetCommentResponse))
            {
                if (assetCommentResponse.Status.Code != (int)StatusCode.OK)
                {
                    // Bad response received from WS
                    throw new ClientException(assetCommentResponse.Status.Code, assetCommentResponse.Status.Message);
                }
                else
                {
                    result = assetCommentResponse.AssetComment != null ? Mapper.Map<KalturaAssetComment>(assetCommentResponse.AssetComment) : null;
                }
            }
            else
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        internal KalturaAssetListResponse GetScheduledRecordingAssets(int groupId, string userID, int domainId, string udid, string language, List<long> channelIdsToFilter, int pageIndex, int? pageSize,
                                        long? startDateToFilter, long? endDateToFilter, KalturaAssetOrderBy? orderBy, KalturaScheduledRecordingAssetType scheduledRecordingType,
                                        KalturaDynamicOrderBy assetOrder = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (assetOrder != null || orderBy != null)
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value, assetOrder);
            }
            else
            {
                order.m_eOrderBy = OrderBy.NONE;
            }

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            ScheduledRecordingsRequest request = new ScheduledRecordingsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userID,
                domainId = domainId,
                orderBy = order,
                m_dServerTime = getServerTime(),
                channelIds = channelIdsToFilter,
                scheduledRecordingAssetType = CatalogMappings.ConvertKalturaScheduledRecordingAssetType(scheduledRecordingType),
                startDate = startDateToFilter.HasValue ? SerializationUtils.ConvertFromUnixTimestamp(startDateToFilter.Value) : new DateTime?(),
                endDate = endDateToFilter.HasValue ? SerializationUtils.ConvertFromUnixTimestamp(endDateToFilter.Value) : new DateTime?()

            };

            // fire request
            UnifiedSearchResponse scheduledRecordingResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out scheduledRecordingResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (scheduledRecordingResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(scheduledRecordingResponse.status.Code, scheduledRecordingResponse.status.Message);
            }

            if (scheduledRecordingResponse.searchResults != null && scheduledRecordingResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = scheduledRecordingResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request, CacheDuration);
                result.TotalCount = scheduledRecordingResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaLastPositionListResponse GetAssetsLastPositionBookmarks(string siteGuid, int groupId, int domainId, string udid, List<KalturaSlimAsset> assets)
        {
            List<KalturaLastPosition> result = null;
            List<AssetBookmarkRequest> assetsToRequestPositions = new List<AssetBookmarkRequest>();

            foreach (KalturaSlimAsset asset in assets)
            {
                AssetBookmarkRequest assetInfo = new AssetBookmarkRequest();
                assetInfo.AssetID = asset.Id;
                bool addToRequest = true;
                switch (asset.Type)
                {
                    case KalturaAssetType.media:
                        assetInfo.AssetType = eAssetTypes.MEDIA;
                        break;
                    case KalturaAssetType.recording:
                        assetInfo.AssetType = eAssetTypes.NPVR;
                        break;
                    case KalturaAssetType.epg:
                        assetInfo.AssetType = eAssetTypes.EPG;
                        break;
                    default:
                        assetInfo.AssetType = eAssetTypes.UNKNOWN;
                        addToRequest = false;
                        break;
                }
                if (addToRequest)
                {
                    assetsToRequestPositions.Add(assetInfo);
                }
            }

            AssetsBookmarksRequest request = new AssetsBookmarksRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid
                },
                Data = new AssetsBookmarksRequestData()
                {
                    Assets = assetsToRequestPositions
                }
            };

            AssetsBookmarksResponse response = null;
            if (!CatalogUtils.GetBaseResponse(request, out response) || response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = CatalogMappings.ConvertBookmarks(response.AssetsBookmarks);

            return new KalturaLastPositionListResponse() { LastPositions = result, TotalCount = response.m_nTotalItems };

        }

        internal KalturaMeta UpdateGroupMeta(int groupId, KalturaMeta meta)
        {
            MetaResponse response = null;
            KalturaMeta result = null;

            try
            {
                Meta apiMeta = null;
                apiMeta = AutoMapper.Mapper.Map<Meta>(meta);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.Module.UpdateGroupMeta(groupId, apiMeta);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateGroupMeta.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.MetaList != null && response.MetaList.Count > 0)
            {
                result = AutoMapper.Mapper.Map<KalturaMeta>(response.MetaList[0]);
            }

            return result;
        }

        internal KalturaTagListResponse SearchTags(int groupId, bool isExcatValue, string value, int topicId, string searchLanguage, int pageIndex, int pageSize)
        {
            KalturaTagListResponse result = new KalturaTagListResponse();
            TagResponse response = null;

            List<TagValue> tagValues = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int searchLanguageId = Utils.Utils.GetLanguageId(groupId, searchLanguage);                    
                    response = Core.Catalog.CatalogManagement.CatalogManager.SearchTags(groupId, isExcatValue, value, topicId, searchLanguageId, pageIndex, pageSize);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling API service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.TagValues != null && response.TagValues.Count > 0)
            {
                result.TotalCount = response.TotalItems;
                // convert TagValues            
                result.Tags = Mapper.Map<List<KalturaTag>>(response.TagValues);
            }

            return result;
        }

        internal KalturaTag AddTag(int groupId, KalturaTag tag, long userId)
        {
            KalturaTag responseTag = new KalturaTag();
            TagResponse response = null;

            try
            {
                TagValue requestTag = AutoMapper.Mapper.Map<TagValue>(tag);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.AddTag(groupId, requestTag, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.TagValues != null && response.TagValues.Count > 0)
            {
                responseTag = AutoMapper.Mapper.Map<KalturaTag>(response.TagValues[0]);
            }

            return responseTag;
        }

        internal KalturaTag UpdateTag(int groupId, long id, KalturaTag tag, long userId)
        {
            KalturaTag responseTag = new KalturaTag();
            TagResponse response = null;

            try
            {
                TagValue tagToUpdate = AutoMapper.Mapper.Map<TagValue>(tag);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.UpdateTag(groupId, id, tagToUpdate, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.TagValues != null && response.TagValues.Count > 0)
            {
                responseTag = AutoMapper.Mapper.Map<KalturaTag>(response.TagValues[0]);
            }

            return responseTag;
        }

        internal bool DeleteTag(int groupId, long id, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.CatalogManager.DeleteTag(groupId, id, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaImageTypeListResponse GetImageTypes(int groupId, bool isSearchByIds, List<long> ids)
        {

            KalturaImageTypeListResponse result = new KalturaImageTypeListResponse();
            ImageTypeListResponse response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.GetImageTypes(groupId, isSearchByIds, ids);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling API service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.ImageTypes != null && response.ImageTypes.Count > 0)
            {
                result.TotalCount = response.TotalItems;
                // convert ImageTypes            
                result.ImageTypes = Mapper.Map<List<KalturaImageType>>(response.ImageTypes);
            }

            return result;
        }

        internal KalturaImageType AddImageType(int groupId, long userId, KalturaImageType imageType)
        {
            KalturaImageType responseImageType = new KalturaImageType();
            ImageTypeResponse response = null;

            try
            {
                ImageType requestImageType = AutoMapper.Mapper.Map<ImageType>(imageType);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.AddImageType(groupId, requestImageType, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.ImageType != null)
            {
                responseImageType = AutoMapper.Mapper.Map<KalturaImageType>(response.ImageType);
            }

            return responseImageType;
        }

        internal KalturaImageType UpdateImageType(int groupId, long userId, long id, KalturaImageType imageType)
        {
            KalturaImageType responseImageType = new KalturaImageType();
            ImageTypeResponse response = null;

            try
            {
                ImageType requestImageType = AutoMapper.Mapper.Map<ImageType>(imageType);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.UpdateImageType(groupId, id, requestImageType, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.ImageType != null)
            {
                responseImageType = AutoMapper.Mapper.Map<KalturaImageType>(response.ImageType);
            }

            return responseImageType;
        }

        internal bool DeleteImageType(int groupId, long userId, long id)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.DeleteImageType(groupId, id, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaRatioListResponse GetRatios(int groupId)
        {
            KalturaRatioListResponse result = new KalturaRatioListResponse();
            RatioListResponse response = null;

            List<ImageType> imageTypes = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.GetRatios(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling API service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Ratios != null && response.Ratios.Count > 0)
            {
                result.TotalCount = response.TotalItems;
                // convert ImageTypes            
                result.Ratios = Mapper.Map<List<KalturaRatio>>(response.Ratios);
            }

            return result;
        }

        internal KalturaImageListResponse GetImagesByIds(int groupId, List<long> imagesIds, bool? isDefault = null)
        {
            KalturaImageListResponse imagesResponse = new KalturaImageListResponse();
            ImageListResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.GetImagesByIds(groupId, imagesIds, isDefault);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Images != null)
            {
                imagesResponse.Images = AutoMapper.Mapper.Map<List<KalturaImage>>(response.Images);
                imagesResponse.TotalCount = imagesResponse.Images.Count;
            }

            return imagesResponse;
        }

        internal KalturaImageListResponse GetImagesByObject(int groupId, long imageObjectId, KalturaImageObjectType imageObjectType, bool? isDefault = null)
        {
            KalturaImageListResponse imagesResponse = new KalturaImageListResponse();
            ImageListResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.GetImagesByObject(groupId, imageObjectId, CatalogMappings.ConvertImageObjectType(imageObjectType), isDefault);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Images != null)
            {
                imagesResponse.Images = AutoMapper.Mapper.Map<List<KalturaImage>>(response.Images);
                imagesResponse.TotalCount = imagesResponse.Images.Count;
            }

            return imagesResponse;
        }

        internal bool DeleteImage(int groupId, long userId, long id)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.DeleteImage(groupId, id, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaImage AddImage(int groupId, long userId, KalturaImage image)
        {
            KalturaImage responseImage = new KalturaImage();
            ImageResponse response = null;

            try
            {
                Image requestImage = AutoMapper.Mapper.Map<Image>(image);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.AddImage(groupId, requestImage, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Image != null)
            {
                responseImage = AutoMapper.Mapper.Map<KalturaImage>(response.Image);
            }

            return responseImage;
        }

        internal void SetContent(int groupId, long userId, long id, string url)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.SetContent(groupId, userId, id, url);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
        }

        internal KalturaRatio AddRatio(int groupId, long userId, KalturaRatio ratio)
        {
            KalturaRatio responseRatio = new KalturaRatio();
            RatioResponse response = null;

            try
            {
                Core.Catalog.CatalogManagement.Ratio requestRatio = AutoMapper.Mapper.Map<Core.Catalog.CatalogManagement.Ratio>(ratio);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.AddRatio(groupId, userId, requestRatio);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Ratio != null)
            {
                responseRatio = AutoMapper.Mapper.Map<KalturaRatio>(response.Ratio);
            }

            return responseRatio;
        }

        internal KalturaRatio UpdateRatio(int groupId, long userId, KalturaRatio ratio, long ratioId)
        {
            KalturaRatio responseRatio = new KalturaRatio();
            RatioResponse response = null;

            try
            {
                Core.Catalog.CatalogManagement.Ratio requestRatio = AutoMapper.Mapper.Map<Core.Catalog.CatalogManagement.Ratio>(ratio);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.UpdateRatio(groupId, userId, requestRatio, ratioId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Ratio != null)
            {
                responseRatio = AutoMapper.Mapper.Map<KalturaRatio>(response.Ratio);
            }

            return responseRatio;
        }

        public KalturaMediaFileTypeListResponse GetMediaFileTypes(int groupId)
        {
            KalturaMediaFileTypeListResponse result = new KalturaMediaFileTypeListResponse() { TotalCount = 0 };
            AssetFileTypeListResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.FileManager.GetMediaFileTypes(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Types != null && response.Types.Count > 0)
            {
                result.TotalCount = response.Types.Count;
                result.Types = new List<KalturaMediaFileType>();
                foreach (MediaFileType assetFileType in response.Types)
                {
                    result.Types.Add(AutoMapper.Mapper.Map<KalturaMediaFileType>(assetFileType));
                }
            }

            return result;
        }

        public KalturaMediaFileType AddMediaFileType(int groupId, KalturaMediaFileType mediaFileType, long userId)
        {
            KalturaMediaFileType result = null;
            MediaFileTypeResponse response = null;

            try
            {
                MediaFileType mediaFileTypeToAdd = AutoMapper.Mapper.Map<MediaFileType>(mediaFileType);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.FileManager.AddMediaFileType(groupId, mediaFileTypeToAdd, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaMediaFileType>(response.MediaFileType);
            return result;
        }

        public KalturaMediaFileType UpdateMediaFileType(int groupId, long id, KalturaMediaFileType mediaFileType, long userId)
        {
            KalturaMediaFileType result = null;
            MediaFileTypeResponse response = null;

            try
            {
                MediaFileType mediaFileTypeToUpdate = AutoMapper.Mapper.Map<MediaFileType>(mediaFileType);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.FileManager.UpdateMediaFileType(groupId, id, mediaFileTypeToUpdate, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaMediaFileType>(response.MediaFileType);
            return result;
        }

        public bool DeleteMediaFileType(int groupId, long id, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.FileManager.DeleteMediaFileType(groupId, id, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaMediaFile AddMediaFile(int groupId, KalturaMediaFile assetFile, long userId)
        {
            KalturaMediaFile result = null;
            AssetFileResponse response = null;

            try
            {
                AssetFile assetFileToAdd = AutoMapper.Mapper.Map<AssetFile>(assetFile);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // Asset type should always eAssetTypes.MEDIA
                    response = Core.Catalog.CatalogManagement.FileManager.InsertMediaFile(groupId, userId, assetFileToAdd);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaMediaFile>(response.File);
            return result;
        }

        internal bool DeleteMediaFile(int groupId, long userId, long id)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.FileManager.DeleteMediaFile(groupId, userId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaMediaFile UpdateMediaFile(int groupId, long id, KalturaMediaFile assetFile, long userId)
        {
            KalturaMediaFile result = null;
            AssetFileResponse response = null;

            try
            {
                AssetFile assetFileToUpdate = AutoMapper.Mapper.Map<AssetFile>(assetFile);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    assetFileToUpdate.Id = id;
                    response = Core.Catalog.CatalogManagement.FileManager.UpdateMediaFile(groupId,  assetFileToUpdate, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }


            result = AutoMapper.Mapper.Map<KalturaMediaFile>(response.File);
            return result;
        }

        internal KalturaMediaFileListResponse GetMediaFiles(int groupId, long id, long assetId)
        {
            KalturaMediaFileListResponse result = new KalturaMediaFileListResponse() { TotalCount = 0 };
            AssetFileListResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.FileManager.GetMediaFiles(groupId, id, assetId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Files != null && response.Files.Count > 0)
            {
                result.TotalCount = response.Files.Count;
                result.Files = new List<KalturaMediaFile>();
                foreach (AssetFile assetFile in response.Files)
                {
                    result.Files.Add(AutoMapper.Mapper.Map<KalturaMediaFile>(assetFile));
                }
            }

            return result;
        }

        internal KalturaChannelListResponse SearchChannels(int groupId, bool isExcatValue, string value, int pageIndex, int pageSize, KalturaChannelsOrderBy channelOrderBy, bool isOperatorSearch)
        {
            KalturaChannelListResponse result = new KalturaChannelListResponse();
            Core.Catalog.CatalogManagement.ChannelListResponse response = null;

            List<GroupsCacheManager.Channel> channels = null;
            ChannelOrderBy orderBy = ChannelOrderBy.Id;
            OrderDir orderDirection = OrderDir.NONE;

            switch (channelOrderBy)
            {
                case KalturaChannelsOrderBy.NONE:
                    {
                        orderBy = ChannelOrderBy.Id;
                        orderDirection = OrderDir.DESC;
                        break;
                    }
                case KalturaChannelsOrderBy.NAME_ASC:
                    {
                        orderBy = ChannelOrderBy.Name;
                        orderDirection = OrderDir.ASC;
                        break;
                    }
                case KalturaChannelsOrderBy.NAME_DESC:
                    {
                        orderBy = ChannelOrderBy.Name;
                        orderDirection = OrderDir.DESC;
                        break;
                    }
                case KalturaChannelsOrderBy.CREATE_DATE_ASC:
                    {
                        orderBy = ChannelOrderBy.CreateDate;
                        orderDirection = OrderDir.ASC;
                        break;
                    }
                case KalturaChannelsOrderBy.CREATE_DATE_DESC:
                    {
                        orderBy = ChannelOrderBy.CreateDate;
                        orderDirection = OrderDir.DESC;
                        break;
                    }
                default:
                    break;
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ChannelManager.SearchChannels(groupId, isExcatValue, value, pageIndex, pageSize, orderBy, orderDirection, isOperatorSearch);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling SearchChannels. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Channels != null && response.Channels.Count > 0)
            {
                result.Channels = new List<KalturaChannel>();                
                // convert channels
                List<KalturaDynamicChannel> dynamicChannels = Mapper.Map<List<KalturaDynamicChannel>>(response.Channels.Where(x => x.m_nChannelTypeID == (int)GroupsCacheManager.ChannelType.KSQL).ToList());
                List<KalturaManualChannel> manualChannels = Mapper.Map<List<KalturaManualChannel>>(response.Channels.Where(x => x.m_nChannelTypeID == (int)GroupsCacheManager.ChannelType.Manual).ToList());
                if (dynamicChannels != null)
                {
                    result.Channels.AddRange(dynamicChannels);
                }

                if (manualChannels != null && manualChannels.Count > 0)
                {
                    result.Channels.AddRange(manualChannels);
                }

                result.TotalCount = response.TotalItems;
            }

            return result;
        }

        internal KalturaChannel GetChannel(int groupId, int channelId, bool isOperatorSearch)
        {
            Core.Catalog.CatalogManagement.ChannelResponse response = null;
            KalturaChannel result = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ChannelManager.GetChannel(groupId, channelId, isOperatorSearch);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while calling GetChannel. groupId: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            // DynamicChannel
            if (response.Channel.m_nChannelTypeID == 4)
            {
                result = Mapper.Map<KalturaDynamicChannel>(response.Channel);
            }
            // Should only be manual channel
            else
            {
                result = Mapper.Map<KalturaManualChannel>(response.Channel);
            }

            return result;
        }

        internal KalturaChannel InsertKSQLChannel(int groupId, KalturaChannel channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannel result = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = APILogic.CRUD.KSQLChannelsManager.Insert(groupId, request);                 
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while calling InsertKSQLChannel. groupId: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            result = Mapper.Map<KalturaChannel>(response.Channel);
            return result;
        }

        internal KalturaChannel InsertChannel(int groupId, KalturaChannel channel, long userId)
        {
            KalturaChannel result = null;
            Core.Catalog.CatalogManagement.ChannelResponse response = null;

            try
            {
                GroupsCacheManager.Channel channelToAdd = AutoMapper.Mapper.Map<GroupsCacheManager.Channel>(channel);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ChannelManager.AddChannel(groupId, channelToAdd, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // DynamicChannel
            if (response.Channel.m_nChannelTypeID == 4)
            {
                result = Mapper.Map<KalturaDynamicChannel>(response.Channel);
            }
            // Should only be manual channel
            else
            {
                result = Mapper.Map<KalturaManualChannel>(response.Channel);
            }

            return result;
        }

        internal KalturaChannel UpdateChannel(int groupId, int  id, KalturaChannel channel, long userId)
        {
            KalturaChannel result = null;
            Core.Catalog.CatalogManagement.ChannelResponse response = null;

            try
            {
                GroupsCacheManager.Channel channelToUpdate = AutoMapper.Mapper.Map<GroupsCacheManager.Channel>(channel);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ChannelManager.UpdateChannel(groupId, id, channelToUpdate, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // DynamicChannel
            if (response.Channel.m_nChannelTypeID == 4)
            {
                result = Mapper.Map<KalturaDynamicChannel>(response.Channel);
            }
            // Should only be manual channel
            else
            {
                result = Mapper.Map<KalturaManualChannel>(response.Channel);
            }

            return result;
        }

        internal KalturaChannel SetKSQLChannel(int groupId, KalturaChannel channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannel profile = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.SetKSQLChannel(groupId, request, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetKSQLChannel. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            profile = Mapper.Map<KalturaChannel>(response.Channel);
            return profile;
        }

        [Obsolete]
        internal KalturaChannelProfile InsertKSQLChannelProfile(int groupId, KalturaChannelProfile channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = APILogic.CRUD.KSQLChannelsManager.Insert(groupId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertKSQLChannel.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            profile = Mapper.Map<Models.API.KalturaChannelProfile>(response.Channel);
            return profile;
        }

        [Obsolete]
        internal KalturaChannelProfile SetKSQLChannelProfile(int groupId, KalturaChannelProfile channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.SetKSQLChannel(groupId, request, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetKSQLChannel. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            profile = Mapper.Map<KalturaChannelProfile>(response.Channel);
            return profile;
        }

        internal bool DeleteChannel(int groupId, int channelId, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ChannelManager.DeleteChannel(groupId, channelId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteKSQLChannel.  groupID: {0}, channelId: {1}, exception: {2}", groupId, channelId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal KalturaChannelProfile GetKSQLChannel(int groupId, int channelId)
        {
            KalturaChannelProfile profile = null;
            KSQLChannelResponse response = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetKSQLChannel(groupId, channelId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetKSQLChannel. groupID: {0}, channelId: {1}, exception: {2}", groupId, channelId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            profile = Mapper.Map<KalturaChannelProfile>(response.Channel);

            return profile;
        }

    }
}