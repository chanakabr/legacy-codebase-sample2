using System.Collections.Generic;

namespace ApiObjects.Roles
{
    public class PredefinedRoleId
    {
        public const long ANONYMOUS = 0;
        public const long USER = 1;
        public const long MASTER = 2;
        public const long OPERATOR = 3;
        public const long MANAGER = 4;
        public const long ADMINISTRATOR = 5;
        public const long SYSTEM_ADMINISTRATOR = 9;
        public const long SHOP_MANAGER_CONTENT = 12;
        public const long SHOP_MANAGER_PPV = 13;
        public const long SHOP_MANAGER_COLLECTION = 14;
        public const long SHOP_MANAGER_CAMPAIGN = 15;
        public const long SHOP_SERVER = 16;
        public const long DMS_OPERATOR = 17;

        private static readonly HashSet<long> ShopManagerRoleIds = new HashSet<long>
        {
            SHOP_MANAGER_CONTENT, SHOP_MANAGER_PPV, SHOP_MANAGER_COLLECTION, SHOP_MANAGER_CAMPAIGN, SHOP_SERVER
        };
        
        private static readonly HashSet<long> ManagerAllowedRoleIds = new HashSet<long>
        {
            ANONYMOUS, USER, MASTER, OPERATOR, MANAGER, SHOP_MANAGER_CONTENT, SHOP_MANAGER_PPV, SHOP_MANAGER_COLLECTION, SHOP_MANAGER_CAMPAIGN, SHOP_SERVER
        };

        public static HashSet<long> GetShopManagerRoleIds() => ShopManagerRoleIds;
        public static HashSet<long> GetManagerAllowedRoleIds() => ManagerAllowedRoleIds;
    }
}