using System;
using System.Collections.Generic;
using System.Text;
using ApiObjects;
#if NETFRAMEWORK
using TVinciShared;
#endif

namespace ApiLogic.Api.Managers.Rule
{
    [Obsolete]
    public class KsqlBuilderOld
    {
        private const string _entitledAssetsOnly = "entitled_assets='entitled'";

        public static string And(params string[] ksql)
        {
            return And((IEnumerable<string>) ksql);
        }

        public static string And(IEnumerable<string> ksql)
        {
            return new StringBuilder()
                .Append("(and ")
                .AppendJoin(" ", ksql)
                .Append(")")
                .ToString();
        }
        
        public static string Or(params string[] ksql)
        {
            return Or((IEnumerable<string>) ksql);
        }

        public static string Or(IEnumerable<string> ksql)
        {
            return new StringBuilder()
                .Append("(or ")
                .AppendJoin(" ", ksql)
                .Append(")")
                .ToString();
        }

        public static string AssetType(eAssetTypes assetType)
        {
            switch (assetType)
            {
                case eAssetTypes.UNKNOWN: return string.Empty;
                case eAssetTypes.EPG: return "asset_type = 'epg'";
                case eAssetTypes.NPVR: return "asset_type = 'recording'";
                case eAssetTypes.MEDIA: return "asset_type = 'media'";
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }
        }

        public static string EntitledAssetsOnly => _entitledAssetsOnly;
    }
}
