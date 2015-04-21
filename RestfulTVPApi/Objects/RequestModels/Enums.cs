using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.RequestModels
{
    public class Enums
    {
        public enum EPGUnit
        {
            Days,
            Hours,
            Current
        }

        public enum StatsType
        {
            Media,
            Epg
        }

        public static RestfulTVPApi.Catalog.StatsType ConvertStatsType(StatsType type)
        {
            RestfulTVPApi.Catalog.StatsType result;

            switch (type)
            {
                case StatsType.Media:
                    result = Catalog.StatsType.MEDIA;
                    break;
                case StatsType.Epg:
                    result = Catalog.StatsType.EPG;
                    break;
                default:
                    throw new Exception("Unknown StatsType");
            }

            return result;
        }
    }
}