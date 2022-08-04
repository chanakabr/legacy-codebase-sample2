using Amazon.S3;
using Amazon.S3.Transfer;
using ApiLogic.Api.Managers.Handlers;
using ApiLogic.Catalog;
using ApiObjects.Response;
using AutoFixture;
using Phx.Lib.Appconfig.Types;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.IO;

namespace ApiLogic.Tests
{
    [TestFixture]
    public class FileManagerTests
    {
        const string KALTURA = "Kaltura";
        static string tempFilePath;

        public static IEnumerable ValidateFileContentCases()
        {
            // Creating a temp file for the test!!!!
            tempFilePath = Path.GetTempFileName();
            // ------------------------------------------------------------------------------------------------------------------------

            var validFile = new OTTFile(tempFilePath, Path.GetFileName(tempFilePath), false);

            yield return new TestCaseData(new SaveTestCase(eResponseStatus.OK, null, validFile)).SetName("GoodTestWillPass");

            var errorTextNotKalturaObj = "File's objectType value must be type of KalturaOTTObject. objectType.Name=TestFileManager";
            yield return new TestCaseData(new SaveTestCase(eResponseStatus.InvalidFileType, errorTextNotKalturaObj, validFile, objectTypeName: "TestFileManager")).SetName("NoKalturaInObjectTypeFailed");

            var errorTextObjectTypeEmpty = "File's objectType name cannot be empty";
            yield return new TestCaseData(new SaveTestCase(eResponseStatus.InvalidFileType, errorTextObjectTypeEmpty, validFile, string.Empty)).SetName("ObjectTypeNameEmptyFailed");

            var errorTextEmptyAfterKaltura = $"File's objectType.Name minimum length is {KALTURA.Length + 1}. objectType.Name=Kaltura";
            yield return new TestCaseData(new SaveTestCase(eResponseStatus.InvalidFileType, errorTextEmptyAfterKaltura, validFile, objectTypeName: KALTURA)).SetName("OnlyKalturaInObjectTypeNameFailed");

            var errorTextFileIsNull = "OTTBasicFile is null and can't be used";
            yield return new TestCaseData(new SaveTestCase(eResponseStatus.Error, errorTextFileIsNull, null)).SetName("FileIsNotGivenFailed");

            var errorFile = new OTTFile("Kaltura.xml", "Kaltura.xml", false);
            var errorTextFileDoesntExist = "file:Kaltura.xml does not exists.";
            yield return new TestCaseData(new SaveTestCase(eResponseStatus.FileDoesNotExists, errorTextFileDoesntExist, errorFile)).SetName("FileDoesNotExistFailed");
            
            var prefix = $"PREFIX/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/1483/";
            yield return new TestCaseData(new SaveTestCase(eResponseStatus.OK, null, validFile, prefix: prefix)).SetName("UploadingWithGivenPrefixSuccess");
        }

        [TestCaseSource(nameof(ValidateFileContentCases))]
        public void CheckValidateFileContent(SaveTestCase saveTestCase)
        {
            Mock<IAmazonS3> S3 = new Mock<IAmazonS3>();
            Mock<ITransferUtility> transferUtility = new Mock<ITransferUtility>();

            transferUtility.Setup(x => x.Upload(saveTestCase.FileTransferUtilityRequest));

            S3FileHandler handler = new S3FileHandler(saveTestCase.Config, true, () => S3.Object, S3 => transferUtility.Object);
            handler.NumberOfRetries = 1; // Because in the S3 configuration it configures it as 0
            var manager = new FileManager(handler);

            var response = manager.SaveFile(saveTestCase.TaskId, saveTestCase.FileToUpload, saveTestCase.ObjectTypeName, saveTestCase.Prefix);

            Assert.AreEqual(saveTestCase.ExpectedResponse.ToStringStatus(), response.ToStringStatus());
        }

        public class SaveTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            public string TaskId;
            public GenericResponse<string> ExpectedResponse { get; private set; }
            public TransferUtilityUploadRequest FileTransferUtilityRequest { get; set; }
            public OTTBasicFile FileToUpload { get; set; }
            public S3Configuration Config { get; set; }
            public string ObjectTypeName { get; set; }
            public string Prefix { get; set; }

            public SaveTestCase(eResponseStatus status, string errorString, OTTBasicFile file, string objectTypeName = "KalturaTestFileManager", string prefix = "")
            {
                ExpectedResponse = new GenericResponse<string>(status, errorString);
                TaskId = fixture.Create<string>();
                Config = new S3DataLakeConfiguration();

                if (file != null)
                {
                    FileTransferUtilityRequest = file.GetTransferUtilityUploadRequest();
                    FileTransferUtilityRequest.BucketName = Config.BucketName.Value;
                    FileTransferUtilityRequest.Key = $"{prefix}{objectTypeName}{Path.GetExtension(tempFilePath)}";
                }

                FileToUpload = file;
                ObjectTypeName = objectTypeName;
                Prefix = prefix;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => File.Delete(tempFilePath);
    }
}