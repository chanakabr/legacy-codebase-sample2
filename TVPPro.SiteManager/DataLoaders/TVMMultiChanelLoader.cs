using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.TVMDataLoader.Protocols.RssChannelsList;
namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMMultiChanelLoader : TVMAdapter<dsChannels>
    {
        protected string m_tvmUser;
        protected string m_tvmPass;

        #region Constractor
        public TVMMultiChanelLoader()
        {
            
        }
        public TVMMultiChanelLoader(string i_tvnUser, string i_tvmPass)
        {
            m_tvmUser = i_tvnUser;
            m_tvmPass = i_tvmPass;
        }

        #endregion

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            RssChannelsList protocol = new RssChannelsList();
            protocol.root.flashvars.player_un = m_tvmUser;
            protocol.root.flashvars.player_pass = m_tvmPass;

            protocol.root.request.@params.start_index = "0";
            protocol.root.request.@params.number_of_items = "30";
            protocol.root.flashvars.no_cache = "0";
            return protocol;
        }
      
        protected override dsChannels PreCacheHandling(object retrievedData)
        {
            RssChannelsList protocol = retrievedData as RssChannelsList;

            if (protocol == null)
            {
                throw new Exception("Returned object is not a CommentsList protocol");
            }

            dsChannels resault = new dsChannels();

            foreach (channel pChannel in protocol.response.channels)
            {
                dsChannels.ChannelsRow itemRow = resault.Channels.NewChannelsRow();
                itemRow.ChannelId = pChannel.id;
                itemRow.Title = pChannel.title;
                resault.Channels.AddChannelsRow(itemRow);
                
            }
            return resault;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{D4A898EE-A5B7-41a3-AE2D-3F99A6814A97}"); }
        }
    }

}
