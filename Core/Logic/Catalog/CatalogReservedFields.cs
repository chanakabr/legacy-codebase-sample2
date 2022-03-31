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
            CatalogLogic.EXTERNAL_OFFER_ID
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
            NamingHelper.ENABLE_CATCHUP
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
            CatalogLogic.LASTMODIFIED
        };
    }
}