namespace ApiObjects.Cloudfront
{
    public static class InvalidationPath
    {
        public static string EpgDay(long partnerId, long dayTimestamp) =>
            $"/api_v3/service/epg/action/get/partnerid/{partnerId}/date/{dayTimestamp}/*";

        public static string EpgPartner(long partnerId) =>
            $"/api_v3/service/epg/action/get/partnerid/{partnerId}/*";
        
        public static string Lineup(long partnerId) =>
            $"/api_v3/service/lineup/action/get/partnerid/{partnerId}/*";
    }
}