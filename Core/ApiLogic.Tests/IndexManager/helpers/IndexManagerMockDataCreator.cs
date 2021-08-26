using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using GroupsCacheManager;
using Moq;
using Utils = ElasticSearch.Common.Utils;

namespace ApiLogic.Tests.IndexManager.helpers
{
    public static class IndexManagerMockDataCreator
    {
        private static int _nextPartnerId = 10000;
        private static Random _random=new Random();

        public static void SetupOpcPartnerMocks(int partnerId, IEnumerable<LanguageObj> languages,ref Mock<ICatalogManager> _mockCatalogManager)
        {
            _mockCatalogManager.Setup(x => x.DoesGroupUsesTemplates(partnerId)).Returns(true);

            var catalogGroupCache = new CatalogGroupCache()
            {
                LanguageMapByCode = languages.ToDictionary(x => x.Code),
                LanguageMapById = languages.ToDictionary(x => x.ID),
            };
            catalogGroupCache.TopicsMapBySystemNameAndByType.Add(
                "test_tag",
                new Dictionary<string, Topic>()
                {
                    {
                        ApiObjects.MetaType.Tag.ToString(),
                        new Topic()
                            {
                                
                            }
                    }
                }
            );
            catalogGroupCache.TopicsMapBySystemNameAndByType.Add(
                "test_numeric_meta",
                new Dictionary<string, Topic>()
                {
                    {
                        ApiObjects.MetaType.Number.ToString(),
                        new Topic()
                            {
                                Type = ApiObjects.MetaType.Number
                            }
                    }
                }
            );
            catalogGroupCache.TopicsMapBySystemNameAndByType.Add(
                "test_string_meta",
                new Dictionary<string, Topic>()
                {
                                {
                                    ApiObjects.MetaType.String.ToString(),
                                    new Topic()
                                        {

                                        }
                                }
                }
            );
            //_catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => x.Value.ContainsKey(ApiObjects.MetaType.Tag.ToString()) && !topicsToIgnore.Contains(x.Key)).Select(x => x.Key.ToLower()).ToList();
            _mockCatalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(partnerId, out catalogGroupCache)).Returns(true);
        }

        public static LanguageObj GetEnglishLanguageWithRandomId()
        {
            var language = new ApiObjects.LanguageObj()
            {
                ID = _random.Next(1000) + 1,
                Code = "eng",
                Name = "english",
                IsDefault = true,
            };
            return language;
        }

        public static int GetRandomPartnerId()
        {
            var partnerId = _nextPartnerId++ + _random.Next(1000);
            return partnerId;
        }

        public static Media GetRandomMedia(int partnerId)
        {
            var rand = _random.Next(1000);
            return new Media()
            {
                m_nIsActive = 1,
                m_nMediaID = 12345 + rand,
                m_sName = $"test media {rand}",
                m_sDescription = "test description",
                m_nGroupID = partnerId,
                m_sStartDate = DateTime.Today.AddDays(-1).ToString(Utils.ES_DATE_FORMAT),
                m_nMediaTypeID = rand + 1
            };
        }

        public static Channel GetRandomChannel(int randomPartnerId)
        {
            var randomNum = _random.Next(1000) + 1;
            return new Channel()
            {
                m_nParentGroupID = randomPartnerId,
                m_nChannelID = randomNum,
                m_sName = $"some name for tests {randomNum}",
                CreateDate = DateTime.Now.AddDays(-1),
                UpdateDate = DateTime.Now.AddDays(-1),
                SystemName = $"test {randomNum}",
                AssetUserRuleId = randomNum,
                m_nIsActive = 1
            };
        }

        public static SocialActionStatistics GetRandomSocialActionStat(int randomPartnerId)
        {
            return new SocialActionStatistics()
            {
                GroupID = randomPartnerId,
                Date = DateTime.Now.AddDays(-1),
                MediaID = _random.Next(1000),
                Action = "like",
                Count = 10
            };
        }
        
        public static TagValue GetRandomTag(int languageId)
        {
            var random = _random.Next(1000) + 1;
            return new TagValue()
            {
                tagId = random,
                value = $"test-{random}",
                topicId = 2,
                languageId = languageId,
                createDate = 1000,
                updateDate = 1000
            };
        }
        
        public static EpgCB GetRandomEpgCb(DateTime? startDate=null, string name = "",string description="")
        {
            var epgCb = new EpgCB();
            var epgId = 1 + new Random().Next(1000);
            epgCb.Name = name == string.Empty ? "la movie" : name;
            epgCb.Description = description == "" ? "this is the movie description" : description;
            epgCb.EpgID = (ulong) epgId;
            epgCb.Language = "en";
            epgCb.CreateDate= DateTime.Now.ToUniversalTime();
            epgCb.EndDate = startDate.HasValue ? startDate.Value : DateTime.Now.AddDays(1).ToUniversalTime();
            epgCb.StartDate=startDate ??DateTime.Now.ToUniversalTime();
            epgCb.CreateDate=DateTime.Now.ToUniversalTime();
            epgCb.EpgIdentifier = epgId.ToString();
            epgCb.DocumentId = $"{epgId}";
            return epgCb;
        }
    }
}