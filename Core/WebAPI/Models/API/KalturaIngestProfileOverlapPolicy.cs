namespace WebAPI.Models.API
{
    /// <summary>
    /// indicates how overlaps in EPG should be managed
    /// (a setting per liniar media id will also be avaiable)
    /// 0 - reject input with overlap
    /// 1 - cut source
    /// 2 - cut target
    /// </summary>
    public enum KalturaIngestProfileOverlapPolicy
    {
        REJECT = 0,
        CUT_SOURCE = 1,
        CUT_TARGET = 2
    }
}