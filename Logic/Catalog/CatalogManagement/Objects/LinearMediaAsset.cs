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
        public string ExternalIngestId { get; set; }
        public string ExternalCdvrId { get; set; }
        public bool EnableCdvr { get; set; }
        public bool EnableCatchUp { get; set; }
        public bool EnableStartOver { get; set; }
        public bool EnableTrickPlay { get; set; }
        public long CatchUpBuffer { get; set; }
        public long TrickPlayBuffer { get; set; }
        public bool EnableRecordingPlaybackNonEntitledChannel { get; set; }

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
            this.ExternalIngestId = null;
            this.ExternalCdvrId = null;
            this.EnableCdvr = false;
            this.EnableCatchUp = false;
            this.EnableStartOver = false;
            this.EnableTrickPlay = false;
            this.CatchUpBuffer = 0;
            this.TrickPlayBuffer = 0;
            this.EnableRecordingPlaybackNonEntitledChannel = false;
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
            this.ExternalIngestId = externalIngestId;
            this.ExternalCdvrId = externalCdvrId;
            FillEnabledAndBufferProperties(accountTstvSettings);
        }

        private void FillEnabledAndBufferProperties(TimeShiftedTvPartnerSettings accountTstvSettings)
        {
            if (accountTstvSettings != null)
            {

                this.EnableCdvr = accountTstvSettings.IsCdvrEnabled.HasValue ? accountTstvSettings.IsCdvrEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.EnableCdvr && this.EnableCdvrState.HasValue && this.EnableCdvrState.Value == TstvState.Disabled)
                {
                    this.EnableCdvr = false;
                }

                this.EnableCatchUp = accountTstvSettings.IsCatchUpEnabled.HasValue ? accountTstvSettings.IsCatchUpEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.EnableCatchUp && this.EnableCatchUpState.HasValue && this.EnableCatchUpState.Value == TstvState.Disabled)
                {
                    this.EnableCatchUp = false;
                }

                this.EnableStartOver = accountTstvSettings.IsStartOverEnabled.HasValue ? accountTstvSettings.IsStartOverEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.EnableStartOver && this.EnableStartOverState.HasValue && this.EnableStartOverState.Value == TstvState.Disabled)
                {
                    this.EnableStartOver = false;
                }

                this.EnableTrickPlay = accountTstvSettings.IsTrickPlayEnabled.HasValue ? accountTstvSettings.IsTrickPlayEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.EnableTrickPlay && this.EnableTrickPlayState.HasValue && this.EnableTrickPlayState.Value == TstvState.Disabled)
                {
                    this.EnableTrickPlay = false;
                }

                this.EnableRecordingPlaybackNonEntitledChannel = accountTstvSettings.IsRecordingPlaybackNonEntitledChannelEnabled.HasValue ? accountTstvSettings.IsRecordingPlaybackNonEntitledChannelEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.EnableRecordingPlaybackNonEntitledChannel && this.EnableRecordingPlaybackNonEntitledChannelState.HasValue && this.EnableRecordingPlaybackNonEntitledChannelState.Value == TstvState.Disabled)
                {
                    this.EnableRecordingPlaybackNonEntitledChannel = false;
                }

                // if the channel buffer setting == 0 then we get the value from the account
                this.CatchUpBuffer = this.BufferCatchUp.HasValue ? this.BufferCatchUp.Value : 0;
                if (this.CatchUpBuffer == 0)
                {
                    this.CatchUpBuffer = accountTstvSettings.CatchUpBufferLength.HasValue ? accountTstvSettings.CatchUpBufferLength.Value : 0;
                }

                // if the channel buffer setting == 0 then we get the value from the account
                this.TrickPlayBuffer = this.BufferTrickPlay.HasValue ? this.BufferTrickPlay.Value : 0;
                if (this.TrickPlayBuffer == 0)
                {
                    this.TrickPlayBuffer = accountTstvSettings.TrickPlayBufferLength.HasValue ? accountTstvSettings.TrickPlayBufferLength.Value : 0;
                }                 
            }
        }
    }
}
