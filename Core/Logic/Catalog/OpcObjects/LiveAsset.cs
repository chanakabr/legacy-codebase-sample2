using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.TimeShiftedTv;
using System.Collections.Generic;
using TVinciShared;

namespace Core.Catalog
{
    public class LiveAsset : MediaAsset
    {
        public const string EXTERNAL_EPG_INGEST_ID = "External Epg Ingest ID";

        public TstvState? EnableCdvrState { get; set; }
        public TstvState? EnableCatchUpState { get; set; }
        public TstvState? EnableStartOverState { get; set; }
        public TstvState? EnableTrickPlayState { get; set; }
        public TstvState? EnableRecordingPlaybackNonEntitledChannelState { get; set; }
        public long? BufferCatchUp { get; set; }
        public long? BufferTrickPlay { get; set; }

        [ExcelColumn(ExcelColumnType.Basic, EXTERNAL_EPG_INGEST_ID, IsMandatory = true)]
        public string ExternalEpgIngestId { get; set; }

        public string ExternalCdvrId { get; set; }
        public bool CdvrEnabled { get; set; }
        public bool CatchUpEnabled { get; set; }
        public bool StartOverEnabled { get; set; }
        public bool TrickPlayEnabled { get; set; }
        public long SummedCatchUpBuffer { get; set; }
        public long SummedTrickPlayBuffer { get; set; }
        public bool RecordingPlaybackNonEntitledChannelEnabled { get; set; }
        public LinearChannelType? ChannelType { get; set; }
        //epg_identifier
        public long EpgChannelId { get; set; }
        public long? PaddingBeforeProgramStarts { get; set; }
        public long? PaddingAfterProgramEnds { get; set; }

        public LiveAsset()
            : base()
        {
            this.EnableCdvrState = null;
            this.EnableCatchUpState = null;
            this.EnableStartOverState = null;
            this.EnableTrickPlayState = null;
            this.EnableRecordingPlaybackNonEntitledChannelState = null;
            this.BufferCatchUp = null;
            this.PaddingBeforeProgramStarts = null;
            this.PaddingAfterProgramEnds = null;
            this.BufferTrickPlay = null;
            this.ExternalEpgIngestId = null;
            this.ExternalCdvrId = null;
            this.CdvrEnabled = false;
            this.CatchUpEnabled = false;
            this.StartOverEnabled = false;
            this.TrickPlayEnabled = false;
            this.SummedCatchUpBuffer = 0;
            this.SummedTrickPlayBuffer = 0;
            this.RecordingPlaybackNonEntitledChannelEnabled = false;
            this.MediaAssetType = MediaAssetType.Linear;
            this.ChannelType = null;
        }

        public LiveAsset(MediaAsset mediaAsset)
            : base(mediaAsset)
        {
            this.EnableCdvrState = null;
            this.EnableCatchUpState = null;
            this.EnableStartOverState = null;
            this.EnableTrickPlayState = null;
            this.EnableRecordingPlaybackNonEntitledChannelState = null;
            this.PaddingBeforeProgramStarts = null;
            this.PaddingAfterProgramEnds = null;
            this.BufferCatchUp = null;
            this.BufferTrickPlay = null;
            this.ExternalEpgIngestId = null;
            this.ExternalCdvrId = null;
            this.CdvrEnabled = false;
            this.CatchUpEnabled = false;
            this.StartOverEnabled = false;
            this.TrickPlayEnabled = false;
            this.SummedCatchUpBuffer = 0;
            this.SummedTrickPlayBuffer = 0;
            this.RecordingPlaybackNonEntitledChannelEnabled = false;
            this.MediaAssetType = MediaAssetType.Linear;
            this.ChannelType = null;
        }

        public LiveAsset(LiveAsset liveAsset)
            : base(liveAsset)
        {
            MediaAssetType = MediaAssetType.Linear;
            EnableCdvrState = liveAsset.EnableCdvrState;
            EnableCatchUpState = liveAsset.EnableCatchUpState;
            EnableStartOverState = liveAsset.EnableStartOverState;
            EnableTrickPlayState = liveAsset.EnableTrickPlayState;
            EnableRecordingPlaybackNonEntitledChannelState = liveAsset.EnableRecordingPlaybackNonEntitledChannelState;
            BufferCatchUp = liveAsset.BufferCatchUp;
            PaddingBeforeProgramStarts = liveAsset.PaddingBeforeProgramStarts;
            PaddingAfterProgramEnds = liveAsset.PaddingBeforeProgramStarts;
            BufferTrickPlay = liveAsset.BufferTrickPlay;
            ExternalEpgIngestId = liveAsset.ExternalEpgIngestId;
            ExternalCdvrId = liveAsset.ExternalCdvrId;
            CdvrEnabled = liveAsset.CdvrEnabled;
            CatchUpEnabled = liveAsset.CatchUpEnabled;
            StartOverEnabled = liveAsset.StartOverEnabled;
            TrickPlayEnabled = liveAsset.TrickPlayEnabled;
            SummedCatchUpBuffer = liveAsset.SummedCatchUpBuffer;
            SummedTrickPlayBuffer = liveAsset.SummedTrickPlayBuffer;
            RecordingPlaybackNonEntitledChannelEnabled = liveAsset.RecordingPlaybackNonEntitledChannelEnabled;
            ChannelType = liveAsset.ChannelType;
            EpgChannelId = liveAsset.EpgChannelId;
        }

