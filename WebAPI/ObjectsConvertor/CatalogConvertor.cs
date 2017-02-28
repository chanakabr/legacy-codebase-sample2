using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.Utils;
using WebAPI.ObjectsConvertor.Mapping.Utils;
using WebAPI.ObjectsConvertor.Mapping;
using Core.Catalog;
using ApiObjects.SearchObjects;
using ApiObjects;
using Core.Catalog.Response;

namespace WebAPI.ObjectsConvertor
{
    public class CatalogConvertor
    {
        public delegate List<KalturaIAssetable> ConvertAssetsDelegate(int GroupId, List<BaseObject> assetBaseList, List<KalturaCatalogWith> withList);

        public static List<KalturaIAssetable> ConvertBaseObjectsToAssetsInfo(int groupId, List<BaseObject> assetBaseList, List<KalturaCatalogWith> withList)
        {
            List<KalturaIAssetable> finalResults = new List<KalturaIAssetable>();

            List<KalturaAssetStatistics> mediaAssetsStats = new List<KalturaAssetStatistics>();
            List<KalturaAssetStatistics> epgAssetsStats = new List<KalturaAssetStatistics>();

            if (withList != null)
            {
                if (withList.Contains(KalturaCatalogWith.stats))
                {
                    long minDateTimeMin = SerializationUtils.ConvertToUnixTimestamp(DateTime.MinValue);
                    long minDateTimeMax = SerializationUtils.ConvertToUnixTimestamp(DateTime.MaxValue);

                    // get media IDs for assets statistics
                    List<int> mediaBaseListIds = assetBaseList.Where(m => m.AssetType == eAssetTypes.MEDIA).Select(x => int.Parse(x.AssetId)).ToList();
                    if (mediaBaseListIds != null && mediaBaseListIds.Count > 0)
                        mediaAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, mediaBaseListIds, KalturaAssetType.media);

                    // get EPG IDs for assets statistics
                    List<int> epgBaseListIds = assetBaseList.Where(m => m.AssetType == eAssetTypes.EPG).Select(e => int.Parse(e.AssetId)).ToList();
                    if (epgBaseListIds != null && epgBaseListIds.Count > 0)
                        epgAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, epgBaseListIds, KalturaAssetType.epg);
                }
            }
            foreach (var item in assetBaseList)
            {
                var assetInfo = Mapper.Map<KalturaAssetInfo>(item);

                if (withList != null)
                {
                    // get files data (media only)
                    if (withList.Contains(KalturaCatalogWith.files) && item.AssetType == eAssetTypes.MEDIA)
                        assetInfo.MediaFiles = Mapper.Map<List<KalturaMediaFile>>(((MediaObj)item).m_lFiles);

                    // get images data
                    if (withList.Contains(KalturaCatalogWith.images))
                    {
                        if (item.AssetType == eAssetTypes.MEDIA)
                            assetInfo.Images = Mapper.Map<List<KalturaMediaImage>>(((MediaObj)item).m_lPicture);
                        else
                            assetInfo.Images = Mapper.Map<List<KalturaMediaImage>>(((ProgramObj)item).m_oProgram.EPG_PICTURES);
                    }

                    // get statistics data
                    if (withList.Contains(KalturaCatalogWith.stats))
                    {
                        if (item.AssetType == eAssetTypes.MEDIA)
                            assetInfo.Statistics = mediaAssetsStats != null ? mediaAssetsStats.Where(mas => mas.AssetId == assetInfo.Id).FirstOrDefault() : null;
                        else
                            assetInfo.Statistics = epgAssetsStats != null ? epgAssetsStats.Where(eas => eas.AssetId == assetInfo.Id).FirstOrDefault() : null;
                    }
                }
                finalResults.Add(assetInfo);
            }

