using ApiObjects;
using System.Collections.Generic;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class RuleActionMapper
    {
        public static List<string> GetAudioCodecs(this KalturaFilterFileByAudioCodecAction model)
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(model.AudioCodecIn, "audioCodecIn", true);
            return types;
        }

        public static HashSet<long> GetFileTypesIds(this KalturaFilterFileByFileTypeIdAction model)
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<HashSet<long>, long>(model.FileTypeIdIn, "fileTypeIdIn", true);
            return types;
        }

        public static List<eAssetTypes> GetAssetTypes(this KalturaFilterFileByFileTypeIdForAssetTypeAction model)
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<KalturaAssetType>, KalturaAssetType>(model.AssetTypeIn, "assetTypeIn", true, true);
            var mapped = AutoMapper.Mapper.Map<List<eAssetTypes>>(types);
            return mapped;
        }

        public static List<string> GetLabels(this KalturaFilterFileByLabelAction model)
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(model.LabelIn, "labelIn", true);
            return types;
        }

        public static List<MediaFileTypeQuality> GetQualities(this KalturaFilterFileByQualityAction model)
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<KalturaMediaFileTypeQuality>, KalturaMediaFileTypeQuality>(model.QualityIn, "qualityIn", true, true);
            return AutoMapper.Mapper.Map<List<MediaFileTypeQuality>>(types);
        }

        public static List<StreamerType> GetStreamerTypes(this KalturaFilterFileByStreamerTypeAction model)
        {
            var streamerTypes = Utils.Utils.ParseCommaSeparatedValues<List<KalturaMediaFileStreamerType>, KalturaMediaFileStreamerType>(model.StreamerTypeIn, "streamerTypeIn", true, true);
            return AutoMapper.Mapper.Map<List<StreamerType>>(streamerTypes);
        }

        public static List<string> GetVideoCodecs(this KalturaFilterFileByVideoCodecAction model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(model.VideoCodecIn, "videoCodecIn", true);
        }
    }
}