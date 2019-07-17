using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [KalturaIntEnum]
    public enum KalturaResponseType
    {
        JSON = 1,
        XML = 2,
        JSONP = 9,
        ASSET_XML =30,
        EXCEL = 31
    }
}