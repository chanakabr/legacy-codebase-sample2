using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsMedia;
using Tvinci.Data.DataLoader;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMChannelInfoLoader : TVMAdapter<dsItemInfo>
    {
        #region properties

        private string m_tvmUser;
        private string m_tvmPass;

        public long ChannelID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "ChannelID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "ChannelID", value);
            }
        }
        #endregion properties

        public TVMChannelInfoLoader(long ChannelId) : this(string.Empty, string.Empty, ChannelId)
        {
        }

        public TVMChannelInfoLoader(string tvmUN, string tvmPass, long channelID)
        {
            this.m_tvmUser = tvmUN;
            this.m_tvmPass = tvmPass;
            this.ChannelID = channelID;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            ChannelsMedia result = new ChannelsMedia();

            channel newChannel = new channel();
            newChannel.id = int.Parse(ChannelID.ToString());
            result.root.request.channelCollection.Add(newChannel);
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();

            return result;
        }

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            ChannelsMedia data = retrievedData as ChannelsMedia;
            if (data == null)
            {
                throw new Exception("");
            }
            dsItemInfo result = new dsItemInfo();

            if (data.response.channelCollection.Count != 0)
            {
                responsechannel channel = data.response.channelCollection[0];

                dsItemInfo.ChannelRow channelRow = result.Channel.NewChannelRow();
                channelRow.ChannelId = channel.id;
                channelRow.Description = channel.description;
                channelRow.Title = channel.title;

                result.Channel.AddChannelRow(channelRow);
            }
            return result;
        }

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{E9FE8346-2777-4393-98FC-07EED12EA60C}"); }
		}
    }
}
