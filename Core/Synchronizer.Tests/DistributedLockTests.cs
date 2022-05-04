using System.Collections.Generic;
using System.Linq;
using CouchbaseManager;
using FeatureFlag;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ott.Lib.FeatureToggle;

namespace Synchronizer.Tests
{
    [TestFixture]
    public class DistributedLockTests
    {
        private const string StrictUnlockDisabledFeatureFlagTest = "distributedlock.strict-unlock-disabled";
        private MockRepository _mockRepository;
        private Mock<ICouchbaseManager> _keyValueStoreMock;
        private Mock<IPhoenixFeatureFlag> _phoenixFeatureFlagMock;
        private int _groupId = 1483;
        private int _userId = 10200;
        private int _numOfRetries = 1;
        private int _retryIntervalMs = 100;
        private uint _ttlSeconds = 100;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _keyValueStoreMock = _mockRepository.Create<ICouchbaseManager>();
            _phoenixFeatureFlagMock = _mockRepository.Create<IPhoenixFeatureFlag>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Lock_ShouldProceedSuccessfullyIfNoKeysToLock()
        {
            var lockContext = new LockContext(_groupId, _userId);
            var expectedKalturaUserFeatureToggleUser = new KalturaFeatureToggleUser(_userId, _groupId);
            var testLockInitiator = "test-lock-initiator";
            var globalLockKeyNameInitiator = "global-test-lock-initiator";
            var globalLockInitiator = testLockInitiator + "_";
            var globalLockDocument = new LockObjectDocument
            {
                LockInitiator = globalLockInitiator
            };
            var globalLockDocumentKey = "OTT_DISTRIBUTED_GLOBAL_LOCK_1483" + globalLockKeyNameInitiator;
            ulong version = 235296;
            var status = eResultStatus.SUCCESS;

            _keyValueStoreMock.Setup(x => x.Add(globalLockDocumentKey, It.IsAny<LockObjectDocument>(), _ttlSeconds, false, true))
                .Returns(true)
                .Callback(new InvocationAction(i =>
                {
                    globalLockDocument.Should().BeEquivalentTo(i.Arguments[1]);
                }));
            _phoenixFeatureFlagMock.Setup(x => x.IsStrictUnlockDisabled()).Returns(false);
            _keyValueStoreMock.Setup(x => x.GetWithVersion<LockObjectDocument>(globalLockDocumentKey, out version, out status, false)).Returns(globalLockDocument);
            _keyValueStoreMock.Setup(x => x.Remove(globalLockDocumentKey, version)).Returns(true);
            
            var distributedLock = new DistributedLock(lockContext, _keyValueStoreMock.Object, _phoenixFeatureFlagMock.Object, new Dictionary<string, string>());

            var result = distributedLock.Lock(Enumerable.Empty<string>(), _numOfRetries, _retryIntervalMs, (int)_ttlSeconds, testLockInitiator, globalLockKeyNameInitiator);

            result.Should().BeTrue();
        }
        
