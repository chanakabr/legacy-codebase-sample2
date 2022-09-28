using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoFixture;
using Core.Catalog;
using Core.GroupManagers;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using NUnit.Framework;
using TvinciCache;

namespace ApiLogic.Tests.Catalog.Helpers
{
    [TestFixture]
    public class EpgAssetMultilingualMutatorTests
    {
        private MockRepository _mockRepository;
        private Mock<ICatalogPartnerConfigManager> _catalogPartnerConfigManagerMock;
        private Mock<IGroupSettingsManager> _groupSettingsManagerMock;
        private Mock<IGroupsFeatures> _groupsFeaturesMock;

        private const int GroupId = 1483;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _catalogPartnerConfigManagerMock = _mockRepository.Create<ICatalogPartnerConfigManager>();
            _groupSettingsManagerMock = _mockRepository.Create<IGroupSettingsManager>();
            _groupsFeaturesMock = _mockRepository.Create<IGroupsFeatures>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void IsAllowedToFallback_ShouldReturnFalse_IfThereIsOnlyDefaultLanguageOrNone()
        {
            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);

            var isAllowedToFallback = epgAssetMultilingualMutator.IsAllowedToFallback(It.IsAny<int>(), new Dictionary<string, LanguageObj>());
            
            isAllowedToFallback.Should().BeFalse();
        }
        
        [Test]
        public void IsAllowedToFallback_ShouldReturnFalse_IfIsNotOPCAccount()
        {
            var languages = GetLanguages();
            _groupSettingsManagerMock.Setup(x => x.DoesGroupUsesTemplates(GroupId)).Returns(false);

            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);
            var isAllowedToFallback = epgAssetMultilingualMutator.IsAllowedToFallback(GroupId, languages);
            
