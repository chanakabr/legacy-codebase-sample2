using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsList;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Context;
using System.Data;

namespace TVPApi
{
    public class APIChannelsListLoader : TVPPro.SiteManager.DataLoaders.ChannelsListLoader
    {

        public APIChannelsListLoader(string tvmUN, string tvmPass, string picSize)
            : base(tvmUN, tvmPass, picSize)
        {
            // Do nothing.
        }

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            count = 0;

            if (retrievedData == null)
                return false;

            ChannelsList result = retrievedData as ChannelsList;

            if (result.response.category.channelCollection.Count == 0)
            {
                count = 0;
                return true;
            }

            count = result.response.category.channelCollection.Count;

            return true;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{F1163BC6-5B81-4457-BAA0-919F9AD56CF1}"); }
        }
    }
}
