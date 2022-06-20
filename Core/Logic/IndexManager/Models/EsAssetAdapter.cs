using System;
using System.Linq;
using ApiObjects;
using ElasticSearch.Common;
using ElasticSearch.NEST;
using ElasticSearch.Searcher;
using Nest;

namespace ApiLogic.IndexManager.Models
{
    public class EsAssetAdapter
    {
        private static readonly Func<string, DateTime> DateTimeConverter = s => DateTime.TryParse(s, out var value) ? value : default;
        
        private readonly IHit<NestBaseAsset> _nestBaseAsset;
        private readonly ElasticSearchApi.ESAssetDocument _esAssetDocument;
        private readonly Lazy<bool> _isV2Version;

        public EsAssetAdapter(ElasticSearchApi.ESAssetDocument esAssetDocument) : this()
        {
            _esAssetDocument = esAssetDocument;
        }

        public EsAssetAdapter(IHit<NestBaseAsset> nestBaseAsset) : this()
        {
            _nestBaseAsset = nestBaseAsset;
        }

        private EsAssetAdapter()
        {
            // As of now, it's enough to use boolean. Hopefully, V2 will be deprecated, so we won't need this adapter in future.
            _isV2Version = new Lazy<bool>(() => _esAssetDocument != null);
        }

        public string Id => GetValue(_ => _.id, _ => _.Id);

        /// <summary>
        /// This property is used to return AssetId. Please, be aware that it won't work for V7 Version.
        /// </summary>
        [Obsolete]
        public long AssetId => GetValue(_ => _.asset_id, _ =>
        {
            long.TryParse(_.Id, out var result);
            return result;
        });

        public DateTime StartDate => GetValue(_ => _.start_date, _ => _.Source.StartDate);

        public DateTime UpdateDate => GetValue(_ => _.update_date, _ => _.Source.UpdateDate);

        public double Score => GetValue(_ => _.score, _ => _.Score.GetValueOrDefault());

        public int MediaTypeId => GetValue(_ => _.media_type_id, _ => _.Fields.Value<int>("media_type_id"));

        public string Name => GetValue(_ => _.name, _ => _.Source.Name);

        public DateTime CreateDate => GetValue(_ => _.extraReturnFields.TryGetValue("create_date", out var value) ? DateTimeConverter(value) : default, _ => _.Source.CreateDate);

        public string GetMetaValue(EsOrderByMetaField esOrderByField)
        {
            var adapter = new EsOrderByFieldAdapter(esOrderByField);
            var value = _isV2Version.Value
                ? GetMetaValueV2(_esAssetDocument, adapter.EsField)
                : GetMetaValueV7(_nestBaseAsset, adapter.EsV7Field.ToString(), esOrderByField.Language);

            return value ?? string.Empty;
        }

        private T GetValue<T>(Func<ElasticSearchApi.ESAssetDocument, T> v2Value, Func<IHit<NestBaseAsset>, T> v7Value)
        {
            return _isV2Version.Value ? v2Value(_esAssetDocument) : v7Value(_nestBaseAsset);
        }

        private static string GetMetaValueV2(ElasticSearchApi.ESAssetDocument esAssetDocument, string esField)
        {
            esAssetDocument.extraReturnFields.TryGetValue(esField, out var value);
            return value;
        }

        private static string GetMetaValueV7(IHit<NestBaseAsset> nestBaseAsset, string esFieldV7, LanguageObj language)
        {
            var isLangForMetaExists = nestBaseAsset.Source.Metas.TryGetValue(language.Code, out var metas);
            if (!isLangForMetaExists)
            {
                return null;
            }

            var metaStartValue = $"metas.{language.Code}.";
            if (esFieldV7.StartsWith(metaStartValue)
                && metas.TryGetValue(esFieldV7.Substring(metaStartValue.Length), out var metaValue))
            {
                return metaValue.FirstOrDefault();
            }
            
            return null;
        }
    }
}