            isAllowedToFallback.Should().BeFalse();
        }
        
        [Test]
        public void IsAllowedToFallback_ShouldReturnFalse_IfEPGV2TurnedOff()
        {
            var languages = GetLanguages();
            _groupSettingsManagerMock.Setup(x => x.DoesGroupUsesTemplates(GroupId)).Returns(true);
            _groupSettingsManagerMock.Setup(x => x.GetEpgFeatureVersion(GroupId)).Returns(EpgFeatureVersion.V1);
            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);
            var isAllowedToFallback = epgAssetMultilingualMutator.IsAllowedToFallback(GroupId, languages);
            
            isAllowedToFallback.Should().BeFalse();
        }
        
        [Test]
        public void IsAllowedToFallback_ShouldReturnFalse_IfCannotFetchCatalogPartnerConfig()
        {
            var languages = GetLanguages();
            _groupSettingsManagerMock.Setup(x => x.DoesGroupUsesTemplates(GroupId)).Returns(true);
            _groupSettingsManagerMock.Setup(x => x.GetEpgFeatureVersion(GroupId)).Returns(EpgFeatureVersion.V2);
            _catalogPartnerConfigManagerMock.Setup(x => x.GetCatalogConfig(GroupId)).Returns(new GenericResponse<CatalogPartnerConfig>());

            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);
            var isAllowedToFallback = epgAssetMultilingualMutator.IsAllowedToFallback(GroupId, languages);
            
            isAllowedToFallback.Should().BeFalse();
        }
        
        [TestCase(false)]
        [TestCase(null)]
        public void IsAllowedToFallback_ShouldReturnFalse_IfEpgMultilingualFallbackSupportTurnedOff(bool? epgMultilingualFallbackSupport)
        {
            var languages = GetLanguages();
            _groupSettingsManagerMock.Setup(x => x.DoesGroupUsesTemplates(GroupId)).Returns(true);
            _groupSettingsManagerMock.Setup(x => x.GetEpgFeatureVersion(GroupId)).Returns(EpgFeatureVersion.V2);
            var genericResponse = new GenericResponse<CatalogPartnerConfig>(new Status(eResponseStatus.OK), new CatalogPartnerConfig {EpgMultilingualFallbackSupport = epgMultilingualFallbackSupport});
            _catalogPartnerConfigManagerMock.Setup(x => x.GetCatalogConfig(GroupId)).Returns(genericResponse);

            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);
            var isAllowedToFallback = epgAssetMultilingualMutator.IsAllowedToFallback(GroupId, languages);
            
            isAllowedToFallback.Should().BeFalse();
        }
        
        [Test]
        public void IsAllowedToFallback_ShouldReturnTrue()
        {
            var languages = GetLanguages();
            _groupSettingsManagerMock.Setup(x => x.DoesGroupUsesTemplates(GroupId)).Returns(true);
            _groupSettingsManagerMock.Setup(x => x.GetEpgFeatureVersion(GroupId)).Returns(EpgFeatureVersion.V2);
            var genericResponse = new GenericResponse<CatalogPartnerConfig>(new Status(eResponseStatus.OK), new CatalogPartnerConfig {EpgMultilingualFallbackSupport = true});
            _catalogPartnerConfigManagerMock.Setup(x => x.GetCatalogConfig(GroupId)).Returns(genericResponse);

            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);
            var isAllowedToFallback = epgAssetMultilingualMutator.IsAllowedToFallback(GroupId, languages);
            
            isAllowedToFallback.Should().BeTrue();
        }

        [Test]
        public void PrepareEpgAsset_ShouldNotModifyNamesAndDescriptions_SpecificLanguagesAreFilled()
        {
            var languages = GetLanguages();
            var defaultLanguage = languages.Single(l => l.Value.IsDefault).Value;
            _groupSettingsManagerMock.Setup(x => x.DoesGroupUsesTemplates(GroupId)).Returns(true);
            _groupSettingsManagerMock.Setup(x => x.GetEpgFeatureVersion(GroupId)).Returns(EpgFeatureVersion.V2);
            var genericResponse = new GenericResponse<CatalogPartnerConfig>(new Status(eResponseStatus.OK), new CatalogPartnerConfig {EpgMultilingualFallbackSupport = true});
            _catalogPartnerConfigManagerMock.Setup(x => x.GetCatalogConfig(GroupId)).Returns(genericResponse);
            
            var fixture = new Fixture();
            var epgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = "default name",
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("arb", fixture.Create<string>()),
                    new LanguageContainer("csp", fixture.Create<string>()),
                },
                Description = "description defualt",
                DescriptionsWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("arb", fixture.Create<string>()),
                    new LanguageContainer("csp", fixture.Create<string>()),
                }
            };
            var expectedEpgAsset = epgAsset.DeepClone();
            
            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);
            epgAssetMultilingualMutator.PrepareEpgAsset(GroupId, epgAsset, defaultLanguage, languages);

            epgAsset.Should().BeEquivalentTo(expectedEpgAsset);
        }
        
        [TestCaseSourceAttribute(nameof(PrepareEpgAssetTestCaseSource))]
        public void PrepareEpgAsset_WithNamesAndDescriptions(EpgAsset epgAsset, EpgAsset expectedEpgAsset)
        {
            var languages = GetLanguages();
            var defaultLanguage = languages.Single(l => l.Value.IsDefault).Value;
            _groupSettingsManagerMock.Setup(x => x.DoesGroupUsesTemplates(GroupId)).Returns(true);
            _groupSettingsManagerMock.Setup(x => x.GetEpgFeatureVersion(GroupId)).Returns(EpgFeatureVersion.V2);
            var genericResponse = new GenericResponse<CatalogPartnerConfig>(new Status(eResponseStatus.OK), new CatalogPartnerConfig {EpgMultilingualFallbackSupport = true});
            _catalogPartnerConfigManagerMock.Setup(x => x.GetCatalogConfig(GroupId)).Returns(genericResponse);
            
            var epgAssetMultilingualMutator = new EpgAssetMultilingualMutator(_catalogPartnerConfigManagerMock.Object, _groupSettingsManagerMock.Object, _groupsFeaturesMock.Object);
            epgAssetMultilingualMutator.PrepareEpgAsset(GroupId, epgAsset, defaultLanguage, languages);

            epgAsset.Should().BeEquivalentTo(expectedEpgAsset);
        }

        private static IEnumerable<object> PrepareEpgAssetTestCaseSource()
        {
            var fixture = new Fixture();
            var defaultName = fixture.Create<string>();
            var defaultDescription = fixture.Create<string>();
            var defaultTagValue = fixture.Create<string>();
            var defaultMetaValue = fixture.Create<string>();

            #region Name, Descriptions, Metas and Tags language containers are null

            var epgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = null,
                Description = defaultDescription,
                DescriptionsWithLanguages =  null,
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString())
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string>{defaultTagValue}
                    }
                }
            };
            var expectedEpgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultName),
                    new LanguageContainer("arb", defaultName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultDescription),
                    new LanguageContainer("arb", defaultDescription),
                },
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString()),
                        Value = new LanguageContainer[]
                        {
                            new LanguageContainer("csp", defaultMetaValue),
                            new LanguageContainer("arb", defaultMetaValue),
                        }
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string> {defaultTagValue},
                        Values = new List<LanguageContainer[]>
                        {
                            new[]
                            {
                                new LanguageContainer("csp", defaultTagValue),
                                new LanguageContainer("arb", defaultTagValue),
                            }
                        }
                    }
                }
            };
            yield return new TestCaseData(epgAsset, expectedEpgAsset).SetName("Name, Descriptions, Metas and Tags language containers are null");

            #endregion

            #region Name, Descriptions, Metas and Tags language containers are empty arrays.

            epgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>(),
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>(),
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString())
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string>{defaultTagValue}
                    }
                }
            };
            expectedEpgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultName),
                    new LanguageContainer("arb", defaultName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultDescription),
                    new LanguageContainer("arb", defaultDescription),
                },
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString()),
                        Value = new LanguageContainer[]
                        {
                            new LanguageContainer("csp", defaultMetaValue),
                            new LanguageContainer("arb", defaultMetaValue),
                        }
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string> {defaultTagValue},
                        Values = new List<LanguageContainer[]>
                        {
                            new[]
                            {
                                new LanguageContainer("csp", defaultTagValue),
                                new LanguageContainer("arb", defaultTagValue),
                            }
                        }
                    }
                }
            };
            yield return new TestCaseData(epgAsset, expectedEpgAsset).SetName("Name, Descriptions, Metas and Tags language containers are empty arrays.");

            #endregion

            #region Name, Descriptions, Metas and Tags language containers are set for one non-default language.

            var arbName = fixture.Create<string>();
            var arbDescription = fixture.Create<string>();
            var arbMetaValue = fixture.Create<string>();
            var arbTagValue = fixture.Create<string>();
            epgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("arb", arbName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("arb", arbDescription),
                },
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString()),
                        Value = new LanguageContainer[]
                        {
                            new LanguageContainer("arb", arbMetaValue),
                        }
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string>{defaultTagValue},
                        Values = new List<LanguageContainer[]>
                        {
                            new[]
                            {
                                new LanguageContainer("arb", arbTagValue),
                            }
                        }
                    }
                }
            };
            expectedEpgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultName),
                    new LanguageContainer("arb", arbName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultDescription),
                    new LanguageContainer("arb", arbDescription),
                },
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString()),
                        Value = new[]
                        {
                            new LanguageContainer("csp", defaultMetaValue),
                            new LanguageContainer("arb", arbMetaValue),
                        }
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string> {defaultTagValue},
                        Values = new List<LanguageContainer[]>
                        {
                            new[]
                            {
                                new LanguageContainer("csp", defaultTagValue),
                                new LanguageContainer("arb", arbTagValue),
                            }
                        }
                    }
                }
            };
            yield return new TestCaseData(epgAsset, expectedEpgAsset).SetName("Name, Descriptions, Metas and Tags language containers are set for one non-default language.");

            #endregion

            #region Name, Descriptions, Metas and Tags language containers are set for every non-default language.

            arbName = fixture.Create<string>();
            arbDescription = fixture.Create<string>();
            var cspName = fixture.Create<string>();
            var cspDescription = fixture.Create<string>();
            var cspMetaValue = fixture.Create<string>();
            var cspTagValue = fixture.Create<string>();
            epgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("csp", cspName),
                    new LanguageContainer("arb", arbName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", cspDescription),
                    new LanguageContainer("arb", arbDescription),
                },
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString()),
                        Value = new[]
                        {
                            new LanguageContainer("csp", cspMetaValue),
                            new LanguageContainer("arb", arbMetaValue),
                        }
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string>{defaultTagValue},
                        Values = new List<LanguageContainer[]>
                        {
                            new[]
                            {
                                new LanguageContainer("csp", cspTagValue),
                                new LanguageContainer("arb", arbTagValue),
                            }
                        }
                    }
                }
            };
            expectedEpgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("csp", cspName),
                    new LanguageContainer("arb", arbName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", cspDescription),
                    new LanguageContainer("arb", arbDescription),
                },
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.MultilingualString.ToString()),
                        Value = new[]
                        {
                            new LanguageContainer("csp", cspMetaValue),
                            new LanguageContainer("arb", arbMetaValue),
                        }
                    }
                },
                Tags = new List<Tags>
                {
                    new Tags
                    {
                        m_lValues = new List<string>{defaultTagValue},
                        Values = new List<LanguageContainer[]>
                        {
                            new[]
                            {
                                new LanguageContainer("csp", cspTagValue),
                                new LanguageContainer("arb", arbTagValue),
                            }
                        }
                    }
                }
            };
            yield return new TestCaseData(epgAsset, expectedEpgAsset).SetName("Name, Descriptions, Metas and Tags language containers are set for every non-default language.");

            #endregion

            #region Metas without type MultilingualString should be processed as well.

            epgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = null,
                Description = defaultDescription,
                DescriptionsWithLanguages =  null,
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.Bool.ToString())
                    },
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.String.ToString())
                    },
                }
            };
            expectedEpgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultName),
                    new LanguageContainer("arb", defaultName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultDescription),
                    new LanguageContainer("arb", defaultDescription),
                },
                Metas = new List<Metas>
                {
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.Bool.ToString()),
                        Value = new []
                        {
                            new LanguageContainer("csp", defaultMetaValue),
                            new LanguageContainer("arb", defaultMetaValue),
                        }
                    },
                    new Metas
                    {
                        m_sValue = defaultMetaValue,
                        m_oTagMeta = new TagMeta(string.Empty, MetaType.String.ToString()),
                        Value = new []
                        {
                            new LanguageContainer("csp", defaultMetaValue),
                            new LanguageContainer("arb", defaultMetaValue),
                        }
                    },
                }
            };
            yield return new TestCaseData(epgAsset, expectedEpgAsset).SetName("Metas without type MultilingualString should be processed as well.");

            #endregion

            #region Nulls for Description should be replaced if there is a default value.

            epgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = null,
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", "234"),
                    new LanguageContainer("arb", null),
                },
            };
            expectedEpgAsset = new EpgAsset
            {
                GroupId = GroupId,
                Name = defaultName,
                NamesWithLanguages = new List<LanguageContainer>
                {
                    new LanguageContainer("csp", defaultName),
                    new LanguageContainer("arb", defaultName),
                },
                Description = defaultDescription,
                DescriptionsWithLanguages =  new List<LanguageContainer>
                {
                    new LanguageContainer("csp", "234"),
                    new LanguageContainer("arb", defaultDescription),
                }
            };
            yield return new TestCaseData(epgAsset, expectedEpgAsset).SetName("Nulls for Description should be replaced if there is a default value.");

            #endregion
        }

        private static IDictionary<string, LanguageObj> GetLanguages()
        {
            return new Dictionary<string, LanguageObj>
            {
                {"eng", new LanguageObj(1, "name", "eng", string.Empty, true, string.Empty)},
                {"csp", new LanguageObj(2, "name", "csp", string.Empty, false, string.Empty)},
                {"arb", new LanguageObj(3, "name", "arb", string.Empty, false, string.Empty)},
            };
        }
    }
}