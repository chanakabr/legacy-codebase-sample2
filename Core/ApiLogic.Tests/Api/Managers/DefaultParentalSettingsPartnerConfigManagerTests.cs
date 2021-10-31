using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using AutoFixture;
using Core.Api;
using DAL;
using Moq;
using NUnit.Framework;
using System.Collections;


namespace ApiLogic.Tests
{
    [TestFixture]
    public class DefaultParentalSettingsPartnerConfigManagerTests
    {
        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IDefaultParentalSettingsPartnerRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(updateTestCase.DefaultParentalSettingsOld, true, false);
            var parentalRuleManagerMock = new Mock<IParentalRuleManager>();


            if (updateTestCase.IsUpdate)
            {
                parentalRuleManagerMock.Setup(x => x.GetParentalRules(It.IsAny<int>(), true)).Returns(updateTestCase.ParentalRulesResponse);
                repositoryMock.Setup(x => x.UpdateDefaultParentalSettingsPartnerConfig(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<DefaultParentalSettingsPartnerConfig>())).Returns(updateTestCase.IsUpsertSuccess);
            }
            else
            {
                repositoryMock.Setup(x => x.InsertDefaultParentalSettingsPartnerConfig(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<DefaultParentalSettingsPartnerConfig>())).Returns(updateTestCase.IsUpsertSuccess);
            }

            DefaultParentalSettingsPartnerConfigManager manager = new DefaultParentalSettingsPartnerConfigManager(repositoryMock.Object, layeredCacheMock.Object, parentalRuleManagerMock.Object);
            var response = manager.UpsertParentalDefaultConfig(fixture.Create<int>(), fixture.Create<long>(), updateTestCase.DefaultParentalSettingsToUpdate);

            Assert.That(response.Code, Is.EqualTo((int)updateTestCase.ResponseStatus));

            if (updateTestCase.IsUpdate)
            {
                repositoryMock.Verify(x => x.UpdateDefaultParentalSettingsPartnerConfig(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<DefaultParentalSettingsPartnerConfig>()), Times.Exactly(updateTestCase.AmountCallToRepository));
            } else
            {
                repositoryMock.Verify(x => x.InsertDefaultParentalSettingsPartnerConfig(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<DefaultParentalSettingsPartnerConfig>()), Times.Exactly(updateTestCase.AmountCallToRepository));
            }
        }

        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdate: false, amountCallToRepository: 1)).SetName("CheckUpdatePartnerSettingsDoesntExistSoInsert");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, amountCallToRepository: 1)).SetName("CheckUpdateWithValidationSuccess");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, isValidationSuccess: false)).SetName("CheckUpdateWithValidationFail");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error)).SetName("CheckDBFils");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdateNeeded : false)).SetName("CheckNotUpdateWhenNotNeeded");
        }

        public class UpdateTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal DefaultParentalSettingsPartnerConfig DefaultParentalSettingsOld { get; private set; }
            internal DefaultParentalSettingsPartnerConfig DefaultParentalSettingsToUpdate { get; private set; }
            internal ParentalRulesResponse ParentalRulesResponse { get; private set; }
            internal bool IsUpsertSuccess { get; private set; }
            internal bool IsUpdate { get; private set; }
            internal int AmountCallToRepository { get; private set; }
            public UpdateTestCase(eResponseStatus responseStatus, bool isUpdate = true, int amountCallToRepository = 0, bool isValidationSuccess = true, bool isUpdateNeeded = true)
            {
                ResponseStatus = responseStatus;
                IsUpsertSuccess = responseStatus == 0;
                IsUpdate = isUpdate;
                AmountCallToRepository = amountCallToRepository;
                DefaultParentalSettingsToUpdate = fixture.Create<DefaultParentalSettingsPartnerConfig>();

                if (isUpdate)
                {
                    DefaultParentalSettingsOld = isUpdateNeeded ? fixture.Create<DefaultParentalSettingsPartnerConfig>() : DefaultParentalSettingsToUpdate;
                    ParentalRulesResponse = fixture.Create<ParentalRulesResponse>();
                    ParentalRulesResponse.status.Set(responseStatus);

                    DefaultParentalSettingsToUpdate.DefaultMoviesParentalRuleId = ParentalRulesResponse.rules[0].id;
                    DefaultParentalSettingsToUpdate.DefaultTvSeriesParentalRuleId = ParentalRulesResponse.rules[0].id;
                    if (!isValidationSuccess)
                    {
                        DefaultParentalSettingsToUpdate.DefaultMoviesParentalRuleId = -1;
                        DefaultParentalSettingsToUpdate.DefaultTvSeriesParentalRuleId = -1;
                    }
                }
            }
        }
    }
}