using System;
using System.Diagnostics.CodeAnalysis;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.IngestStatus;

namespace WebAPI.Controllers
{
    // this is a workaround to generate documentation and client libs
    // should be removed, when we'll find solution to update KalturaClient.xml with non-Phoenix endpoints

    [Service("ingestStatus")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class IngestStatusController : IKalturaController
    {
        /// <summary>
        /// Returns Core Ingest service partner configurations
        /// </summary>
        /// <returns></returns>
        [Action("getPartnerConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaIngestStatusPartnerConfiguration GetPartnerConfiguration()
        {
            throw new NotImplementedException("call should go to Ingest Status service instead of Phoenix");
        }

        /// <summary>
        /// Returns Core Ingest service partner configurations
        /// </summary>
        /// <param name="config"> the partner config updates </param>
        /// <returns></returns>
        [Action("updatePartnerConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static void UpdatePartnerConfiguration(KalturaIngestStatusPartnerConfiguration config)
        {
            throw new NotImplementedException("call should go to Ingest Status service instead of Phoenix");
        }

        /// <summary>
        /// Response with list of ingest jobs.
        /// </summary>
        /// <param name="pager">Filter pager</param>
        /// <param name="filter">Filter pager</param>
        /// <param name="idsFilter">Filter pager</param>
        /// <returns></returns>
        [Action("getEpgList")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaIngestStatusEpgListResponse GetEpgList(KalturaIngestByIdsFilter idsFilter = null, KalturaIngestByCompoundFilter filter = null, KalturaFilterPager pager = null)
        {
            throw new NotImplementedException("call should go to Ingest status service instead of Phoenix");
        }
    }
}