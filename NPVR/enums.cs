using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{


    // this enum must correspond to the table lu_npvr_providers in TVinci DB.
    public enum NPVRProvider
    {
        None = 0,
        AlcatelLucent = 1,
        Kaltura = 2,
        Harmonic = 3
    }

    public enum ProtectStatus
    {
        Protected = 0,
        NotProtected = 1,
        RecordingDoesNotExist = 2,
        Error = 3
    }

    public enum RecordStatus
    {
        OK = 0,
        ResourceAlreadyExists = 1,
        QuotaExceeded = 2,
        Error = 3,
        AssetDoesNotExist = 4,
        BadRequest = 5,
        UnauthorizedOperation = 6,
        CommunicationsError = 7,
        InvalidStatus = 8,
        InternalServerError = 9,
        NotImplemented = 10
    }

    public enum CancelDeleteStatus
    {
        OK = 0,
        AlreadyCanceled = 1,
        Error = 2,
        AssetDoesNotExist = 3,
        AssetAlreadyRecorded = 4,
        UnauthorizedOperation = 5,
        CommunicationsError = 6,
        InternalServerError = 7,
        NotImplemented = 8
    }

    public enum SearchByField : ulong
    {
        byAssetId = 1, // recording id
        byStartTime = 2,
        byStatus = 4,
        byChannelId = 8,
        byProgramId = 16, // epg program id
        bySeasonId = 32, // series id
        bySeriesId = 64, // series id
    }

    public enum NPVROrderDir
    {
        DESC = 0,
        ASC = 1
    }

    public enum NPVROrderBy
    {
        startTime = 0,
        name = 1,
        channelId = 2
    }

    public enum NPVRRecordingStatus
    {
        Completed = 0,
        Ongoing = 1,
        Scheduled = 2,
        Cancelled = 3
    }

}
