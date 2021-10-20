namespace ApiLogic.Catalog
{
    public class UserSearchContext
    {
        public long DomainId { get; set; }
        public long UserId { get; set; }
        public int LanguageId { get; set; }
        public string Udid { get; set; }
        public string UserIp { get; set; }
        public bool IgnoreEndDate { get; set; }
        public bool UseStartDate { get; set; }
        public bool UseFinal { get; set; }
        public bool GetOnlyActiveAssets { get; set; }
        public bool IsAllowedToViewInactiveAssets { get; set; }

        public UserSearchContext(long domainId, long userId, int languageId, string udid, string userIp, bool ignoreEndDate, bool useStartDate, bool useFinal, bool getOnlyActiveAssets, bool isAllowedToViewInactiveAssets)
        {
            DomainId = domainId;
            UserId = userId;
            LanguageId = languageId;
            Udid = udid;
            UserIp = userIp;
            IgnoreEndDate = ignoreEndDate;
            UseStartDate = useStartDate;
            UseFinal = useFinal;
            GetOnlyActiveAssets = getOnlyActiveAssets;
            IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets;
        }

        public override string ToString()
        {
            return
                $"{{{nameof(DomainId)}:{DomainId}, {nameof(UserId)}:{UserId}, {nameof(LanguageId)}:{LanguageId}, {nameof(Udid)}:{Udid}, {nameof(UserIp)}:{UserIp}, {nameof(IgnoreEndDate)}:{IgnoreEndDate}, {nameof(UseStartDate)}:{UseStartDate}, {nameof(UseFinal)}:{UseFinal}, {nameof(GetOnlyActiveAssets)}:{GetOnlyActiveAssets}, {nameof(IsAllowedToViewInactiveAssets)}:{IsAllowedToViewInactiveAssets}}}";
        }
    }
}