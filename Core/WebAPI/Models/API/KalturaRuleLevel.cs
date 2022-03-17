namespace WebAPI.Models.API
{
    /// <summary>
    /// Distinction if rule was defined at account, household or user level
    /// </summary>
    public enum KalturaRuleLevel
    {
        invalid = 0,
        user = 1,
        household = 2,
        account = 3
    }
}