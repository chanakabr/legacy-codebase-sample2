using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using Core.Catalog;
using TVinciShared;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IFilterFileRule
    {
        bool MatchRules(FilterFileRule.Target file, IEnumerable<AssetRuleAction> ruleActions);
    }

    public class FilterFileRule : IFilterFileRule
    {
        private static readonly Lazy<FilterFileRule> Lazy =
            new Lazy<FilterFileRule>(() => new FilterFileRule(), LazyThreadSafetyMode.PublicationOnly);
        public static IFilterFileRule Instance => Lazy.Value;

        public bool MatchRules(Target file, IEnumerable<AssetRuleAction> ruleActions)
        {
            return ruleActions.All(a => MatchRule(file, a));
        }

        private static bool MatchRule(Target file, AssetRuleAction ruleAction)
        {
            var fileType = file.FileType; // could be null
            switch (ruleAction)
            {
                case FilterFileByAudioCodec ac:
                    return fileType?.AudioCodecs != null && ac.AudioCodecs.Intersect(fileType.AudioCodecs, StringComparer.InvariantCultureIgnoreCase).Any();
                case FilterFileByFileTypeForAssetType fta: // should be before FilterFileByFileType
                    return !fta.AssetTypes.Contains(file.AssetType) || (fileType != null && fta.FileTypeIds.Contains(fileType.Id));
                case FilterFileByFileType ft:
                    return fileType != null && ft.FileTypeIds.Contains(fileType.Id);
                case FilterFileByLabel l:
                    return l.Labels.Intersect(file.Labels, StringComparer.InvariantCultureIgnoreCase).Any();
                case FilterFileByQuality q:
                    return fileType != null && q.Qualities.Contains(fileType.Quality);
                case FilterFileByStreamerType st:
                    return fileType?.StreamerType != null && st.StreamerTypes.Contains(fileType.StreamerType.Value);
                case FilterFileByVideoCodec vc:
                    return fileType?.VideoCodecs != null && vc.VideoCodecs.Intersect(fileType.VideoCodecs, StringComparer.InvariantCultureIgnoreCase).Any();
                case FilterFileByDynamicData dd:
                    return file.DynamicData.TryGetValue(dd.Key, out var values) && values.Intersect(dd.Values, StringComparer.InvariantCultureIgnoreCase).Any();
                default: throw new NotImplementedException("unknown filter file action");
            }
        }

        public class Target
        {
            private static readonly IReadOnlyCollection<string> EmptyLabels = new List<string>(0);
            private static readonly IDictionary<string, IEnumerable<string>> EmptyDynamicData = new Dictionary<string, IEnumerable<string>>(0);

            public MediaFileType FileType { get; } // could be null
            public eAssetTypes AssetType { get; }
            public IReadOnlyCollection<string> Labels { get; }
            public IDictionary<string, IEnumerable<string>> DynamicData { get; }

            public Target(MediaFileType fileType, eAssetTypes assetType, string commaSeparatedLabels, IDictionary<string, IEnumerable<string>> dynamicData)
            {
                FileType = fileType;
                AssetType = assetType;
                Labels = commaSeparatedLabels.IsNullOrEmpty() ? EmptyLabels : commaSeparatedLabels.Split(',');
                DynamicData = dynamicData == null
                    ? EmptyDynamicData
                    : new Dictionary<string, IEnumerable<string>>(dynamicData, StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }
}