            return finalResults;
        }

        public static List<KalturaAssetInfo> ConvertEPGChannelProgrammeObjectToAssetsInfo(int groupId, List<EPGChannelProgrammeObject> epgPrograms, List<KalturaCatalogWith> withList)
        {
            List<KalturaAssetInfo> finalResults = new List<KalturaAssetInfo>();

            List<KalturaAssetStatistics> mediaAssetsStats = new List<KalturaAssetStatistics>();
            List<KalturaAssetStatistics> epgAssetsStats = new List<KalturaAssetStatistics>();

            if (withList != null)
            {
                if (withList.Contains(KalturaCatalogWith.stats))
                {
                    long minDateTimeMin = SerializationUtils.ConvertToUnixTimestamp(DateTime.MinValue);
                    long minDateTimeMax = SerializationUtils.ConvertToUnixTimestamp(DateTime.MaxValue);

                    // get EPG IDs for assets statistics
                    List<int> epgBaseListIds = epgPrograms.Select(e => (int)e.EPG_ID).ToList();
                    if (epgBaseListIds != null && epgBaseListIds.Count > 0)
                        epgAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, epgBaseListIds, KalturaAssetType.epg);
                }
            }
            foreach (var item in epgPrograms)
            {
                var assetInfo = Mapper.Map<KalturaAssetInfo>(item);

                if (withList != null)
                {
                    // get images data
                    if (withList.Contains(KalturaCatalogWith.images))
                    {
                        assetInfo.Images = Mapper.Map<List<KalturaMediaImage>>(item.EPG_PICTURES);
                    }

                    // get statistics data
                    if (withList.Contains(KalturaCatalogWith.stats))
                    {
                        assetInfo.Statistics = epgAssetsStats != null ? epgAssetsStats.Where(eas => eas.AssetId == assetInfo.Id).FirstOrDefault() : null;
                    }
                }
                finalResults.Add(assetInfo);
            }

            return finalResults;
        }

        public static List<KalturaIAssetable> ConvertBaseObjectsToSlimAssetsInfo(int groupId, List<BaseObject> assetBaseList, List<KalturaCatalogWith> with)
        {
            List<KalturaIAssetable> result = new List<KalturaIAssetable>();

            List<KalturaAssetStatistics> mediaAssetsStats = new List<KalturaAssetStatistics>();
            List<KalturaAssetStatistics> epgAssetsStats = new List<KalturaAssetStatistics>();

            if (with != null)
            {
                if (with.Contains(KalturaCatalogWith.stats))
                {
                    long minDateTimeMin = SerializationUtils.ConvertToUnixTimestamp(DateTime.MinValue);
                    long minDateTimeMax = SerializationUtils.ConvertToUnixTimestamp(DateTime.MaxValue);

                    // get media IDs for assets statistics
                    List<int> mediaBaseListIds = assetBaseList.Where(m => m.AssetType == eAssetTypes.MEDIA).Select(x => int.Parse(x.AssetId)).ToList();
                    if (mediaBaseListIds != null && mediaBaseListIds.Count > 0)
                        mediaAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, mediaBaseListIds, KalturaAssetType.media);

                    // get EPG IDs for assets statistics
                    List<int> epgBaseListIds = assetBaseList.Select(e => int.Parse(e.AssetId)).ToList();
                    if (epgBaseListIds != null && epgBaseListIds.Count > 0)
                        epgAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, epgBaseListIds, KalturaAssetType.epg);
                }
            }

            foreach (var item in assetBaseList)
            {
                var assetInfo = Mapper.Map<KalturaBaseAssetInfo>(item);

                if (with != null)
                {
                    // get files data (media only)
                    if (with.Contains(KalturaCatalogWith.files) && item.AssetType == eAssetTypes.MEDIA)
                        assetInfo.MediaFiles = Mapper.Map<List<KalturaMediaFile>>(((MediaObj)item).m_lFiles);

                    // get images data
                    if (with.Contains(KalturaCatalogWith.images))
                    {
                        if (item.AssetType == eAssetTypes.MEDIA)
                            assetInfo.Images = Mapper.Map<List<KalturaMediaImage>>(((MediaObj)item).m_lPicture);
                        else
                            assetInfo.Images = Mapper.Map<List<KalturaMediaImage>>(((ProgramObj)item).m_oProgram.EPG_PICTURES);
                    }

                    // get statistics data
                    if (with.Contains(KalturaCatalogWith.stats))
                    {
                        if (item.AssetType == eAssetTypes.MEDIA)
                            assetInfo.Statistics = mediaAssetsStats != null ? mediaAssetsStats.Where(mas => mas.AssetId == assetInfo.Id).FirstOrDefault() : null;
                        else
                            assetInfo.Statistics = epgAssetsStats != null ? epgAssetsStats.Where(eas => eas.AssetId == assetInfo.Id).FirstOrDefault() : null;
                    }
                }
                result.Add(assetInfo);
            }

            return result;
        }

        public static CutWith ConvertCutWith(WebAPI.Models.Catalog.KalturaAssetInfoFilter.KalturaCutWith cutWith)
        {
            switch (cutWith)
            {
                case KalturaAssetInfoFilter.KalturaCutWith.and:
                    return CutWith.AND;
                case KalturaAssetInfoFilter.KalturaCutWith.or:
                    return CutWith.OR;
                default:
                    throw new ArgumentException("Unknown cut type");
            }
        }

        public static OrderObj ConvertOrderToOrderObj(KalturaOrder order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case KalturaOrder.a_to_z:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaOrder.z_to_a:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaOrder.views:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaOrder.ratings:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaOrder.votes:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaOrder.newest:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaOrder.relevancy:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaOrder.oldest_first:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaOrder.likes:
                    result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
            }
            return result;
        }

        public static OrderObj ConvertOrderToOrderObj(KalturaAssetOrderBy order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case KalturaAssetOrderBy.NAME_ASC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.LIKES_DESC:
                    {
                        result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                        result.m_eOrderDir = OrderDir.DESC;
                        break;
                    }
            }
            return result;
        }

        public static OrderObj ConvertOrderToOrderObj(KalturaAssetCommentOrderBy order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case KalturaAssetCommentOrderBy.CREATE_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;              
            }
            return result;
        }


        public static List<KalturaEPGChannelAssets> ConvertEPGChannelAssets(int groupId, List<EpgResultsObj> epgResultsList, List<KalturaCatalogWith> withList)
        {
            List<KalturaEPGChannelAssets> finalResults = new List<KalturaEPGChannelAssets>();
            KalturaEPGChannelAssets channel = null;

            foreach (var epgResult in epgResultsList)
            {
                channel = new KalturaEPGChannelAssets();
                channel.TotalCount = epgResult.m_nTotalItems;
                channel.ChannelID = epgResult.m_nChannelID;
                channel.Assets = ConvertEPGChannelProgrammeObjectToAssetsInfo(groupId, epgResult.m_lEpgProgram, withList);

                finalResults.Add(channel);
            }

            return finalResults;
        }

    }
}
