using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using Core.Catalog;
using FluentAssertions;
using NUnit.Framework;

namespace ApiLogic.Tests.Api.Managers.Rule
{
    public class FilterFileRuleTests
    {
        private readonly IFilterFileRule _filterFileRule = new FilterFileRule();
        
        [TestCase(new object[]{ "MP1", "aac" }, true)]
        [TestCase(new object[]{ "MP1", "MP2" }, false)]
        public void ShouldFilterByAudioCodec(object[] audioCodecs, bool shouldMatch)
        {
            var audioCodecRule = new[]
                { new FilterFileByAudioCodecInDiscovery { AudioCodecs = new List<string> { "MP3", "AAC" } } };
            
            var fileType = DefaultFileType;
            fileType.AudioCodecs = new HashSet<string>(audioCodecs.Cast<string>());
            var targetFile = new FilterFileRule.Target(fileType, eAssetTypes.EPG, null, null);
            
            _filterFileRule.MatchRules(targetFile, audioCodecRule).Should().Be(shouldMatch);
        }
        
        [TestCase(eAssetTypes.EPG, 1, true)]
        [TestCase(eAssetTypes.EPG, 4, false)]
        [TestCase(eAssetTypes.MEDIA, 2, true)]
        [TestCase(eAssetTypes.MEDIA, 4, false)]
        [TestCase(eAssetTypes.NPVR, 1, true)]
        [TestCase(eAssetTypes.NPVR, 4, true)]
        public void ShouldFilterByFileTypeForAssetType(eAssetTypes assetType, long fileTypeId, bool shouldMatch)
        {
            var fileTypeForAssetTypeRule = new[]
            {
                new FilterFileByFileTypeForAssetTypeInDiscovery
                {
                    AssetTypes = new List<eAssetTypes> { eAssetTypes.EPG, eAssetTypes.MEDIA },
                    FileTypeIds = new HashSet<long> { 1, 2, 3 }
                }
            };

            var fileType = new MediaFileType { Id = fileTypeId };
            var targetFile = new FilterFileRule.Target(fileType, assetType, null, null);
            
            _filterFileRule.MatchRules(targetFile, fileTypeForAssetTypeRule).Should().Be(shouldMatch);
        }
        
        [TestCase(1, true)]
        [TestCase(4, false)]
        public void ShouldFilterByFileType(long fileTypeId, bool shouldMatch)
        {
            var fileTypeRule = new[] { new FilterFileByFileTypeInDiscovery { FileTypeIds = new HashSet<long> { 1, 2, 3 } } };

            var fileType = new MediaFileType { Id = fileTypeId };
            var targetFile = new FilterFileRule.Target(fileType, eAssetTypes.EPG, null, null);
            
            _filterFileRule.MatchRules(targetFile, fileTypeRule).Should().Be(shouldMatch);
        }
        
