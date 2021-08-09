using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ChannelsSchema;
using GroupsCacheManager;
using Nest;

namespace ApiLogic.IndexManager.NestData
{
    [ElasticsearchType(IdProperty = nameof(ChannelId))]
    public class ChannelMetadata
    {
        public ChannelMetadata(Channel channel)
        {
            Name = channel.m_sName;
            Description = channel.m_sDescription;
            SystemName = channel.SystemName;
            ChannelType = channel.m_nChannelTypeID;
            ChannelId = channel.m_nChannelID;
            AssetUserRuleId = channel.AssetUserRuleId;
            IsActive = channel.m_nIsActive == 1;
            CreateDate = channel.CreateDate.GetValueOrDefault().ToUniversalTime();
            UpdateDate = channel.UpdateDate.GetValueOrDefault().ToUniversalTime();
        }

        [PropertyName("channel_type")]
        public int ChannelType { get; set; }

        [PropertyName("asset_user_rule_id")]
        public long? AssetUserRuleId { get; set; }

        [PropertyName("update_date")]
        public DateTime UpdateDate { get; set; }

        [PropertyName("create_date")]
        public DateTime CreateDate { get; set; }

        [PropertyName("is_active")]
        public bool IsActive { get; set; }

        [PropertyName("channel_id")]
        public int ChannelId { get; set; }
        
        [PropertyName("system_name")]
        public string SystemName { get; set; }

        [PropertyName("description")]
        public string Description { get; set; }
        
        [PropertyName("name")]
        public string Name { get; set; }
    }
}
