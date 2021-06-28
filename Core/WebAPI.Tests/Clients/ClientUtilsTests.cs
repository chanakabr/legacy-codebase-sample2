using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ApiObjects.Response;
using AutoMapper;
using AutoMapper.Configuration;
using NUnit.Framework;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Tests.Clients
{
    public class ClientUtilsTests
    {
        private static TReturn ExceptionCallback<TReturn>() => throw new Exception();
        private static TReturn TimeoutExceptionCallback<TReturn>() => throw new TimeoutException();
        private static TReturn CommunicationExceptionCallback<TReturn>() => throw new CommunicationException();
        private static TReturn NotImplementedExceptionCallback<TReturn>() => throw new NotImplementedException();
        private static TReturn BadRequestExceptionCallback<TReturn>()
            => throw new Exception(null, new BadRequestException());

        private static TReturn ExceptionCallback<TReturn>(string source) => throw new Exception();
        private static TReturn TimeoutExceptionCallback<TReturn>(string source) => throw new TimeoutException();
        private static TReturn BadRequestExceptionCallback<TReturn>(string source)
            => throw new Exception(null, new BadRequestException());

        [OneTimeSetUp]
        public void SetUp()
        {
            var cfg = new MapperConfigurationExpression();
            cfg.CreateMap<KalturaOTTObject, string>();
            cfg.CreateMap<string, KalturaOTTObject>();
            Mapper.Initialize(cfg);
        }

        [TestCaseSource(nameof(BoolResponseExceptionTestCaseData))]
        public void GetBoolResponseFromWS_ThrowsException(Func<bool> callback, Type expectedType, StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(expectedType, () => ClientUtils.GetBoolResponseFromWS(callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetBoolResponseFromWS_ReturnsExpectedResult(bool expectedResult)
        {
            bool Callback() => expectedResult;

            var actualResult = ClientUtils.GetBoolResponseFromWS(Callback);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCaseSource(nameof(LongResponseExceptionTestCaseData))]
        public void GetLongResponseFromWS_ThrowsException(Func<long> callback, Type expectedType, StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(expectedType, () => ClientUtils.GetLongResponseFromWS(callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [TestCase(0)]
        [TestCase(100)]
        public void GetLongResponseFromWS_ReturnsExpectedResult(long expectedResult)
        {
            long Callback() => expectedResult;

            var actualResult = ClientUtils.GetLongResponseFromWS(Callback);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCaseSource(nameof(StringResponseExceptionTestCaseData))]
        public void GetStringResponseFromWS_ThrowsException(Func<string> callback, Type expectedType, StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(expectedType, () => ClientUtils.GetStringResponseFromWS(callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("expected")]
        public void GetStringResponseFromWS_ReturnsExpectedResult(string expectedResult)
        {
            string Callback() => expectedResult;

            var actualResult = ClientUtils.GetStringResponseFromWS(Callback);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCaseSource(nameof(ResponseExceptionTestCaseData))]
        public void GetResponseFromWS_ThrowsException(Func<string> callback, Type expectedType, StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(expectedType, () => ClientUtils.GetResponseFromWs(callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [Test]
        public void GetResponseFromWS_ReturnsExpectedResult()
        {
            const string expectedResult = "expectedResult";
            string Callback() => expectedResult;

            var actualResult = ClientUtils.GetResponseFromWs(Callback);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCaseSource(nameof(ResponseStatusWithMappingExceptionTestCaseData))]
        public void GetResponseStatusFromWS_ThrowsException(
            Func<string, Status> callback,
            Type expectedType,
            StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(
                expectedType,
                () => ClientUtils.GetResponseStatusFromWS(callback, new KalturaOTTObject()));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [Test]
        public void GetResponseStatusFromWSWithMapping_DoesNotThrowException()
        {
            Assert.DoesNotThrow(() => ClientUtils.GetResponseStatusFromWS<KalturaOTTObject, string>(
                source => new Status { Code = (int)StatusCode.OK },
                new KalturaOTTObject()));
        }

        [TestCaseSource(nameof(ResponseStatusExceptionTestCaseData))]
        public void GetResponseStatusFromWSReturnsBool_ThrowsException(
            Func<Status> callback,
            Type expectedType,
            StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(expectedType, () => ClientUtils.GetResponseStatusFromWS(callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [Test]
        public void GetResponseStatusFromWS_DoesNotThrowException()
        {
            var result = ClientUtils.GetResponseStatusFromWS(() => new Status { Code = (int)StatusCode.OK });

            Assert.True(result);
        }

        [TestCaseSource(nameof(ResponseListExceptionTestCaseData))]
        public void GetResponseListFromWS_ThrowsException(
            Func<GenericListResponse<string>> callback,
            Type expectedType,
            StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(
                expectedType,
                () => ClientUtils.GetResponseListFromWS<KalturaOTTObject, string>(callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [Test]
        public void GetResponseListFromWS_ReturnsEmptyObjects()
        {
            var result = ClientUtils.GetResponseListFromWS<KalturaOTTObject, string>(
                () => new GenericListResponse<string>(Status.Ok, null));

            Assert.NotNull(result);
            Assert.NotNull(result.Objects);
            Assert.False(result.Objects.Any());
            Assert.AreEqual(0, result.TotalCount);
        }

        [Test]
        public void GetResponseListFromWS_ReturnsMappedObjects()
        {
            var items = new List<string> { string.Empty };
            var result = ClientUtils.GetResponseListFromWS<KalturaOTTObject, string>(
                () => new GenericListResponse<string>(Status.Ok, items));

            Assert.NotNull(result);
            Assert.NotNull(result.Objects);
            Assert.AreEqual(items.Count, result.Objects.Count);
            Assert.AreEqual(items.Count, result.TotalCount);
        }

        [TestCaseSource(nameof(GenericResponseExceptionTestCaseData))]
        public void GetResponseFromWS_ThrowsException(
            Func<GenericResponse<string>> callback,
            Type expectedType,
            StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(
                expectedType,
                () => ClientUtils.GetResponseFromWS<KalturaOTTObject, string>(callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [Test]
        public void GetResponseFromWS_ReturnsEmptyObjects()
        {
            var result = ClientUtils.GetResponseFromWS<KalturaOTTObject, string>(
                () => new GenericResponse<string>(Status.Ok, null));

            Assert.Null(result);
        }

        [Test]
        public void GetResponseFromWS_ReturnsMappedObjects()
        {
            var result = ClientUtils.GetResponseFromWS<KalturaOTTObject, string>(
                () => new GenericResponse<string>(Status.Ok, string.Empty));

            Assert.NotNull(result);
        }

        [TestCaseSource(nameof(GenericResponseWithMappingExceptionTestCaseData))]
        public void GetResponseFromWS_ThrowsException(
            Func<string, GenericResponse<string>> callback,
            Type expectedType,
            StatusCode? expectedStatusCode)
        {
            var actualException = Assert.Throws(
                expectedType,
                () => ClientUtils.GetResponseFromWS(new KalturaOTTObject(), callback));

            ValidateClientException(actualException, expectedStatusCode);
        }

        [Test]
        public void GetResponseFromWSWithMapping_ReturnsEmptyObjects()
        {
            var result = ClientUtils.GetResponseFromWS<KalturaOTTObject, string>(
                new KalturaOTTObject(),
                source => new GenericResponse<string>(Status.Ok, null));

            Assert.Null(result);
        }

        [Test]
        public void GetResponseFromWSWithMapping_ReturnsMappedObject()
        {
            var result = ClientUtils.GetResponseFromWS<KalturaOTTObject, string>(
                new KalturaOTTObject(),
                source => new GenericResponse<string>(Status.Ok, string.Empty));

            Assert.NotNull(result);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Mapper.Reset();
        }

        private static IEnumerable<TestCaseData> BoolResponseExceptionTestCaseData()
        {
            yield return new TestCaseData((Func<bool>)ExceptionCallback<bool>, typeof(ClientException), null);
            yield return new TestCaseData((Func<bool>)BadRequestExceptionCallback<bool>, typeof(BadRequestException), null);
            yield return new TestCaseData(
                (Func<bool>)TimeoutExceptionCallback<bool>,
                typeof(ClientException),
                StatusCode.Timeout);
        }

        private static IEnumerable<TestCaseData> LongResponseExceptionTestCaseData()
        {
            yield return new TestCaseData((Func<long>) ExceptionCallback<long>, typeof(ClientException), null);
            yield return new TestCaseData((Func<long>) BadRequestExceptionCallback<long>, typeof(BadRequestException), null);
            yield return new TestCaseData(
                (Func<long>) CommunicationExceptionCallback<long>,
                typeof(ClientException),
                StatusCode.InternalConnectionIssue);
        }

        private static IEnumerable<TestCaseData> StringResponseExceptionTestCaseData()
        {
            yield return new TestCaseData((Func<string>) ExceptionCallback<string>, typeof(ClientException), null);
            yield return new TestCaseData((Func<string>) BadRequestExceptionCallback<string>, typeof(BadRequestException), null);
            yield return new TestCaseData(
                (Func<string>) TimeoutExceptionCallback<string>,
                typeof(ClientException),
                StatusCode.Timeout);
        }

        private static IEnumerable<TestCaseData> ResponseExceptionTestCaseData()
        {
            yield return new TestCaseData((Func<string>) ExceptionCallback<string>, typeof(ClientException), null);
            yield return new TestCaseData((Func<string>) BadRequestExceptionCallback<string>, typeof(BadRequestException), null);
            yield return new TestCaseData(
                (Func<string>) NotImplementedExceptionCallback<string>,
                typeof(ClientException),
                StatusCode.NotImplemented);
            yield return new TestCaseData((Func<string>)(() => null), typeof(ClientException), null);
        }

        private static IEnumerable<TestCaseData> ResponseStatusWithMappingExceptionTestCaseData()
        {
            yield return new TestCaseData(
                (Func<string, Status>) ExceptionCallback<Status>,
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<string, Status>) BadRequestExceptionCallback<Status>,
                typeof(BadRequestException),
                null);
            yield return new TestCaseData(
                (Func<string, Status>) TimeoutExceptionCallback<Status>,
                typeof(ClientException),
                StatusCode.Timeout);
            yield return new TestCaseData(
                (Func<string, Status>) (source => null),
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<string, Status>) (source => new Status { Code = (int)StatusCode.BadRequest }),
                typeof(ClientException),
                StatusCode.BadRequest);
        }

        private static IEnumerable<TestCaseData> ResponseStatusExceptionTestCaseData()
        {
            yield return new TestCaseData(
                (Func<Status>) ExceptionCallback<Status>,
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<Status>) BadRequestExceptionCallback<Status>,
                typeof(BadRequestException),
                null);
            yield return new TestCaseData(
                (Func<Status>) CommunicationExceptionCallback<Status>,
                typeof(ClientException),
                StatusCode.InternalConnectionIssue);
            yield return new TestCaseData(
                (Func<Status>) (() => null),
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<Status>) (() => new Status { Code = (int)StatusCode.Timeout }),
                typeof(ClientException),
                StatusCode.Timeout);
        }

        private static IEnumerable<TestCaseData> ResponseListExceptionTestCaseData()
        {
            yield return new TestCaseData(
                (Func<GenericListResponse<string>>) ExceptionCallback<GenericListResponse<string>>,
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<GenericListResponse<string>>) BadRequestExceptionCallback<GenericListResponse<string>>,
                typeof(BadRequestException),
                null);
            yield return new TestCaseData(
                (Func<GenericListResponse<string>>) NotImplementedExceptionCallback<GenericListResponse<string>>,
                typeof(ClientException),
                StatusCode.NotImplemented);
            yield return new TestCaseData(
                (Func<GenericListResponse<string>>) (() => null),
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<GenericListResponse<string>>) (() => new GenericListResponse<string>()),
                typeof(ClientException),
                null);
        }

        private static IEnumerable<TestCaseData> GenericResponseExceptionTestCaseData()
        {
            yield return new TestCaseData(
                (Func<GenericResponse<string>>) ExceptionCallback<GenericResponse<string>>,
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<GenericResponse<string>>) BadRequestExceptionCallback<GenericResponse<string>>,
                typeof(BadRequestException),
                null);
            yield return new TestCaseData(
                (Func<GenericResponse<string>>) CommunicationExceptionCallback<GenericResponse<string>>,
                typeof(ClientException),
                StatusCode.InternalConnectionIssue);
            yield return new TestCaseData(
                (Func<GenericResponse<string>>) (() => null),
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<GenericResponse<string>>) (() => new GenericResponse<string>()),
                typeof(ClientException),
                null);
        }

        private static IEnumerable<TestCaseData>GenericResponseWithMappingExceptionTestCaseData()
        {
            yield return new TestCaseData(
                (Func<string, GenericResponse<string>>) ExceptionCallback<GenericResponse<string>>,
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<string, GenericResponse<string>>) BadRequestExceptionCallback<GenericResponse<string>>,
                typeof(BadRequestException),
                null);
            yield return new TestCaseData(
                (Func<string, GenericResponse<string>>) TimeoutExceptionCallback<GenericResponse<string>>,
                typeof(ClientException),
                StatusCode.Timeout);
            yield return new TestCaseData(
                (Func<string, GenericResponse<string>>) (source => null),
                typeof(ClientException),
                null);
            yield return new TestCaseData(
                (Func<string, GenericResponse<string>>) (source => new GenericResponse<string>()),
                typeof(ClientException),
                null);
        }

        private static void ValidateClientException(Exception exception, StatusCode? expectedStatusCode)
        {
            if (!(exception is ClientException clientException))
            {
                return;
            }

            var expectedValue = expectedStatusCode.HasValue
                ? (int) expectedStatusCode.Value
                : (int) StatusCode.Error;
            Assert.AreEqual(expectedValue, clientException.Code);
        }
    }
}