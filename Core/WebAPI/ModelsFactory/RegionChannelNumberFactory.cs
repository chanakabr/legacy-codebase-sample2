using ApiObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.API;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Models.General;

namespace WebAPI.ModelsFactory
{
    public static class RegionChannelNumberFactory
    {
        public static KalturaRegionChannelNumber Create(int regionId, int channelNumber)
        {
            return new KalturaRegionChannelNumber { RegionId = regionId, ChannelNumber = channelNumber };
        }
        
        public static KalturaRegionChannelNumberMultiLcns Create(int regionId, int channelNumber, string lcns)
        {
            return new KalturaRegionChannelNumberMultiLcns { RegionId = regionId, ChannelNumber = channelNumber, LCNs = lcns};
        }
    }
}
