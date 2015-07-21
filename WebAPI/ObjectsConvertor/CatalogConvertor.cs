using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.ObjectsConvertor
{
    public class CatalogConvertor
    {
        public delegate List<IAssetable> ConvertAssetsDelegate(int GroupId, List<BaseObject> assetBaseList, List<With> withList);

        public static List<IAssetable> ConvertBaseObjectsToAssetsInfo(int groupId, List<BaseObject> assetBaseList, List<With> withList)
        {
            List<IAssetable> finalResults = new List<IAssetable>();

            List<AssetStats> mediaAssetsStats = new List<AssetStats>();
            List<AssetStats> epgAssetsStats = new List<AssetStats>();

            if (withList != null)
            {
                if (withList.Contains(With.stats))
                {
                    long minDateTimeMin = SerializationUtils.ConvertToUnixTimestamp(DateTime.MinValue);
                    long minDateTimeMax = SerializationUtils.ConvertToUnixTimestamp(DateTime.MaxValue);

                    // get media IDs for assets statistics
                    List<int> mediaBaseListIds = assetBaseList.Where(m => m.AssetType == eAssetTypes.MEDIA).Select(x => int.Parse(x.AssetId)).ToList();
                    if (mediaBaseListIds != null && mediaBaseListIds.Count > 0)
                        mediaAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, mediaBaseListIds, Mapper.Map<StatsType>(AssetType.media));

                    // get EPG IDs for assets statistics
                    List<int> epgBaseListIds = assetBaseList.Select(e => int.Parse(e.AssetId)).ToList();
                    if (epgBaseListIds != null && epgBaseListIds.Count > 0)
                        epgAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, epgBaseListIds, Mapper.Map<StatsType>(AssetType.epg));
                }
            }
            foreach (var item in assetBaseList)
            {
                var assetInfo = Mapper.Map<AssetInfo>(item);

                if (withList != null)
                {
                    // get files data (media only)
                    if (withList.Contains(With.files) && item.AssetType == eAssetTypes.MEDIA)
                        assetInfo.Files = Mapper.Map<List<File>>(((MediaObj)item).m_lFiles);

                    // get images data
                    if (withList.Contains(With.images))
                    {
                        if (item.AssetType == eAssetTypes.MEDIA)
                            assetInfo.Images = Mapper.Map<List<Image>>(((MediaObj)item).m_lPicture);
                        else
                            assetInfo.Images = Mapper.Map<List<Image>>(((ProgramObj)item).m_oProgram.EPG_PICTURES);
                    }

                    // get statistics data
                    if (withList.Contains(With.stats))
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

        public static List<IAssetable> ConvertBaseObjectsToSlimAssetsInfo(int groupId, List<BaseObject> assetBaseList, List<With> with)
        {
            List<IAssetable> result = new List<IAssetable>();

            List<AssetStats> mediaAssetsStats = new List<AssetStats>();
            List<AssetStats> epgAssetsStats = new List<AssetStats>();

            if (with != null)
            {
                if (with.Contains(With.stats))
                {
                    long minDateTimeMin = SerializationUtils.ConvertToUnixTimestamp(DateTime.MinValue);
                    long minDateTimeMax = SerializationUtils.ConvertToUnixTimestamp(DateTime.MaxValue);

                    // get media IDs for assets statistics
                    List<int> mediaBaseListIds = assetBaseList.Where(m => m.AssetType == eAssetTypes.MEDIA).Select(x => int.Parse(x.AssetId)).ToList();
                    if (mediaBaseListIds != null && mediaBaseListIds.Count > 0)
                        mediaAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, mediaBaseListIds, Mapper.Map<StatsType>(AssetType.media));

                    // get EPG IDs for assets statistics
                    List<int> epgBaseListIds = assetBaseList.Select(e => int.Parse(e.AssetId)).ToList();
                    if (epgBaseListIds != null && epgBaseListIds.Count > 0)
                        epgAssetsStats = ClientsManager.CatalogClient().GetAssetsStats(groupId, string.Empty, epgBaseListIds, Mapper.Map<StatsType>(AssetType.epg));
                }
            }

            foreach (var item in assetBaseList)
            {
                var assetInfo = Mapper.Map<SlimAssetInfo>(item);

                if (with != null)
                {
                    // get files data (media only)
                    if (with.Contains(With.files) && item.AssetType == eAssetTypes.MEDIA)
                        assetInfo.Files = Mapper.Map<List<File>>(((MediaObj)item).m_lFiles);

                    // get images data
                    if (with.Contains(With.images))
                    {
                        if (item.AssetType == eAssetTypes.MEDIA)
                            assetInfo.Images = Mapper.Map<List<Image>>(((MediaObj)item).m_lPicture);
                        else
                            assetInfo.Images = Mapper.Map<List<Image>>(((ProgramObj)item).m_oProgram.EPG_PICTURES);
                    }

                    // get statistics data
                    if (with.Contains(With.stats))
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

        public static OrderObj ConvertOrderToOrderObj(Order order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case Order.a_to_z:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case Order.z_to_a:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Order.views:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Order.ratings:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Order.votes:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Order.newest:
                    result.m_eOrderBy = OrderBy.CREATE_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Order.relevancy:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
            }
            return result;
        }
    }
}
