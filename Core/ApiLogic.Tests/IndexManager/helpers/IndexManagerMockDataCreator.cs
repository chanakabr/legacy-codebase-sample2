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
                LanguageMapById = languages.ToDictionary(x => x.ID)
            };

            _mockCatalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(partnerId, out catalogGroupCache)).Returns(true);
        }

        public static LanguageObj GetRandomLanguage()
        {
            var language = new ApiObjects.LanguageObj()
            {
                ID = _random.Next(1000) + 1,
                Code = "en",
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
            return new TagValue()
            {
                tagId = _random.Next(1000) + 1,
                value = "test",
                topicId = 2,
                languageId = languageId,
                createDate = 1000,
                updateDate = 1000
            };
        }
    }
}