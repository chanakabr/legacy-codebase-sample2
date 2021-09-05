using System;
using System.Collections.Generic;
using System.Data;
using ApiLogic.Users;
using Core.Users;
using DAL;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Users.Services
{
    [TestFixture]
    public class DomainLimitationModuleRepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<IDomainLimitationModuleDal> _domainLimitationModuleDalMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _domainLimitationModuleDalMock = _mockRepository.Create<IDomainLimitationModuleDal>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Add_ValidParameters_ReturnsLimitationsManager()
        {
            var fakeLimits = FakeLimits();
            _domainLimitationModuleDalMock
                .Setup(x => x.InsertGroupLimitsAndDeviceFamilies(It.IsAny<DAL.DTO.LimitationsManagerDTO>(), 1, 7))
                .Returns(FakeLimitationsManagerResponse1());
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Add(1, 7, FakeLimitationsManager());

            result.Should().NotBeNull();
        }

        [Test]
        public void Add_ExceptionIsThrown_ReturnsNull()
        {
            var fakeLimits = FakeLimits();
            _domainLimitationModuleDalMock
                .Setup(x => x.InsertGroupLimitsAndDeviceFamilies(It.IsAny<DAL.DTO.LimitationsManagerDTO>(), 1, 7))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while Add: message.");
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Add(1, 7, FakeLimitationsManager());

            result.Should().BeNull();
        }

        [Test]
        public void Get_ValidParameters_ReturnsLimitationsManager()
        {
            _domainLimitationModuleDalMock
                .Setup(x => x.GetGroupLimitsAndDeviceFamilies(1, 1))
                .Returns(FakeLimitationsManagerResponse1());
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Get(1, 1);

            result.Should().NotBeNull();
            result.domianLimitID.Should().Be(1);
            result.DomainLimitName.Should().Be("Domain Limit Name");
            result.Concurrency.Should().Be(2);
            result.npvrQuotaInSecs.Should().Be(0);
            result.Frequency.Should().Be(3);
            result.Quantity.Should().Be(4);
            result.nUserLimit.Should().Be(6);
            result.UserFrequency.Should().Be(5);
            result.lDeviceFamilyLimitations.Count.Should().Be(2);
            result.lDeviceFamilyLimitations[0].Should().NotBeNull();
            result.lDeviceFamilyLimitations[0].deviceFamily.Should().Be(1);
            result.lDeviceFamilyLimitations[0].deviceFamilyName.Should().Be("Device Family 1");
            result.lDeviceFamilyLimitations[0].concurrency.Should().Be(2);
            result.lDeviceFamilyLimitations[0].quantity.Should().Be(4);
            result.lDeviceFamilyLimitations[0].Frequency.Should().Be(-1);
            result.lDeviceFamilyLimitations[1].Should().NotBeNull();
            result.lDeviceFamilyLimitations[1].deviceFamily.Should().Be(2);
            result.lDeviceFamilyLimitations[1].deviceFamilyName.Should().Be("Device Family 2");
            result.lDeviceFamilyLimitations[1].concurrency.Should().Be(5);
            result.lDeviceFamilyLimitations[1].quantity.Should().Be(7);
            result.lDeviceFamilyLimitations[1].Frequency.Should().Be(6);
        }

        [Test]
        public void Get_GroupOrDomainLimitNotFound_ReturnsNull()
        {
            DAL.DTO.LimitationsManagerDTO dlmDTO = null;

            _domainLimitationModuleDalMock
                .Setup(x => x.GetGroupLimitsAndDeviceFamilies(1, 1))
                .Returns(dlmDTO);
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Get(1, 1);

            result.Should().BeNull();
        }

        [Test]
        public void GetDomainLimitationModuleIds_ValidParameters_ReturnsLimitationModuleIds()
        {
            _domainLimitationModuleDalMock
                .Setup(x => x.GetGroupDeviceLimitationModules(1))
                .Returns(FakeLimitationModuleIds(3));
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.GetDomainLimitationModuleIds(1);

            result.Should().Equal(1, 2, 3);
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void GetDomainLimitationModuleIds_LimitationModulesNotFound_ReturnsNull(int rowCount)
        {
            _domainLimitationModuleDalMock
                .Setup(x => x.GetGroupDeviceLimitationModules(1))
                .Returns(FakeLimitationModuleIds(rowCount));
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.GetDomainLimitationModuleIds(1);

            result.Should().BeNull();
        }

        [Test]
        public void GetDomainLimitationModuleIds_ExceptionIsThrown_ReturnsFalse()
        {
            _domainLimitationModuleDalMock
                .Setup(x => x.GetGroupDeviceLimitationModules(1))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while GetDomainLimitationModuleIds: groupId=1, message.");
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.GetDomainLimitationModuleIds(1);

            result.Should().BeNull();
        }

        [Test]
        public void Delete_SuccessfullyDeleted_ReturnsTrue()
        {
            _domainLimitationModuleDalMock
                .Setup(x => x.DeleteGroupLimitsAndDeviceFamilies(1, 2))
                .Returns(1);
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Delete(1, 2);

            result.Should().BeTrue();
        }

        [Test]
        public void Delete_FailedToDelete_ReturnsFalse()
        {
            _domainLimitationModuleDalMock
                .Setup(x => x.DeleteGroupLimitsAndDeviceFamilies(1, 2))
                .Returns(0);
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Delete(1, 2);

            result.Should().BeFalse();
        }

        [Test]
        public void Delete_ExceptionIsThrown_ReturnsFalse()
        {
            _domainLimitationModuleDalMock
                .Setup(x => x.DeleteGroupLimitsAndDeviceFamilies(1, 2))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while Delete: limitId=1, message.");
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Delete(1, 2);

            result.Should().BeFalse();
        }

        [Test]
        public void Update_ValidParameters_ReturnsLimitationsManager()
        {
            var fakeLimits = FakeLimits();
            _domainLimitationModuleDalMock
                .Setup(x => x.UpdateGroupLimitsAndDeviceFamilies(1, 7, It.IsAny<DAL.DTO.LimitationsManagerDTO>()))
                .Returns(FakeLimitationsManagerResponse1());
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Update(1, 7, FakeLimitationsManager());

            result.Should().NotBeNull();
        }

        [Test]
        public void Update_ExceptionIsThrown_ReturnsNull()
        {
            var fakeLimits = FakeLimits();
            _domainLimitationModuleDalMock
                .Setup(x => x.UpdateGroupLimitsAndDeviceFamilies(1, 7, It.IsAny<DAL.DTO.LimitationsManagerDTO>()))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error,$"Error with DB while trying to Update: limitId=1, exception=message.");
            var domainLimitationModuleRepository = new DomainLimitationModuleRepository(_domainLimitationModuleDalMock.Object, _loggerMock.Object);

            var result = domainLimitationModuleRepository.Update(1, 7, FakeLimitationsManager());

            result.Should().BeNull();
        }

        private DeviceFamilyLimitations[] FakeDeviceFamilyLimitations()
        {
            return new[]
            {
                new DeviceFamilyLimitations
                {
                    deviceFamily = 1,
                    concurrency = 11,
                    quantity = 12,
                    Frequency = 13
                },
                new DeviceFamilyLimitations
                {
                    deviceFamily = 2,
                    concurrency = 21,
                    quantity = 22,
                    Frequency = 23
                }
            };
        }

        private Tuple<IEnumerable<KeyValuePair<int, int>>, IEnumerable<KeyValuePair<int, int>>, IEnumerable<KeyValuePair<int, int>>> FakeLimits()
        {
            return new Tuple<IEnumerable<KeyValuePair<int, int>>, IEnumerable<KeyValuePair<int, int>>, IEnumerable<KeyValuePair<int, int>>>(
                new[] { new KeyValuePair<int, int>(1, 11), new KeyValuePair<int, int>(2, 21) },
                new[] { new KeyValuePair<int, int>(1, 12), new KeyValuePair<int, int>(2, 22) },
                new[] { new KeyValuePair<int, int>(1, 13), new KeyValuePair<int, int>(2, 23) });
        }

        private DataSet FakeLimitationModuleDataSet(bool hasGroupLimitTable = true, bool hasDomainLimitTable = true, int domainConcurrencyLimit = 0)
        {
            var dataSet = new DataSet();

            var table0 = dataSet.Tables.Add();
            table0.Columns.Add(new DataColumn("GROUP_CONCURRENT_MAX_LIMIT", typeof(int)));
            table0.Columns.Add(new DataColumn("npvr_quota_in_seconds", typeof(int)));
            if (hasGroupLimitTable)
            {
                table0.Rows.Add(2, 3);
            }

            var table1 = dataSet.Tables.Add();
            table1.Columns.Add(new DataColumn("ID", typeof(int)));
            table1.Columns.Add(new DataColumn("NAME", typeof(string)));
            table1.Columns.Add(new DataColumn("CONCURRENT_MAX_LIMIT", typeof(int)));
            table1.Columns.Add(new DataColumn("freq_period_id", typeof(int)));
            table1.Columns.Add(new DataColumn("DEVICE_MAX_LIMIT", typeof(int)));
            table1.Columns.Add(new DataColumn("USER_MAX_LIMIT", typeof(int)));
            table1.Columns.Add(new DataColumn("user_freq_period_id", typeof(int)));
            if (hasDomainLimitTable)
            {
                table1.Rows.Add(1, "Household Limitation Module #1", domainConcurrencyLimit, 4, 5, 6, 7);
            }

            var table2 = dataSet.Tables.Add();
            table2.Columns.Add(new DataColumn("ID", typeof(int)));
            table2.Columns.Add(new DataColumn("NAME", typeof(string)));
            table2.Rows.Add(10, "Device Family 10");
            table2.Rows.Add(20, "Device Family 20");

            var table3 = dataSet.Tables.Add();
            table3.Columns.Add(new DataColumn("device_family_id", typeof(int)));
            table3.Columns.Add(new DataColumn("description", typeof(string)));
            table3.Columns.Add(new DataColumn("value", typeof(int)));
            table3.Rows.Add(10, "COncurrency", 11);
            table3.Rows.Add(10, "QUantity", 12);
            table3.Rows.Add(10, "FRequency", 13);

            return dataSet;
        }

        private DataTable FakeLimitationModuleIdsDataTable(int rowCount)
        {
            if (rowCount < 0)
            {
                return null;
            }

            var table = new DataTable();
            table.Columns.Add(new DataColumn("ID", typeof(int)));
            for (var i = 1; i <= rowCount; i++)
            {
                table.Rows.Add(i);
            }

            return table;
        }

        private DAL.DTO.LimitationsManagerDTO FakeLimitationsManagerDTO1()
        {
            return new DAL.DTO.LimitationsManagerDTO
            {
                domianLimitID = 1,
                Concurrency = 2,
                Frequency = 3,
                Quantity = 4,
                DomainLimitName = "Domain Limit Name",
                UserFrequency = 5,
                nUserLimit = 6,
                lDeviceFamilyLimitations = new List<DAL.DTO.DeviceFamilyLimitationsDTO>
                {
                    new DAL.DTO.DeviceFamilyLimitationsDTO(){ deviceFamily = 1,
                                                              concurrency = -1, 
                                                                Frequency = -1,
                    quantity = -1},
                    new DAL.DTO.DeviceFamilyLimitationsDTO(){ deviceFamily = 2,
                                                              concurrency = 5,
                                                                Frequency = 6,
                    quantity = 7 }
                },
                Description = "description"
            };
        }

        private DAL.DTO.LimitationsManagerDTO FakeLimitationsManagerResponse1()
        {
            return new DAL.DTO.LimitationsManagerDTO
            {
                domianLimitID = 1,
                Concurrency = 2,
                Frequency = 3,
                Quantity = 4,
                DomainLimitName = "Domain Limit Name",
                UserFrequency = 5,
                nUserLimit = 6,
                lDeviceFamilyLimitations = new List<DAL.DTO.DeviceFamilyLimitationsDTO>
                {
                    new DAL.DTO.DeviceFamilyLimitationsDTO(){ deviceFamily = 1,
                                                              concurrency = 2,
                                                                Frequency = -1,
                    quantity = 4,
                     deviceFamilyName = "Device Family 1"},
                    new DAL.DTO.DeviceFamilyLimitationsDTO(){ deviceFamily = 2,
                                                              concurrency = 5,
                                                                Frequency = 6,
                    quantity = 7,
                    deviceFamilyName = "Device Family 2"}
                },
                Description = "description"
            };
        }

        private LimitationsManager FakeLimitationsManager()
        {
            return new LimitationsManager
            {
                domianLimitID = 1,
                Concurrency = 2,
                Frequency = 3,
                Quantity = 4,
                DomainLimitName = "Domain Limit Name",
                UserFrequency = 5,
                nUserLimit = 6,
                lDeviceFamilyLimitations = new List<DeviceFamilyLimitations>
                {
                    new DeviceFamilyLimitations(){ deviceFamily = 1,
                                                              concurrency = -1,
                                                                Frequency = -1,
                    quantity = -1 },
                    new DeviceFamilyLimitations(){ deviceFamily = 2,
                                                              concurrency = 5,
                                                                Frequency = 6,
                    quantity = 7 }
                },
                Description = "description"
            };
        }

        private IEnumerable<int> FakeLimitationModuleIds(int rowCount) 
        {
            if (rowCount <= 0) return null;

            var result = new List<int>();

            for (int i = 1; i <= rowCount; i++) 
            {
                result.Add(i);
            }

            return result;
        }
    }
}
