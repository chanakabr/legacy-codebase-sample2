using FluentAssertions;
using LiveToVod.BOL;
using LiveToVod.DAL;
using NUnit.Framework;

namespace LiveToVod.Tests.DAL
{
    [TestFixture]
    public class MapperTests
    {
        [Test]
        public void Map_LiveToVodPartnerConfigurationIsNull_ReturnsNull()
        {
            var result = Mapper.Map(null, 2);

            result.Should().BeNull();
        }

        [Test]
        public void Map_LiveToVodPartnerConfigurationIsValid_ReturnsExpectedResult()
        {
            var config = new LiveToVodPartnerConfiguration(true, 10, "metaDataClassifier");

            var result = Mapper.Map(config, 2);

            result.Should().NotBeNull();
            result.Id.Should().Be(LiveToVodPartnerConfigurationData.PARTNER_CONFIG_DOCUMENT_ID);
            result.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            result.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
            result.MetadataClassifier.Should().Be(config.MetadataClassifier);
            result.LastUpdaterId.Should().Be(2);
        }

        [Test]
        public void Map_LiveToVodLinearAssetConfigurationIsNull_ReturnsNull()
        {
            var result = Mapper.Map(1, null, 2);

            result.Should().BeNull();
        }

        [Test]
        public void Map_LiveToVodLinearAssetConfigurationIsValid_ReturnsExpectedResult()
        {
            var config = new LiveToVodLinearAssetConfiguration(10, true, 20);

            var result = Mapper.Map(1, config, 2);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(config.LinearAssetId);
            result.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            result.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
            result.LastUpdaterId.Should().Be(2);
        }

        [Test]
        public void Map_LiveToVodPartnerConfigurationDataIsNull_ReturnsNull()
        {
            var result = Mapper.Map((LiveToVodPartnerConfigurationData)null);

            result.Should().BeNull();
        }

        [Test]
        public void Map_LiveToVodPartnerConfigurationDataIsValid_ReturnsExpectedResult()
        {
            var config = new LiveToVodPartnerConfigurationData
            {
                Id = 1,
                IsLiveToVodEnabled = true,
                RetentionPeriodDays = 10,
                MetadataClassifier = "metaDataClassifier",
                LastUpdaterId = 2
            };

            var result = Mapper.Map(config);

            result.Should().NotBeNull();
            result.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            result.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
            result.MetadataClassifier.Should().Be(config.MetadataClassifier);
        }

        [Test]
        public void Map_LiveToVodLinearAssetConfigurationDataIsNull_ReturnsNull()
        {
            var result = Mapper.Map((LiveToVodLinearAssetConfigurationData)null);

            result.Should().BeNull();
        }

        [Test]
        public void Map_LiveToVodLinearAssetConfigurationDataIsValid_ReturnsExpectedResult()
        {
            var config = new LiveToVodLinearAssetConfigurationData
            {
                LinearAssetId = 10,
                IsLiveToVodEnabled = true,
                RetentionPeriodDays = 20,
                LastUpdaterId = 2
            };

            var result = Mapper.Map(config);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(config.LinearAssetId);
            result.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            result.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
        }
    }
}