using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.TimeShiftedTv;

namespace Core.Catalog.CatalogManagement
{
    public class LinearMediaAsset: MediaAsset
    {
        public TstvState? EnableCdvrState { get; set; }
        public TstvState? EnableCatchUpState { get; set; }
        public TstvState? EnableStartOverState { get; set; }
        public TstvState? EnableTrickPlayState { get; set; }
        public TstvState? EnableRecordingPlaybackNonEntitledChannelState { get; set; }
        public long? BufferCatchUp { get; set; }
        public long? BufferTrickPlay { get; set; }
        public string ExternalEpgIngestId { get; set; }
        public string ExternalCdvrId { get; set; }
        public bool CdvrEnabled { get; set; }
        public bool CatchUpEnabled { get; set; }
        public bool StartOverEnabled { get; set; }
        public bool TrickPlayEnabled { get; set; }
        public long SummedCatchUpBuffer { get; set; }
        public long SummedTrickPlayBuffer { get; set; }
        public bool RecordingPlaybackNonEntitledChannelEnabled { get; set; }

        public LinearMediaAsset()
            :base()
        {
            this.EnableCdvrState = null;
            this.EnableCatchUpState = null;
            this.EnableStartOverState = null;
            this.EnableTrickPlayState = null;
            this.EnableRecordingPlaybackNonEntitledChannelState = null;
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
        }

        public LinearMediaAsset(TstvState enableCdvr, TstvState enableCatchUp, TstvState enableStartOver, TstvState enableTrickPlay, TstvState enableRecordingPlaybackNonEntitledChannel, long catchUpBuffer,
                                long trickPlayBuffer, string externalIngestId, string externalCdvrId, MediaAsset mediaAsset, TimeShiftedTvPartnerSettings accountTstvSettings)
            : base(mediaAsset)
        {
            this.MediaAssetType = MediaAssetType.Linear;
            this.EnableCdvrState = enableCdvr;
            this.EnableCatchUpState = enableCatchUp;
            this.EnableStartOverState = enableStartOver;
            this.EnableTrickPlayState = enableTrickPlay;
            this.EnableRecordingPlaybackNonEntitledChannelState = enableRecordingPlaybackNonEntitledChannel;
            this.BufferCatchUp = catchUpBuffer;
            this.BufferTrickPlay = trickPlayBuffer;
            this.ExternalEpgIngestId = externalIngestId;
            this.ExternalCdvrId = externalCdvrId;
            FillEnabledAndBufferProperties(accountTstvSettings);
        }

        private void FillEnabledAndBufferProperties(TimeShiftedTvPartnerSettings accountTstvSettings)
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
            }
        }
    }
}
