namespace WebAPI.Models.API
{
    /// <summary>
    /// 0 - reject input with holes
    /// 1 - autofill holes
    /// 2 - keep holes and don’t autofill
    /// </summary>
    public enum KalturaIngestProfileAutofillPolicy
    {
        REJECT = 0,
        AUTOFILL = 1,
        KEEP_HOLES = 2
    }
}