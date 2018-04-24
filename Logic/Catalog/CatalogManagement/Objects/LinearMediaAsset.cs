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
        public long EpgChannelId { get; set; }
        public TstvState? EnableCdvr { get; set; }
        public TstvState? EnableCatchUp { get; set; }
        public TstvState? EnableStartOver { get; set; }
        public TstvState? EnableTrickPlay { get; set; }
        public TstvState? EnableRecordingPlaybackNonEntitledChannel { get; set; }
        public long? CatchUpBuffer { get; set; }
        public long? TrickPlayBuffer { get; set; }
        public string ExternalIngestId { get; set; }
        public string ExternalCdvrId { get; set; }
        public bool CdvrEnabled { get; set; }
        public bool CatchUpEnabled { get; set; }
        public bool StartOverEnabled { get; set; }
        public bool TrickPlayEnabled { get; set; }
        public long BufferCatchUp { get; set; }
        public long BufferTrickPlay { get; set; }
        public bool RecordingPlaybackNonEntitledChannelEnabled { get; set; }

        public LinearMediaAsset()
            :base()
        {
            this.EpgChannelId = 0;
            this.EnableCdvr = null;
            this.EnableCatchUp = null;
            this.EnableStartOver = null;
            this.EnableTrickPlay = null;
            this.EnableRecordingPlaybackNonEntitledChannel = null;
            this.CatchUpBuffer = null;
            this.TrickPlayBuffer = null;
            this.ExternalIngestId = null;
            this.ExternalCdvrId = null;
            this.CdvrEnabled = false;
            this.CatchUpEnabled = false;
            this.StartOverEnabled = false;
            this.TrickPlayEnabled = false;
            this.BufferCatchUp = 0;
            this.BufferTrickPlay = 0;
            this.RecordingPlaybackNonEntitledChannelEnabled = false;
            this.MediaAssetType = MediaAssetType.Linear;
        }

        public LinearMediaAsset(long epgChannelId, TstvState enableCdvr, TstvState enableCatchUp, TstvState enableStartOver, TstvState enableTrickPlay, TstvState enableRecordingPlaybackNonEntitledChannel, long catchUpBuffer,
                                long trickPlayBuffer, string externalIngestId, string externalCdvrId, MediaAsset mediaAsset, TimeShiftedTvPartnerSettings accountTstvSettings)
            : base(mediaAsset.Id, eAssetTypes.MEDIA, mediaAsset.Name, mediaAsset.NamesWithLanguages, mediaAsset.Description, mediaAsset.DescriptionsWithLanguages, mediaAsset.CreateDate, mediaAsset.UpdateDate, mediaAsset.StartDate,
                    mediaAsset.EndDate, mediaAsset.Metas, mediaAsset.Tags, mediaAsset.Images, mediaAsset.CoGuid, mediaAsset.IsActive.Value, mediaAsset.CatalogStartDate, mediaAsset.FinalEndDate, mediaAsset.MediaType, mediaAsset.EntryId,
                    mediaAsset.DeviceRuleId, mediaAsset.GeoBlockRuleId, mediaAsset.Files, mediaAsset.UserTypes)
        {
            this.MediaAssetType = MediaAssetType.Linear;
            this.EpgChannelId = epgChannelId;
            this.EnableCdvr = enableCdvr;
            this.EnableCatchUp = enableCatchUp;
            this.EnableStartOver = enableStartOver;
            this.EnableTrickPlay = enableTrickPlay;
            this.EnableRecordingPlaybackNonEntitledChannel = enableRecordingPlaybackNonEntitledChannel;
            this.CatchUpBuffer = catchUpBuffer;
            this.TrickPlayBuffer = trickPlayBuffer;
            this.ExternalIngestId = externalIngestId;
            this.ExternalCdvrId = externalCdvrId;
            FillEnabledAndBufferProperties(accountTstvSettings);
        }

        private void FillEnabledAndBufferProperties(TimeShiftedTvPartnerSettings accountTstvSettings)
        {
            if (accountTstvSettings != null)
            {

                this.CdvrEnabled = accountTstvSettings.IsCdvrEnabled.HasValue ? accountTstvSettings.IsCdvrEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.CdvrEnabled && this.EnableCdvr.HasValue && this.EnableCdvr.Value == TstvState.Disabled)
                {
                    this.CdvrEnabled = false;
                }

                this.CatchUpEnabled = accountTstvSettings.IsCatchUpEnabled.HasValue ? accountTstvSettings.IsCatchUpEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.CatchUpEnabled && this.EnableCatchUp.HasValue && this.EnableCatchUp.Value == TstvState.Disabled)
                {
                    this.CatchUpEnabled = false;
                }

                this.StartOverEnabled = accountTstvSettings.IsStartOverEnabled.HasValue ? accountTstvSettings.IsStartOverEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.StartOverEnabled && this.EnableStartOver.HasValue && this.EnableStartOver.Value == TstvState.Disabled)
                {
                    this.StartOverEnabled = false;
                }

                this.TrickPlayEnabled = accountTstvSettings.IsTrickPlayEnabled.HasValue ? accountTstvSettings.IsTrickPlayEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.TrickPlayEnabled && this.EnableTrickPlay.HasValue && this.EnableTrickPlay.Value == TstvState.Disabled)
                {
                    this.TrickPlayEnabled = false;
                }

                this.RecordingPlaybackNonEntitledChannelEnabled = accountTstvSettings.IsRecordingPlaybackNonEntitledChannelEnabled.HasValue ? accountTstvSettings.IsRecordingPlaybackNonEntitledChannelEnabled.Value : false;
                // if account is true and channel is false we change the value to false
                if (this.RecordingPlaybackNonEntitledChannelEnabled && this.EnableRecordingPlaybackNonEntitledChannel.HasValue && this.EnableRecordingPlaybackNonEntitledChannel.Value == TstvState.Disabled)
                {
                    this.RecordingPlaybackNonEntitledChannelEnabled = false;
                }

                // if the channel buffer setting == 0 then we get the value from the account
                this.BufferCatchUp = this.CatchUpBuffer.HasValue ? this.CatchUpBuffer.Value : 0;
                if (this.BufferCatchUp == 0)
                {
                    this.BufferCatchUp = accountTstvSettings.CatchUpBufferLength.HasValue ? accountTstvSettings.CatchUpBufferLength.Value : 0;
                }

                // if the channel buffer setting == 0 then we get the value from the account
                this.BufferTrickPlay = this.TrickPlayBuffer.HasValue ? this.TrickPlayBuffer.Value : 0;
                if (this.BufferTrickPlay == 0)
                {
                    this.BufferTrickPlay = accountTstvSettings.TrickPlayBufferLength.HasValue ? accountTstvSettings.TrickPlayBufferLength.Value : 0;
                }                 
            }
        }
    }
}
