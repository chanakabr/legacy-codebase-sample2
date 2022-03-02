namespace ApiLogic.Catalog
{
    public class UserSearchContext
    {
        public long DomainId { get; }
        public long UserId { get; }
        public int LanguageId { get; }
        public string Udid { get; }
        public string UserIp { get; }
        public bool IgnoreEndDate { get; }
        public bool UseStartDate { get; }
        public bool UseFinal { get; }
        public bool GetOnlyActiveAssets { get; }
        public bool IsAllowedToViewInactiveAssets { get; }
        public string SessionCharacteristicKey { get; }

        public UserSearchContext(long domainId, long userId, int languageId, string udid, string userIp, bool ignoreEndDate, bool useStartDate, bool useFinal, bool getOnlyActiveAssets, bool isAllowedToViewInactiveAssets, string sessionCharacteristicKey)
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
            SessionCharacteristicKey = sessionCharacteristicKey;
        }

        public override string ToString()
        {
            return
                $"{{{nameof(DomainId)}:{DomainId}, {nameof(UserId)}:{UserId}, {nameof(LanguageId)}:{LanguageId}, {nameof(Udid)}:{Udid}, {nameof(UserIp)}:{UserIp}, {nameof(IgnoreEndDate)}:{IgnoreEndDate}, {nameof(UseStartDate)}:{UseStartDate}, {nameof(UseFinal)}:{UseFinal}, {nameof(GetOnlyActiveAssets)}:{GetOnlyActiveAssets}, {nameof(IsAllowedToViewInactiveAssets)}:{IsAllowedToViewInactiveAssets}, {nameof(SessionCharacteristicKey)}:{SessionCharacteristicKey}}}";
        }
    }
}