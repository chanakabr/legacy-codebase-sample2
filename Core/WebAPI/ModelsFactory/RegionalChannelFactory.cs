using WebAPI.Models.API;

namespace WebAPI.ModelsFactory
{
    public static class RegionalChannelFactory
    {
        public static KalturaRegionalChannel Create(long linearChannelId, int channelNumber)
        {
            return new KalturaRegionalChannel { LinearChannelId = (int) linearChannelId, ChannelNumber = channelNumber };
        }
        
        public static KalturaRegionalChannelMultiLcns Create(long linearChannelId, int channelNumber, string lcns)
        {
            return new KalturaRegionalChannelMultiLcns { LinearChannelId = (int) linearChannelId, ChannelNumber = channelNumber , LCNs = lcns};
        }
    }
}
