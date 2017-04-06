using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public interface INPVRProvider
    {
        bool SynchronizeNpvrWithDomain { get; set; }

        NPVRUserActionResponse CreateAccount(NPVRParamsObj args);

        NPVRUserActionResponse DeleteAccount(NPVRParamsObj args);

        NPVRQuotaResponse GetQuotaData(NPVRParamsObj args);

        NPVRRecordResponse RecordAsset(NPVRParamsObj args);

        NPVRCancelDeleteResponse CancelAsset(NPVRParamsObj args);

        NPVRCancelDeleteResponse DeleteAsset(NPVRParamsObj args);

        NPVRProtectResponse SetAssetProtectionStatus(NPVRParamsObj args);

        NPVRRetrieveAssetsResponse RetrieveAssets(NPVRRetrieveParamsObj args);

        NPVRRecordResponse RecordSeries(NPVRParamsObj args);

        NPVRCancelDeleteResponse CancelSeries(NPVRParamsObj args);

        NPVRCancelDeleteResponse DeleteSeries(NPVRParamsObj args);

        NPVRLicensedLinkResponse GetNPVRLicensedLink(NPVRParamsObj args);

        NPVRRetrieveSeriesResponse RetrieveSeries(NPVRRetrieveParamsObj args);

        NPVRUserActionResponse UpdateAccount(NPVRParamsObj args);
    }
}