        [Test]
        public void Lock_ShouldProceedSuccessfullyIfSeveralKeysToLock()
        {
            var lockContext = new LockContext(_groupId, _userId);
            var expectedKalturaUserFeatureToggleUser = new KalturaFeatureToggleUser(_userId, _groupId);
            var testLockInitiator = "test-lock-initiator";
            var globalLockKeyNameInitiator = "global-test-lock-initiator";
            var globalLockInitiator = testLockInitiator + "_epg_day_lock1";
            var globalLockDocument = new LockObjectDocument
            {
                LockInitiator = globalLockInitiator
            };
            var keyLockDocument = new LockObjectDocument
            {
                LockInitiator = testLockInitiator
            };
            var globalLockDocumentKey = "OTT_DISTRIBUTED_GLOBAL_LOCK_1483" + globalLockKeyNameInitiator;
            ulong version = 235296;
            var status = eResultStatus.SUCCESS;
            var keys = new[] {"epg_day_lock1"};

            _keyValueStoreMock.Setup(x => x.Add(globalLockDocumentKey, It.IsAny<LockObjectDocument>(), _ttlSeconds, false, true))
                .Returns(true)
                .Callback(new InvocationAction(i =>
                {
                    globalLockDocument.Should().BeEquivalentTo(i.Arguments[1]);
                }));
            _keyValueStoreMock.Setup(x => x.Add(keys[0], It.IsAny<LockObjectDocument>(), _ttlSeconds, false, true))
                .Returns(true)
                .Callback(new InvocationAction(i =>
                {
                    keyLockDocument.Should().BeEquivalentTo(i.Arguments[1]);
                }));
            _phoenixFeatureFlagMock.Setup(x => x.IsStrictUnlockDisabled()).Returns(false);
            _keyValueStoreMock.Setup(x => x.GetWithVersion<LockObjectDocument>(globalLockDocumentKey, out version, out status, false)).Returns(globalLockDocument);
            _keyValueStoreMock.Setup(x => x.Remove(globalLockDocumentKey, version)).Returns(true);
            
            var distributedLock = new DistributedLock(lockContext, _keyValueStoreMock.Object, _phoenixFeatureFlagMock.Object, new Dictionary<string, string>());

            var result = distributedLock.Lock(keys, _numOfRetries, _retryIntervalMs, (int)_ttlSeconds, testLockInitiator, globalLockKeyNameInitiator);

            result.Should().BeTrue();
        }
        
