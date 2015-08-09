using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Catalog;
using WebAPI.Models;
using WebAPI.Utils;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class CatalogMappings
    {
        public static void RegisterMappings()
        {
            //MediaPicture to Image
            Mapper.CreateMap<Picture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.m_sURL))
                 .ForMember(dest => dest.Height, opt => opt.MapFrom(src => GetPictureHeight(src.m_sSize)))
                 .ForMember(dest => dest.Width, opt => opt.MapFrom(src => GetPictureWidth(src.m_sSize)))
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.ratio));

            //EPGPicture to Image
            Mapper.CreateMap<EpgPicture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.PicHeight))
                 .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.PicWidth))
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.Ratio));

            //File 
            Mapper.CreateMap<FileMedia, KalturaMediaFile>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nMediaID))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nFileId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_sFileFormat))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.m_sUrl));

            //BuzzScore
            Mapper.CreateMap<BuzzWeightedAverScore, KalturaBuzzScore>()
                 .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                 .ForMember(dest => dest.NormalizedAvgScore, opt => opt.MapFrom(src => src.NormalizedWeightedAverageScore))
                 .ForMember(dest => dest.AvgScore, opt => opt.MapFrom(src => src.WeightedAverageScore));

            //AssetStats 
            Mapper.CreateMap<AssetStatsResult, KalturaAssetStatistics>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nAssetID))
                 .ForMember(dest => dest.Likes, opt => opt.MapFrom(src => src.m_nLikes))
                 .ForMember(dest => dest.Views, opt => opt.MapFrom(src => src.m_nViews))
                 .ForMember(dest => dest.RatingCount, opt => opt.MapFrom(src => src.m_nVotes))
                 .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.m_dRate))
                 .ForMember(dest => dest.BuzzAvgScore, opt => opt.MapFrom(src => src.m_buzzAverScore));

            //Media to AssetInfo
            Mapper.CreateMap<MediaObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            Mapper.CreateMap<EPGChannelProgrammeObject, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            Mapper.CreateMap<ProgramObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src.m_oProgram)));

            //Media to SlimAssetInfo
            Mapper.CreateMap<MediaObj, KalturaSlimAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID));

            //Media to SlimAssetInfo
            Mapper.CreateMap<ProgramObj, KalturaSlimAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)KalturaAssetType.epg));

            //channelObj to Channel
            Mapper.CreateMap<channelObj, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sTitle))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPic));


            //Channel (Catalog) to Channel
            Mapper.CreateMap<WebAPI.Catalog.Channel, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.MediaTypes, opt => opt.MapFrom(src => src.m_nMediaType));

            //CategoryResponse to Category
            Mapper.CreateMap<CategoryResponse, WebAPI.Models.Catalog.KalturaOTTCategory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sTitle))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.m_nParentCategoryID))
                .ForMember(dest => dest.ChildCategories, opt => opt.MapFrom(src => src.m_oChildCategories))
                .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => src.m_oChannels))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPics));
        }

        private static int GetPictureWidth(string size)
        {
            string[] sizeArr = size.ToLower().Split('x');

            if (sizeArr != null && sizeArr.Length == 2)
            {
                int width;

                if (int.TryParse(sizeArr[0].Trim(), out width))
                {
                    return width;
                }
            }

            return 0;
        }

        private static int GetPictureHeight(string size)
        {
            string[] sizeArr = size.ToLower().Split('x');

            if (sizeArr != null && sizeArr.Length == 2)
            {
                int height;

                if (int.TryParse(sizeArr[1].Trim(), out height))
                {
                    return height;
                }
            }

            return 0;
        }

        private static Dictionary<string, string> BuildExtraParamsDictionary(EPGChannelProgrammeObject epg)
        {
            Dictionary<string, string>  extraParams = new Dictionary<string, string>();
            
            extraParams.Add("epg_channel_id ", epg.EPG_CHANNEL_ID);
            extraParams.Add("epg_id", epg.EPG_IDENTIFIER);
            extraParams.Add("related_media_id", epg.media_id);

            return extraParams;
        }

        private static Dictionary<string, List<string>> BuildTagsDictionary(List<EPGDictionary> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
            List<string> tagsList;

            foreach (var tag in list)
            {
                if (tags.ContainsKey(tag.Key))
                {
                    tags[tag.Key].Add(tag.Value);
                }
                else
                {
                    tagsList = new List<string>();
                    tagsList.Add(tag.Value);
                    tags.Add(tag.Key, tagsList);
                }
            }
            return tags;
        }

        private static Dictionary<string, string> BuildMetasDictionary(List<EPGDictionary> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, string> metas = new Dictionary<string, string>();

            foreach (var meta in list)
            {
                metas.Add(meta.Key, meta.Value);
            }

            return metas;
        }

        private static Dictionary<string, string> BuildExtraParamsDictionary(MediaObj media)
        {
            Dictionary<string, string> extraParams = new Dictionary<string, string>();

            extraParams.Add("sys_start_date", SerializationUtils.ConvertToUnixTimestamp(media.m_dStartDate).ToString());
            extraParams.Add("sys_final_date", SerializationUtils.ConvertToUnixTimestamp(media.m_dFinalDate).ToString());
            extraParams.Add("external_ids", media.m_ExternalIDs);

            return extraParams;

        }

        private static Dictionary<string, List<string>> BuildTagsDictionary(List<Tags> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();

            foreach (var tag in list)
            {
                tags.Add(tag.m_oTagMeta.m_sName, tag.m_lValues);
            }

            return tags;
        }

        private static Dictionary<string, string> BuildMetasDictionary(List<Metas> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, string> metas = new Dictionary<string,string>();

            foreach (var meta in list)
            {
                metas.Add(meta.m_oTagMeta.m_sName, meta.m_sValue);
            }

            return metas;
        }
    }
}