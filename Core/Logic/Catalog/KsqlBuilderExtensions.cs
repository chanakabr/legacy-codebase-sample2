using System;
using System.Collections.Generic;
using Core.Catalog;
using TVinciShared;

namespace ApiLogic.Catalog
{
    public static class KsqlBuilderExtensions
    {
        public static KsqlBuilder AnyMediaIds(this KsqlBuilder builder, IEnumerable<long> mediaIds)
        {
            builder.In(CatalogLogic.MEDIA_ID, mediaIds);

            return builder;
        }

        public static KsqlBuilder AnyEpgIds(this KsqlBuilder builder, IEnumerable<long> epgIds)
        {
            builder.In(CatalogLogic.EPG_ID, epgIds);

            return builder;
        }

        public static KsqlBuilder AnyAssetTypes(this KsqlBuilder builder, IEnumerable<long> assetTypes)
        {
            builder.Or(x => x.Values(x.Equal, CatalogLogic.ASSET_TYPE, assetTypes));

            return builder;
        }

        public static KsqlBuilder MediaType(this KsqlBuilder builder)
        {
            builder.Equal(CatalogLogic.ASSET_TYPE, CatalogLogic.MEDIA_ASSET_TYPE);

            return builder;
        }

        public static KsqlBuilder EpgType(this KsqlBuilder builder)
        {
            builder.Equal(CatalogLogic.ASSET_TYPE, CatalogLogic.EPG_ASSET_TYPE);

            return builder;
        }

        public static KsqlBuilder Values<T>(this KsqlBuilder builder, Func<string, T, KsqlBuilder> fn, string field, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                fn(field, value);
            }

            return builder;
        }
    }
}