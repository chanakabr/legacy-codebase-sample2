namespace WebAPI.Models.Domains
{
    public enum KalturaHouseholdState
    {
        ok,
        created_without_npvr_account,
        suspended,
        no_users_in_household,
        pending,
    }
}