        [Test]
        public void Unlock_ShouldProceedSuccessfullyWithStrictFunctionality()
        {
            var lockContext = new LockContext(_groupId, _userId);
            var expectedKalturaUserFeatureToggleUser = new KalturaFeatureToggleUser(_userId, _groupId);
            var lockKey = "epg_day_lock1";
            var testLockInitiator = "test-lock-initiator";
            ulong version = 235296;
            var status = eResultStatus.SUCCESS;
            var lockDocument = new LockObjectDocument
            {
                LockInitiator = testLockInitiator
            };
            var expectedResult = new Dictionary<string, bool>
            {
                {"epg_day_lock1", true}
            };
            
            _phoenixFeatureFlagMock.Setup(x => x.IsStrictUnlockDisabled()).Returns(false);
            _keyValueStoreMock.Setup(x => x.GetWithVersion<LockObjectDocument>(lockKey, out version, out status, false)).Returns(lockDocument);
            _keyValueStoreMock.Setup(x => x.Remove(lockKey, version)).Returns(true);
            
            var distributedLock = new DistributedLock(lockContext, _keyValueStoreMock.Object, _phoenixFeatureFlagMock.Object, new Dictionary<string, string>());
        
            var result = distributedLock.Unlock(new []{lockKey}, testLockInitiator);
        
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void Unlock_ShouldProceedSuccessfullyWithStrictFunctionalityIfNoKeyFound()
        {
            var lockContext = new LockContext(_groupId, _userId);
            var expectedKalturaUserFeatureToggleUser = new KalturaFeatureToggleUser(_userId, _groupId);
            var lockKey = "epg_day_lock1";
            var testLockInitiator = "test-lock-initiator";
            ulong version = 235296;
            var status = eResultStatus.KEY_NOT_EXIST;
            var lockDocument = new LockObjectDocument
            {
                LockInitiator = testLockInitiator
            };
            var expectedResult = new Dictionary<string, bool>
            {
                {"epg_day_lock1", true}
            };

            _phoenixFeatureFlagMock.Setup(x => x.IsStrictUnlockDisabled()).Returns(false);
            _keyValueStoreMock.Setup(x => x.GetWithVersion<LockObjectDocument>(lockKey, out version, out status, false)).Returns(lockDocument);

            var distributedLock = new DistributedLock(lockContext, _keyValueStoreMock.Object, _phoenixFeatureFlagMock.Object, new Dictionary<string, string>());
        
            var result = distributedLock.Unlock(new []{lockKey}, testLockInitiator);
        
            _keyValueStoreMock.Verify(x => x.Remove(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void Unlock_ShouldFailWithStrictFunctionalityInCaseOfError()
        {
            var lockContext = new LockContext(_groupId, _userId);
            var expectedKalturaUserFeatureToggleUser = new KalturaFeatureToggleUser(_userId, _groupId);
            var lockKey = "epg_day_lock1";
            var testLockInitiator = "test-lock-initiator";
            ulong version = 235296;
            var status = eResultStatus.ERROR;
            var lockDocument = new LockObjectDocument
            {
                LockInitiator = testLockInitiator
            };
            var expectedResult = new Dictionary<string, bool>
            {
                {"epg_day_lock1", false}
            };

            _phoenixFeatureFlagMock.Setup(x => x.IsStrictUnlockDisabled()).Returns(false);
            _keyValueStoreMock.Setup(x => x.GetWithVersion<LockObjectDocument>(lockKey, out version, out status, false)).Returns(lockDocument);

            var distributedLock = new DistributedLock(lockContext, _keyValueStoreMock.Object, _phoenixFeatureFlagMock.Object, new Dictionary<string, string>());
        
            var result = distributedLock.Unlock(new []{lockKey}, testLockInitiator);
        
            _keyValueStoreMock.Verify(x => x.Remove(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void Unlock_ShouldFailWithStrictFunctionality()
        {
            var lockContext = new LockContext(_groupId, _userId);
            var expectedKalturaUserFeatureToggleUser = new KalturaFeatureToggleUser(_userId, _groupId);
            var lockKey = "epg_day_lock1";
            var testLockInitiator = "test-lock-initiator";
            ulong version = 235296;
            var status = eResultStatus.SUCCESS;
            var lockDocument = new LockObjectDocument
            {
                LockInitiator = testLockInitiator + "asldkjf"
            };
            var expectedResult = new Dictionary<string, bool>
            {
                {"epg_day_lock1", false}
            };

            _phoenixFeatureFlagMock.Setup(x => x.IsStrictUnlockDisabled()).Returns(false);
            _keyValueStoreMock.Setup(x => x.GetWithVersion<LockObjectDocument>(lockKey, out version, out status, false)).Returns(lockDocument);

            var distributedLock = new DistributedLock(lockContext, _keyValueStoreMock.Object, _phoenixFeatureFlagMock.Object, new Dictionary<string, string>());
        
            var result = distributedLock.Unlock(new []{lockKey}, testLockInitiator);
        
            _keyValueStoreMock.Verify(x => x.Remove(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void Unlock_ShouldProceedSuccessfullyWithoutStrictFunctionality()
        {
            var lockContext = new LockContext(_groupId, _userId);
            var expectedKalturaUserFeatureToggleUser = new KalturaFeatureToggleUser(_userId, _groupId);
            var lockKey = "epg_day_lock1";
            var testLockInitiator = "test-lock-initiator";
            ulong version = 235296;
            var status = eResultStatus.SUCCESS;
            var lockDocument = new LockObjectDocument
            {
                LockInitiator = testLockInitiator + "asldkjf"
            };
            var expectedResult = new Dictionary<string, bool>
            {
                {"epg_day_lock1", true}
            };

            _phoenixFeatureFlagMock.Setup(x => x.IsStrictUnlockDisabled()).Returns(true);
            _keyValueStoreMock.Setup(x => x.GetWithVersion<LockObjectDocument>(lockKey, out version, out status, false)).Returns(lockDocument);
            _keyValueStoreMock.Setup(x => x.Remove(lockKey, version)).Returns(true);

            var distributedLock = new DistributedLock(lockContext, _keyValueStoreMock.Object, _phoenixFeatureFlagMock.Object, new Dictionary<string, string>());
        
            var result = distributedLock.Unlock(new []{lockKey}, testLockInitiator);
            
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