        [TestCase("my_label,Out_Of_Home", true)]
        [TestCase("my_label,in_home", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void ShouldFilterByLabels(string labels, bool shouldMatch)
        {
            var labelRule = new[] { new FilterFileByLabelInDiscovery { Labels = new List<string> { "out_of_home", "second" } } };
            
            var targetFile = new FilterFileRule.Target(null, eAssetTypes.EPG, labels, null);
            
            _filterFileRule.MatchRules(targetFile, labelRule).Should().Be(shouldMatch);
        }
        
        [TestCase(MediaFileTypeQuality.HD_720, true)]
        [TestCase(MediaFileTypeQuality.HD_1080, false)]
        public void ShouldFilterByQuality(MediaFileTypeQuality quality, bool shouldMatch)
        {
            var qualityRule = new[]
            {
                new FilterFileByQualityInDiscovery
                {
                    Qualities = new List<MediaFileTypeQuality>
                    {
                        MediaFileTypeQuality.Adaptive, MediaFileTypeQuality.HD_720
                    }
                }
            };

            var fileType = DefaultFileType;
            fileType.Quality = quality;
            var targetFile = new FilterFileRule.Target(fileType, eAssetTypes.EPG, null, null);
            
            _filterFileRule.MatchRules(targetFile, qualityRule).Should().Be(shouldMatch);
        }
        
        [TestCase(StreamerType.applehttp, true)]
        [TestCase(StreamerType.mpegdash, false)]
        [TestCase(null, false)]
        public void ShouldFilterByStreamerType(StreamerType? streamerType, bool shouldMatch)
        {
            var streamerTypeRule = new[]
            {
                new FilterFileByStreamerTypeInDiscovery()
                {
                    StreamerTypes = new List<StreamerType>
                    {
                        StreamerType.applehttp, StreamerType.multicast
                    }
                }
            };

            var fileType = DefaultFileType;
            fileType.StreamerType = streamerType;
            var targetFile = new FilterFileRule.Target(fileType, eAssetTypes.EPG, null, null);
            
            _filterFileRule.MatchRules(targetFile, streamerTypeRule).Should().Be(shouldMatch);
        }
        
        [TestCase(new object[]{ "FFMpeg", "x264" }, true)]
        [TestCase(new object[]{ "FFMpeg", "x265" }, false)]
        public void ShouldFilterByVideoCodec(object[] videoCodecs, bool shouldMatch)
        {
            var videoCodecRule = new[]
                { new FilterFileByVideoCodecInDiscovery { VideoCodecs = new List<string> { "MP4", "x264" } } };
            
            var fileType = DefaultFileType;
            fileType.VideoCodecs = new HashSet<string>(videoCodecs.Cast<string>());
            var targetFile = new FilterFileRule.Target(fileType, eAssetTypes.EPG, null, null);
            
            _filterFileRule.MatchRules(targetFile, videoCodecRule).Should().Be(shouldMatch);
        }

        [TestCase("key", new[] { "HD", "4K" }, false)]
        [TestCase("quality", new[] { "Full" }, false)]
        [TestCase("quality", new[] { "STB" }, false)]
        [TestCase("quality", new[] { "hd" }, true)]
        public void ShouldFilterByDynamicData(string key, string[] values, bool shouldMatch)
        {
            var dynamicDataRule = new[]
            {
                new FilterFileByDynamicDataInDiscovery { Key = key, Values = values }
            };
            var dynamicData = new Dictionary<string, IEnumerable<string>>
            {
                { "quality", new[] { "HD", "4K" } },
                { "device", new[] { "STB" } }
            };
            var targetFile = new FilterFileRule.Target(DefaultFileType, eAssetTypes.EPG, null, dynamicData);

            var result = _filterFileRule.MatchRules(targetFile, dynamicDataRule);

            result.Should().Be(shouldMatch);
        }

        [TestCase(StreamerType.applehttp, MediaFileTypeQuality.Adaptive, true)]
        [TestCase(StreamerType.mpegdash, MediaFileTypeQuality.Adaptive, false)]
        [TestCase(StreamerType.applehttp, MediaFileTypeQuality.HD_1080, false)]
        public void ShouldMatchAllRules(StreamerType? streamerType, MediaFileTypeQuality quality, bool shouldMatch)
        {
            var streamerTypeRule = new FilterFileByStreamerTypeInDiscovery
            {
                StreamerTypes = new List<StreamerType> { StreamerType.applehttp, StreamerType.multicast }
            };
            var qualityRule = new FilterFileByQualityInDiscovery
            {
                Qualities = new List<MediaFileTypeQuality> { MediaFileTypeQuality.Adaptive, MediaFileTypeQuality.HD_720 }
            };
            var rules = new AssetRuleAction[] { streamerTypeRule, qualityRule };
            
            var fileType = DefaultFileType;
            fileType.StreamerType = streamerType;
            fileType.Quality = quality;
            var targetFile = new FilterFileRule.Target(fileType, eAssetTypes.EPG, null, null);
            
            _filterFileRule.MatchRules(targetFile, rules).Should().Be(shouldMatch);
        }

        private static MediaFileType DefaultFileType => new MediaFileType
        {
            Id = 1,
            Quality = MediaFileTypeQuality.HD_720,
            AudioCodecs = new HashSet<string> { "MP3", "AAC" },
            VideoCodecs = new HashSet<string> { "MPEG", "MP4" },
            StreamerType = StreamerType.url
        };
    }
}