using ApiLogic.Pricing.Handlers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Pricing.Dto;
using ApiObjects.Response;
using AutoFixture;
using Core.Pricing;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ApiLogic.Tests.Pricing.Handlers
{
    [TestFixture]
    public class CollectionManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deleteCollection, bool isCollectionExists)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<ICollectionRepository>();

            repositoryMock.Setup(x => x.DeleteCollection(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deleteCollection);

            repositoryMock.Setup(x => x.IsCollectionExists(It.IsAny<int>(), It.IsAny<long>()))
                                    .Returns(isCollectionExists);

            CollectionManager manager = new CollectionManager(repositoryMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), It.IsAny<long>());

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable DeleteCases()
        {
            yield return new TestCaseData(eResponseStatus.OK, true, true).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.CollectionNotExist, false, false).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(eResponseStatus.Error, false, true).SetName("CheckDeleteFailed");
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(eResponseStatus expectedCode, long id)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<ICollectionRepository>();

            repositoryMock.Setup(x => x.Insert_Collection(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                 It.IsAny<string>(), It.IsAny<long>(), It.IsAny<LanguageContainer[]>(), It.IsAny<LanguageContainer[]>(), It.IsAny<List<long>>(), It.IsAny<List<SubscriptionCouponGroupDTO>>(),
                 It.IsAny<List<KeyValuePair<VerificationPaymentGateway, string>>>())).Returns(id);

            CollectionManager manager = new CollectionManager(repositoryMock.Object);

            Collection collection = fixture.Create<Collection>();

            BundleCodeContainer bundleCodeContainer = new BundleCodeContainer();
            bundleCodeContainer.Initialize("1", "name");

            collection.m_sCodes = new BundleCodeContainer[1] { bundleCodeContainer };

            var response = manager.Add(fixture.Create<ContextData>(), collection);

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
        }
    }
}
