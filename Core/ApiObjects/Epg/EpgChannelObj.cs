using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    public class EpgChannelObj
    {
        [DBFieldMapping("CHANNEL_ID")]
        public string ChannelExternalId { get; set; }

        [DBFieldMapping("ID")]
        public int ChannelId { get; set; }

        [DBFieldMapping("NAME")]
        public string ChannelName { get; set; }

        [DBFieldMapping("epg_channel_type")]
        public EpgChannelType ChannelType { get; set; }

        public EpgChannelObj() { }

        public EpgChannelObj(int nChannelId, string channelExternalID, string sChannelName, int nChannelType)
        {
            this.ChannelExternalId = channelExternalID;
            this.ChannelId = nChannelId;
            this.ChannelName = sChannelName;
            this.ChannelType = (EpgChannelType)nChannelType;
        }

        //public EpgChannelObj(DataRow row)
        //{
        //    this.ChannelExternalId = ODBCWrapper.Utils.GetSafeStr(row, "CHANNEL_ID").Replace("\r", "").Replace("\n", "");
        //    this.ChannelName = ODBCWrapper.Utils.GetSafeStr(row, "NAME");
        //    this.ChannelId = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
        //    this.ChannelType = (EpgChannelType)ODBCWrapper.Utils.GetIntSafeVal(row, "epg_channel_type");


        //}
    }
}
