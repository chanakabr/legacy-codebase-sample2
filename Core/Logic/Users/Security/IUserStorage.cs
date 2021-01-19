using Core.Users;
using System;

namespace ApiLogic.Users.Security
{
    /// <summary>
    /// Purpose of the interface & class to cover UsersDal methods, which use username as an input parameter
    /// </summary>
    public interface IUserStorage
    {
        int GetUserIDByUsername(string username, int groupId);
        string GetActivationToken(int groupId, string username);
        bool GenerateToken(int groupId, string username, int tokenValidityHours, out int userId, out string email, out string firstName, out string token);
        UserActivationState GetUserActivationState(int nParentGroupID, int nActivationMustHours, ref string sUserName, ref int nUserID, ref bool isGracePeriod);
        int GetUserPasswordFailHistory(string username, int groupId, ref DateTime dNow, ref int failCount, ref DateTime lastFailDate, ref DateTime lastHitDate, ref DateTime passwordUpdateDate);
    }
}
