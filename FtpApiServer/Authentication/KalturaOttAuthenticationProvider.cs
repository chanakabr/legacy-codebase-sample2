using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using Kaltura.Services;
using KLogMonitor;
using System;
using System.Reflection;

namespace FtpApiServer.Authentication
{
    public class KalturaOttAuthenticationProvider : IMembershipProvider
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly Kaltura.Client _KalturaClient;

        public KalturaOttAuthenticationProvider(Kaltura.Client kalturaClient)
        {
            _KalturaClient = kalturaClient;
        }

        public MemberValidationResult ValidateUser(string usernameWithGroup, string password)
        {
            var usernameParams = usernameWithGroup.Split('/');
            if (usernameParams.Length < 2)
            {
                _Logger.Error("FTP Api Server > ValidateUser > Expected username format is ottUsername/groupId");
                return new MemberValidationResult(MemberValidationStatus.InvalidLogin);
            }

            var username = usernameParams[0];
            var groupIdStr = usernameParams[1];
            if (!int.TryParse(groupIdStr, out var groupId))
            {
                _Logger.Error("FTP Api Server > ValidateUser > Group id should be int value");
                return new MemberValidationResult(MemberValidationStatus.InvalidLogin);
            }

            var authenticatedUser = AuthenticateOttUser(username, password, groupId);
            if (authenticatedUser == null) { return new MemberValidationResult(MemberValidationStatus.InvalidLogin); }

            _Logger.Info($"FTP Api Server > ValidateUser > Successfully Authenticated user:[{username}] and group:[{groupId}]");
            var validationResult = new MemberValidationResult(MemberValidationStatus.AuthenticatedUser, authenticatedUser);
            return validationResult;
        }

        private AuthenticatedKalturaOttUser AuthenticateOttUser(string username, string password, int groupId)
        {
            // TODO: support veon login flow with email\ ottuser .. or expect them to use a permenant password user without pin login.
            try
            {
                var loginResult = OttUserService.Login(groupId, username, password).ExecuteAndWaitForResponse(_KalturaClient);
                if (string.IsNullOrEmpty(loginResult?.LoginSession?.Ks))
                {
                    _Logger.Error($"Failed authenticate [{username}], [{password}], [{groupId}], respons did not inclued Ks");
                    return null;
                }
                return new AuthenticatedKalturaOttUser(username, loginResult.LoginSession.Ks, groupId);
            }
            catch (Exception e)
            {
                _Logger.Error($"Failed authenticate [{username}], [{password}], [{groupId}], see inner exception for details", e);
                return null;
            }
        }
    }
}