        public LiveAsset(
            long epgChannelId,
            TstvState? enableCdvr,
            TstvState? enableCatchUp,
            TstvState? enableStartOver,
            TstvState? enableTrickPlay,
            TstvState? enableRecordingPlaybackNonEntitledChannel,
            long catchUpBuffer,
            long paddingBeforeProgramStarts,
            long paddingAfterProgramEnds,
            long trickPlayBuffer,
            string externalIngestId,
            string externalCdvrId,
            MediaAsset mediaAsset,
            TimeShiftedTvPartnerSettings accountTstvSettings,
            LinearChannelType channelType)
            : base(mediaAsset)
        {
            this.MediaAssetType = MediaAssetType.Linear;
            this.EnableCdvrState = enableCdvr;
            this.EnableCatchUpState = enableCatchUp;
            this.EnableStartOverState = enableStartOver;
            this.EnableTrickPlayState = enableTrickPlay;
            this.EnableRecordingPlaybackNonEntitledChannelState = enableRecordingPlaybackNonEntitledChannel;
            this.BufferCatchUp = catchUpBuffer;
            this.PaddingBeforeProgramStarts = paddingBeforeProgramStarts;
            this.PaddingAfterProgramEnds = paddingAfterProgramEnds;
            this.BufferTrickPlay = trickPlayBuffer;
            this.ExternalEpgIngestId = externalIngestId;
            this.ExternalCdvrId = externalCdvrId;
            this.ChannelType = channelType;
            this.EpgChannelId = epgChannelId;
            FillEnabledAndBufferProperties(accountTstvSettings);
        }

        internal void FillEnabledAndBufferProperties(TimeShiftedTvPartnerSettings accountTstvSettings)
        {
            if (accountTstvSettings != null)
            {
                this.CdvrEnabled = accountTstvSettings.IsCdvrEnabled.HasValue ? accountTstvSettings.IsCdvrEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.CdvrEnabled && this.EnableCdvrState.HasValue && this.EnableCdvrState.Value == TstvState.Disabled)
                {
                    this.CdvrEnabled = false;
                }

                this.CatchUpEnabled = accountTstvSettings.IsCatchUpEnabled.HasValue ? accountTstvSettings.IsCatchUpEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.CatchUpEnabled && this.EnableCatchUpState.HasValue && this.EnableCatchUpState.Value == TstvState.Disabled)
                {
                    this.CatchUpEnabled = false;
                }

                this.StartOverEnabled = accountTstvSettings.IsStartOverEnabled.HasValue ? accountTstvSettings.IsStartOverEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.StartOverEnabled && this.EnableStartOverState.HasValue && this.EnableStartOverState.Value == TstvState.Disabled)
                {
                    this.StartOverEnabled = false;
                }

                this.TrickPlayEnabled = accountTstvSettings.IsTrickPlayEnabled.HasValue ? accountTstvSettings.IsTrickPlayEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.TrickPlayEnabled && this.EnableTrickPlayState.HasValue && this.EnableTrickPlayState.Value == TstvState.Disabled)
                {
                    this.TrickPlayEnabled = false;
                }

                this.RecordingPlaybackNonEntitledChannelEnabled = accountTstvSettings.IsRecordingPlaybackNonEntitledChannelEnabled.HasValue ? accountTstvSettings.IsRecordingPlaybackNonEntitledChannelEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.RecordingPlaybackNonEntitledChannelEnabled && this.EnableRecordingPlaybackNonEntitledChannelState.HasValue && this.EnableRecordingPlaybackNonEntitledChannelState.Value == TstvState.Disabled)
                {
                    this.RecordingPlaybackNonEntitledChannelEnabled = false;
                }

                // if the channel buffer setting == 0 then we get the value from the account
                this.SummedCatchUpBuffer = this.BufferCatchUp.HasValue ? this.BufferCatchUp.Value : 0;
                if (this.SummedCatchUpBuffer == 0)
                {
                    this.SummedCatchUpBuffer = accountTstvSettings.CatchUpBufferLength.HasValue ? accountTstvSettings.CatchUpBufferLength.Value : 0;
                }

                // if the channel buffer setting == 0 then we get the value from the account
                this.SummedTrickPlayBuffer = this.BufferTrickPlay.HasValue ? this.BufferTrickPlay.Value : 0;
                if (this.SummedTrickPlayBuffer == 0)
                {
                    this.SummedTrickPlayBuffer = accountTstvSettings.TrickPlayBufferLength.HasValue ? accountTstvSettings.TrickPlayBufferLength.Value : 0;
                }

                this.PaddingBeforeProgramStarts = PaddingBeforeProgramStarts > 0
                    ? PaddingBeforeProgramStarts
                    : accountTstvSettings.PaddingBeforeProgramStarts ?? default;
                this.PaddingAfterProgramEnds = PaddingAfterProgramEnds > 0
                    ? PaddingAfterProgramEnds
                    : accountTstvSettings.PaddingAfterProgramEnds ?? default;
            }
        }

        public override void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns, IExcelStructureManager structureManager)
        {
            base.SetExcelValues(groupId, columnNamesToValues, columns, structureManager);

            // EXTERNAL_EPG_INGEST_ID
            var columnName = ExcelColumn.GetFullColumnName(EXTERNAL_EPG_INGEST_ID);
            if (columnNamesToValues.ContainsKey(columnName))
            {
                this.ExternalEpgIngestId = columnNamesToValues[columnName].ToString();
            }
        }
        public override Dictionary<string, object> GetExcelValues(int groupId)
        {
            Dictionary<string, object> excelValues = base.GetExcelValues(groupId);

            if (!string.IsNullOrEmpty(this.ExternalEpgIngestId))
            {
                var excelColumn = ExcelColumn.GetFullColumnName(EXTERNAL_EPG_INGEST_ID);
                excelValues.TryAdd(excelColumn, this.ExternalEpgIngestId);
            }

            return excelValues;
        }
    }
}
