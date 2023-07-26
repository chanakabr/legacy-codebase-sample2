using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.NEST;
using Nest;

namespace Core.Catalog.Response
{
    [DataContract]
    public class ExtendedSearchResult: UnifiedSearchResult 
    {
        public ExtendedSearchResult() { }

        public ExtendedSearchResult(ElasticSearchApi.ESAssetDocument doc, UnifiedSearchResult unifiedSearchResult)
        {
            AssetId = unifiedSearchResult.AssetId;
            m_dUpdateDate = doc.update_date;
            AssetType = unifiedSearchResult.AssetType;
            EndDate = doc.end_date;
            StartDate = doc.start_date;
            Score = doc.score;
            ExtraFields = doc.extraReturnFields?
                .Select(x => new ApiObjects.KeyValuePair { key = x.Key, value = x.Value })
                .ToList();
        }

        public ExtendedSearchResult(IHit<NestBaseAsset> doc, UnifiedSearchResult unifiedSearchResult, UnifiedSearchDefinitions definitions)
        {
            AssetId = unifiedSearchResult.AssetId;
            m_dUpdateDate = doc.Source.UpdateDate;
            AssetType = unifiedSearchResult.AssetType;
            EndDate = doc.Source.EndDate;
            StartDate = doc.Source.StartDate;
            Score = unifiedSearchResult.Score;
            if (definitions.extraReturnFields?.Count > 0)
            {
                var language = definitions.langauge?.Code ?? string.Empty;
                ExtraFields = definitions.extraReturnFields
                    .Select(x => (x, doc.Fields.Value<object>(UnifiedSearchNestBuilder.GetExtraFieldName(language, x))))
                    .Where(x => x.Item1 != null)
                    .Select(x => new ApiObjects.KeyValuePair()
                    {
                        key = x.Item1,
                        value = Convert.ToString(x.Item2)
                    }).ToList();
            }
        }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public List<ApiObjects.KeyValuePair> ExtraFields { get; set; }
    }

    public class ExtendedRecordingSearchResult : ExtendedSearchResult
    {
        public ExtendedRecordingSearchResult(ElasticSearchApi.ESAssetDocument doc, RecordingSearchResult recordingSearchResult)
            : base(doc, recordingSearchResult)
        {
            EpgId = recordingSearchResult.EpgId;
            RecordingType = recordingSearchResult.RecordingType;
            IsMulti = recordingSearchResult.IsMulti;
        }

        public ExtendedRecordingSearchResult(IHit<NestBaseAsset> doc, RecordingSearchResult recordingSearchResult, UnifiedSearchDefinitions definitions)
            : base(doc, recordingSearchResult, definitions)
        {
            EpgId = recordingSearchResult.EpgId;
            RecordingType = recordingSearchResult.RecordingType;
            IsMulti = recordingSearchResult.IsMulti;
        }

        public string EpgId { get; set; }

        public RecordingType? RecordingType { get; set; }

        public bool IsMulti { get; set; }
    }

    public class ExtendedEpgSearchResult : ExtendedSearchResult
    {
        public ExtendedEpgSearchResult(ElasticSearchApi.ESAssetDocument doc, EpgSearchResult epgSearchResult)
            : base(doc, epgSearchResult)
        {
            DocumentId = epgSearchResult.DocumentId;
        }

        public ExtendedEpgSearchResult(IHit<NestBaseAsset> doc, EpgSearchResult epgSearchResult, UnifiedSearchDefinitions definitions)
            : base(doc, epgSearchResult, definitions)
        {
            DocumentId = epgSearchResult.DocumentId;
        }

        public string DocumentId { get; set; }
    }
}
