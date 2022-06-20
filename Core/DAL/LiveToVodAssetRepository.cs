using System;
using System.Threading;
using DAL.DTO;
using ODBCWrapper;

namespace DAL
{
    public class LiveToVodAssetRepository : ILiveToVodAssetRepository
    {
        private static readonly Lazy<ILiveToVodAssetRepository> Lazy = new Lazy<ILiveToVodAssetRepository>(
            () => new LiveToVodAssetRepository(),
            LazyThreadSafetyMode.PublicationOnly
        );

        public static ILiveToVodAssetRepository Instance => Lazy.Value;

        public bool TryGetMediaIdByEpgId(long epgId, out long mediaId)
        {
            var sp = new StoredProcedure("GetLiveToVodMediaIdByEpgId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("EpgId", epgId);

            return sp.ExecuteReturnValue(out mediaId);
        }

        public long? InsertLiveToVodAsset(LiveToVodAssetDTO liveToVodAssetDto)
        {
            var sp = new StoredProcedure("InsertLiveToVodAsset");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("LinearAssetId", liveToVodAssetDto.LinearAssetId);
            sp.AddParameter("EpgChannelId", liveToVodAssetDto.EpgChannelId);
            sp.AddParameter("EpgIdentifier", liveToVodAssetDto.EpgIdentifier);
            sp.AddParameter("EpgId", liveToVodAssetDto.EpgId);
            sp.AddParameter("Crid", liveToVodAssetDto.Crid);
            sp.AddParameter("OriginalStartDate", liveToVodAssetDto.OriginalStartDate);
            sp.AddParameter("OriginalEndDate", liveToVodAssetDto.OriginalEndDate);
            sp.AddParameter("PaddingBeforeProgramStarts", liveToVodAssetDto.PaddingBeforeProgramStarts);
            sp.AddParameter("PaddingAfterProgramEnds", liveToVodAssetDto.PaddingAfterProgramEnds);
            sp.AddParameter("MediaId", liveToVodAssetDto.MediaId);
            sp.AddParameter("UpdaterId", liveToVodAssetDto.UpdaterId);
            var result = sp.ExecuteReturnValue<long>(out var liveToVodAssetId);

            return result ? liveToVodAssetId : (long?)null;
        }

        public bool UpdateLiveToVodAsset(LiveToVodAssetDTO liveToVodAssetDto)
        {
            var sp = new StoredProcedure("UpdateLiveToVodAsset");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("Crid", liveToVodAssetDto.Crid);
            sp.AddParameter("OriginalStartDate", liveToVodAssetDto.OriginalStartDate);
            sp.AddParameter("OriginalEndDate", liveToVodAssetDto.OriginalEndDate);
            sp.AddParameter("PaddingBeforeProgramStarts", liveToVodAssetDto.PaddingBeforeProgramStarts);
            sp.AddParameter("PaddingAfterProgramEnds", liveToVodAssetDto.PaddingAfterProgramEnds);
            sp.AddParameter("MediaId", liveToVodAssetDto.MediaId);
            sp.AddParameter("UpdaterId", liveToVodAssetDto.UpdaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }
    }
}