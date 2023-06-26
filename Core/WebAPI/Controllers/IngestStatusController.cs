using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
        public static KalturaIngestStatusEpgListResponse GetEpgList(KalturaIngestByIdsFilter idsFilter = null, KalturaIngestByCompoundFilter filter = null, KalturaFilterPager pager = null)
        {
            throw new NotImplementedException("call should go to Ingest status service instead of Phoenix");
        }

        /// <summary>
        /// Returns information about specific Ingest job
        /// </summary>
        /// <param name="ingestId">The id of the requested ingest job</param>
        /// <returns></returns>
        [Action("getEpgDetails")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaIngestEpgDetails GetEpgDetails(long ingestId)
        {
            throw new NotImplementedException("call should go to Ingest status service instead of Phoenix");
        }

        /// <summary>
        /// Get as input ingest job id, filter and pager and response with page of filtered detailed ingest job results.
        /// </summary>
        /// <param name="ingestId">The id of the requested ingest job</param>
        /// <param name="filter">Filter for Ingest program, results</param>
        /// <param name="pager">Paging the request</param>
        [Action("getEpgProgramResultList")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaIngestStatusEpgProgramResultListResponse GetEpgProgramResultList(long ingestId, KalturaIngestEpgProgramResultFilter filter = null, KalturaFilterPager pager = null)
        {
            throw new NotImplementedException("call should go to Ingest status service instead of Phoenix");
        }

        /// <summary>
        /// List detailed results of ingested assets.
        /// </summary>
        /// <param name="filter">Filter object with parameters to filter selected ingest processes and assets</param>
        /// <param name="pager">Paging the request</param>
        [Action("getVodAssetResult")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaVodIngestAssetResultResponse GetVodIngestAssetResult(
            KalturaVodIngestAssetResultFilter filter = null,
            KalturaFilterPager pager = null)
        {
            throw new NotImplementedException("call should go to Ingest status service instead of Phoenix");
        }
    }
}