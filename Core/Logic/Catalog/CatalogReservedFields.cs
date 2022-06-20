using System.Collections.Generic;
using ApiLogic.IndexManager.Helpers;

namespace Core.Catalog
{
    public class CatalogReservedFields
    {
        public static readonly HashSet<string> ReservedUnifiedSearchStringFields = new HashSet<string>
        {
            CatalogLogic.NAME,
            CatalogLogic.DESCRIPION,
            CatalogLogic.EPG_CHANNEL_ID,
            "crid",
            CatalogLogic.EXTERNALID,
            CatalogLogic.ENTRYID,
            "....",
            CatalogLogic.EXTERNAL_OFFER_ID,
            CatalogLogic.L2V_CRID,
            CatalogLogic.L2V_EPG_ID
        };

        public static readonly HashSet<string> ReservedUnifiedSearchNumericFields = new HashSet<string>
        {
            "like_counter",
            "views",
            "rating",
            "votes",
            CatalogLogic.EPG_CHANNEL_ID,
            CatalogLogic.MEDIA_ID,
            CatalogLogic.EPG_ID,
            CatalogLogic.LINEAR_MEDIA_ID,
            CatalogLogic.STATUS,
            CatalogLogic.RECORDING_ID,
            NamingHelper.ENABLE_CDVR,
            NamingHelper.ENABLE_CATCHUP,
            CatalogLogic.L2V_LINEAR_ASSET_ID,
            CatalogLogic.L2V_EPG_CHANNEL_ID
        };

        public static readonly HashSet<string> ReservedUnifiedDateFields = new HashSet<string>
        {
            CatalogLogic.CREATIONDATE,
            CatalogLogic.PLAYBACKSTARTDATETIME,
            CatalogLogic.START_DATE,
            CatalogLogic.PLAYBACKENDDATETIME,
            CatalogLogic.CATALOGSTARTDATETIME,
            CatalogLogic.CATALOGENDDATETIME,
            CatalogLogic.END_DATE,
            CatalogLogic.LASTMODIFIED,
            CatalogLogic.L2V_ORIGINAL_START_DATE,
            CatalogLogic.L2V_ORIGINAL_END_DATE
        };
    }
}