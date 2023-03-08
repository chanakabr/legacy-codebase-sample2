using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.Base
{
    public class ContextData
    {
        public int GroupId { get; }
        public long? DomainId { get; set; }
        public long? RegionId { get; set; }
        public long? UserId { get; set; }
        public long? OriginalUserId { get; set; }
        public string Udid { get; set; }
        public string UserIp { get; set; }
        public string Language { get; set; }
        public string Format { get; set; }
        public bool ManagementData => !string.IsNullOrEmpty(Format) && Format == "30";
        public string SessionCharacteristicKey { get; set; }
        public IEnumerable<long> UserRoleIds { get; set; }

        public ContextData(int groupId)
        {
            GroupId = groupId;
        }

        public long GetCallerUserId()
        {
            if (OriginalUserId.HasValue && OriginalUserId.Value > 0)
            {
                return OriginalUserId.Value;
            }

            if (UserId.HasValue && UserId.Value > 0)
            {
                return UserId.Value;
            }

            return 0;
        }

        public bool IsUserRoleExist(long id)
        {
            return UserRoleIds != null && UserRoleIds.Contains(id);
        }

        public override string ToString()
        {
            return
                $"GroupId:{GroupId}, DomainId:{DomainId}, RegionId:{RegionId}, UserId:{UserId}, Udid:{Udid}, UserIp:{UserIp}, Language:{Language}, Format:{Format}, SessionCharacteristicKey:{SessionCharacteristicKey}.";
        }
    }
}
