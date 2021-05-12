using AutoFixture;
using CachingProvider.LayeredCache;
using DAL;
using DAL.DTO;
using Moq;
using NUnit.Framework;
using WebAPI.ClientManagers;
using WebAPI.Managers.Models;
using WebAPI.Filters;
using System;
using System.Linq;
using FluentAssertions;

namespace WebAPI.Tests.Managers
{
    [TestFixture]
    public class GroupsManagerTests
    {
        [Test]
        public void CheckAddBaseConfiguration()
        {
            // Set Up
            AutoMapperConfig.RegisterMappings();
            var fixture = new Fixture();

            const int groupId = 4;
            var groupToUpdate = fixture.Create<Group>();
            var groupToBeSaved = CreateGroupDTO(groupToUpdate);

            // Mocking
            var baseConfigRepository = new Mock<IGroupBaseConfigurationRepository>();
            baseConfigRepository.Setup(x => x.GetGroupConfigKey(It.IsIn(groupId))).Returns("group_key_{0}");
            baseConfigRepository.Setup(x => x.SaveConfig(It.IsIn(groupId), It.IsAny<GroupDTO>()))
                          .Returns(true);

            // Exec
            var manager = new GroupsManager(Mock.Of<ILayeredCache>(), baseConfigRepository.Object);
            manager.AddBaseConfiguration(groupId, groupToUpdate);

            // Assertion
            baseConfigRepository.Verify(m => m.SaveConfig(It.IsIn(groupId), GroupIsDeepEqual(groupToBeSaved)));
        }

        private static GroupDTO CreateGroupDTO(Group copyFrom) 
        {
            int revokedKsMaxTtlSeconds;

            if (copyFrom.AppTokenSessionMaxDurationSeconds > copyFrom.KSExpirationSeconds)
            {
                revokedKsMaxTtlSeconds = copyFrom.AppTokenSessionMaxDurationSeconds;
            }
            else
            {
                revokedKsMaxTtlSeconds = (int)copyFrom.KSExpirationSeconds;
            }

            return new GroupDTO
            {
                UserSecret = Guid.NewGuid().ToString().Replace("-", ""),
                RevokedKsMaxTtlSeconds = revokedKsMaxTtlSeconds,
                UseStartDate = true,
                GetOnlyActiveAssets = true,
                ShouldSupportSingleLogin = false,
                TokenKeyFormat = "token_{0}",
                RefreshTokenExpirationSeconds = 1728000,
                IsRefreshTokenExtendable = false,
                IsSwitchingUsersAllowed = true,
                AppTokenKeyFormat = "app_token_{0}",
                UserSessionsKeyFormat = "sessions_{0}",
                RevokedKsKeyFormat = "r_ks_{0}",
                UploadTokenKeyFormat = "upload_token_{0}",
                RevokedSessionKeyFormat = "r_session_{0}",
                IsRefreshTokenEnabled = false,
                ShouldCheckDeviceInDomain = true,
                EnforceGroupsSecret = false,
                AccountPrivateKey = copyFrom.AccountPrivateKey,
                AdminSecret = copyFrom.AdminSecret,
                AdvertisingValuesMetas = copyFrom.AdvertisingValuesMetas,
                AdvertisingValuesTags = copyFrom.AdvertisingValuesTags,
                AnonymousKSExpirationSeconds = copyFrom.AnonymousKSExpirationSeconds,
                ApiCredentials = ConvertCredentialsToDTO(copyFrom.ApiCredentials),
                AppTokenMaxExpirySeconds = copyFrom.AppTokenMaxExpirySeconds,
                AppTokenSessionMaxDurationSeconds = copyFrom.AppTokenSessionMaxDurationSeconds,
                ApptokenUserValidationDisabled = copyFrom.ApptokenUserValidationDisabled,
                BillingCredentials = ConvertCredentialsToDTO(copyFrom.BillingCredentials),
                DomainsCredentials = ConvertCredentialsToDTO(copyFrom.DomainsCredentials),
                ConditionalAccessCredentials = ConvertCredentialsToDTO(copyFrom.ConditionalAccessCredentials),
                FairplayCertificate = copyFrom.FairplayCertificate,
                KSExpirationSeconds = copyFrom.KSExpirationSeconds,
                Languages = copyFrom.Languages.Select(x => new LanguageDTO {
                    Code = x.Code,
                    Direction = x.Direction,
                    Id = x.Id,
                    IsDefault = x.IsDefault,
                    Name = x.Name
                }).ToList(),
                MediaPrepAccountId = copyFrom.MediaPrepAccountId,
                MediaPrepAccountSecret = copyFrom.MediaPrepAccountSecret,
                NotificationsCredentials = ConvertCredentialsToDTO(copyFrom.NotificationsCredentials),
                PricingCredentials = ConvertCredentialsToDTO(copyFrom.PricingCredentials),
                RefreshExpirationForPinLoginSeconds = copyFrom.RefreshExpirationForPinLoginSeconds,
                ShouldSupportFriendlyURL = copyFrom.ShouldSupportFriendlyURL,
                SocialCredentials = ConvertCredentialsToDTO(copyFrom.SocialCredentials),
                UDrmUrl = copyFrom.UDrmUrl,
                UploadTokenExpirySeconds = copyFrom.UploadTokenExpirySeconds,
                UsersCredentials = ConvertCredentialsToDTO(copyFrom.UsersCredentials),
                UserSecretFallback = copyFrom.UserSecretFallback,
                UserSecretFallbackExpiryEpoch = copyFrom.UserSecretFallbackExpiryEpoch
            };
        }

        private static CredentialsDTO ConvertCredentialsToDTO(Credentials copyFrom) 
        {
            return new CredentialsDTO
            {
                Username = copyFrom.Username,
                Password = copyFrom.Password
            };
        }

        private static GroupDTO GroupIsDeepEqual(GroupDTO expected)
        {
            return Match.Create<GroupDTO>(actual =>
            {
                actual.Should().BeEquivalentTo(expected, config => config.Excluding(a => a.UserSecret));
                return true;
            });
        }
    }
}
