using ApiObjects;
using Core.Users;

namespace ApiLogic.Users
{
    public interface IBaseDomainFactory
    {
        IBaseDomain GetBaseImpl(int groupId);
    }

    public class BaseDomainFactory : IBaseDomainFactory
    {
        private const string USERS_CONNECTION = "users_connection_string";

        public IBaseDomain GetBaseImpl(int groupId)
        {
            var implId = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.DOMAINS, groupId, (int)ImplementationsModules.Domains, USERS_CONNECTION);

            switch (implId)
            {
                case 1:
                    return new TvinciDomain(groupId);
                default:
                    return null;
            }
        }
    }
}
