using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsMedia;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsList;
using TVPPro.Configuration.Technical;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class ChannelsListLoader : TVMAdapter<dsItemInfo>
    {
        #region properties

        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "GroupID", value);
            }

        }

        public string PicSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PicSize", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "PicSize", value);
            }
        }

        public string TvmUser
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmUser", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmUser", value);
            }

        }

        public string TvmPass
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmPass", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmPass", value);
            }

        }

        #endregion properties

        public ChannelsListLoader(int groupID, string tvmUN, string tvmPass, string picSize)
        {
            this.TvmUser = tvmUN;
            this.TvmPass = tvmPass;
            this.PicSize = picSize;
            this.GroupID = groupID;

        }
        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            ChannelsList result = new ChannelsList();

            result.root.request.category = new category();
            result.root.request.category.id = "0";
            result.root.flashvars.player_un = this.TvmUser;
            result.root.flashvars.player_pass = this.TvmPass;
            result.root.flashvars.pic_size1 = this.PicSize;
            result.root.flashvars.zip = "1";

            result.root.request.MakeSchemaCompliant();

            return result;
        }

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            ChannelsList data = retrievedData as ChannelsList;

            if (data == null)
            {
                throw new Exception("");
            }

            dsItemInfo result = new dsItemInfo();

            if (data.response.category.channelCollection.Count != 0)
            {
                dsItemInfo.ChannelRow channelRow;

                foreach (Tvinci.Data.TVMDataLoader.Protocols.ChannelsList.channel channel in data.response.category.channelCollection)
                {
                    channelRow = result.Channel.NewChannelRow();
                    channelRow.ChannelId = channel.id.ToString();
                    channelRow.Title = channel.title;
                    channelRow.Description = channel.description;
                    result.Channel.AddChannelRow(channelRow);
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{369CAF38-B400-4640-A2A4-8E15AFE71080}"); }
        }
    }
}
