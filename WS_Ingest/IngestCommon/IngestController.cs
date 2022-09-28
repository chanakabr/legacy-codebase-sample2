using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using Core.Api;
using TVinciShared;
using System.Text;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace Ingest
{
    public class IngestController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string BAD_REQUEST = "Bad request";
        private const string UNKNOWN_INGEST_TYPE = "Unknown ingest type";
        private const string INVALID_CREDENTIALS = "Invalid credentials";
        private const string INGEST_V2_ERROR_MSG = "Please Use AddFromBulkUpload instead,using this route with IngestV2 is deprecated.";
        private const string ERROR_STATUS = "ERROR";
        private const string ERROR_DEPRECATED = "DEPRECATED";
        private const string KALTURA_PROGRAM_ASSET_BULK_UPLOAD_TYPE_NAME = "KalturaProgramAsset";

        // TODO need to delete this mmethod and use Core.Catalog.Controller.IngestController insted (delete this class)
        public static IngestResponse IngestData(IngestRequest request, eIngestType ingestType)
        {
            IngestResponse ingestResponse = new IngestResponse()
            {
                IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() },
                AssetsStatus = new List<IngestAssetStatus>()
            };

            if (request == null)
            {
                log.Error("Request is empty");

                // request is null
                return new IngestResponse()
                {
                    Status = ERROR_STATUS,
                    Description = "Error while deserializing request",
                    IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = BAD_REQUEST }
                };
            }

            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                log.Error("No username or password");

                // no username or password
                return new IngestResponse()
                {
                    Status = ERROR_STATUS,
                    Description = "No username or password",
                    IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = "No username or password" }
                };
            }

            string response = string.Empty;
            try
            {
                // verify user
                int playerID = 0;
                int groupID = PageUtils.GetGroupByUNPass(request.UserName.ToLower().Trim(), request.Password.ToLower().Trim(), ref playerID);

                if (groupID > 0)
                {
                    switch (ingestType)
                    {
                        case eIngestType.Tvinci:
                            {
                                KLogger.LogContextData[Constants.TOPIC] = "VOD Ingest";

                                if (CatalogManager.Instance.DoesGroupUsesTemplates(groupID))
                                {
                                    ingestResponse = IngestManager.HandleMediaIngest(groupID, request.Data);
                                }
                                else if (TvinciImporter.ImporterImpl.DoTheWorkInner(request.Data, groupID, string.Empty, ref response, false, out ingestResponse))
                                {
                                    HandleMediaIngestResponse(response, request.Data, ingestResponse);
                                }

                                break;
                            }
                        case eIngestType.Adi:
                            {
                                ADIFeeder.ADIFeeder adiFeeder = (ADIFeeder.ADIFeeder)ADIFeeder.ADIFeeder.GetInstance(0, 0, request.Data, groupID);
                                bool retVal = adiFeeder.DoTheTask();
                                response = adiFeeder.GetResXml();
                                return HandleMediaIngestResponse(response, request.Data);
                            }
                        case eIngestType.KalturaEpg:
                            {
                                if (OperationContext.Current != null)
                                {
                                    KLogger.LogContextData[Constants.TOPIC] = "EPG Ingest";
                                }

                                var ingestFeatureVersion = Core.GroupManagers.GroupSettingsManager.Instance.GetEpgFeatureVersion(groupID);
                                if (ingestFeatureVersion != EpgFeatureVersion.V1)
                                {
                                    return HandleIngestUsingAddFromBulkUpload(request, ingestResponse, groupID);
                                }


                                bool isSucceeded = false;
                                EpgIngest.Ingest ingest = new EpgIngest.Ingest();

                                Dictionary<string, ImageType> groupRatioNamesToImageTypes = null;
                                if (CatalogManager.Instance.DoesGroupUsesTemplates(groupID))
                                {
                                    groupRatioNamesToImageTypes = Core.Catalog.CatalogManagement.ImageManager.GetImageTypesMapBySystemName(groupID);
                                }
                                isSucceeded = ingest.Initialize(request.Data, groupID, CatalogManager.Instance.DoesGroupUsesTemplates(groupID), groupRatioNamesToImageTypes, out ingestResponse);
                                if (isSucceeded)
                                {
                                    response = ingest.SaveChannelPrograms(ref ingestResponse);
                                }

                                ingestResponse.Status = response;
                                return ingestResponse;
                            }
                        default:
                            return new IngestResponse()
                            {
                                Status = ERROR_STATUS,
                                Description = UNKNOWN_INGEST_TYPE,
                                IngestStatus = new Status() { Code = (int)eResponseStatus.UnknownIngestType, Message = UNKNOWN_INGEST_TYPE }

                            };
                    }
                }
                else
                {
                    // invalid credentials
                    log.Error(string.Format("INVALID_CREDENTIALS - user:{0}, pass:{1}", request.UserName, request.Password));
                    return new IngestResponse()
                    {
                        Status = ERROR_STATUS,
                        Description = "INVALID_CREDENTIALS",
                        IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = INVALID_CREDENTIALS }
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("For input {0}", request.Data), ex);
                return new IngestResponse()
                {
                    Status = ERROR_STATUS,
                    IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
                };
            }

            return ingestResponse;
        }

        private static IngestResponse HandleIngestUsingAddFromBulkUpload(IngestRequest request, IngestResponse ingestResponse, int groupID)
        {
            ingestResponse.AssetsStatus = new List<IngestAssetStatus>();
            var jobData = new BulkUploadIngestJobData();
            var objectData = new BulkUploadEpgAssetData();
            objectData.GroupId = groupID;
            var byteArray = Encoding.UTF8.GetBytes(request.Data);
            using var xmlDataStream = new MemoryStream(byteArray);
            var file = new OTTStreamFile(xmlDataStream, $"{groupID}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid()}");
            var addFromBulkResponse = BulkUploadManager.AddBulkUpload(groupID, 0, KALTURA_PROGRAM_ASSET_BULK_UPLOAD_TYPE_NAME, BulkUploadJobAction.Upsert, jobData, objectData, file);
            if (!addFromBulkResponse.IsOkStatusCode())
            {
                ingestResponse.Status = addFromBulkResponse.Status.Code.ToString();
                ingestResponse.Description = addFromBulkResponse.Status.Message;
                return ingestResponse;
            }

            var isIngestCompleted = false;
            var sw = Stopwatch.StartNew();
            while (!isIngestCompleted || sw.Elapsed.TotalMinutes >= 5)
            {
                Thread.Sleep(500);
                var getBulkUploadResp = BulkUploadManager.GetBulkUpload(groupID, addFromBulkResponse.Object.Id);
                if (!getBulkUploadResp.IsOkStatusCode())
                {
                    ingestResponse.Status = addFromBulkResponse.Status.Code.ToString();
                    ingestResponse.Description = addFromBulkResponse.Status.Message;
                    return ingestResponse;
                }

                var bulkUpload = getBulkUploadResp.Object;
                log.Debug($"bulk uploadId:[{bulkUpload.Id}] status:[{bulkUpload.Status}], results count:[{bulkUpload.Results.Count}], groupId:[{groupID}]");

                if (bulkUpload.IsProcessCompleted)

                    log.Info($"completed bulk uploadId:[{bulkUpload.Id}] status:[{bulkUpload.Status}], results count:[{bulkUpload.Results.Count}], groupId:[{groupID}]");
                {
                    ingestResponse.IngestStatus = Status.Ok;
                    foreach (var res in bulkUpload.Results)
                    {
                        var programResult = (BulkUploadProgramAssetResult)res;
                        var programWarnnings = new List<Status>();
                        if (programResult.Warnings.Any()) { programWarnnings.AddRange(programResult.Warnings); }
                        if (programResult.Errors.Any()) { programWarnnings.AddRange(programResult.Errors); }

                        var ingestAssetStatus = new IngestAssetStatus
                        {
                            EntryID = programResult.ObjectId.ToString(),
                            ExternalAssetId = programResult.ProgramExternalId.ToString(),
                            InternalAssetId = programResult.ChannelId,
                            Warnings = programWarnnings,
                            Status = programResult.Status == BulkUploadResultStatus.Ok ? Status.Ok : Status.Error,
                        };

                    }
                    isIngestCompleted = true;
                }
            }

            return ingestResponse;
        }


        private static string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
        {
            string sVal = "";
            if (theNode != null)
            {
                XmlAttributeCollection theAttr = theNode.Attributes;
                if (theAttr != null)
                {
                    int nCount = theAttr.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sName = theAttr[i].Name.ToLower();
                        if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                        {
                            sVal = theAttr[i].Value.ToString();
                            break;
                        }
                    }
                }
            }
            return sVal;
        }

        private static IngestResponse HandleMediaIngestResponse(string response, string data, IngestResponse ingestResponse = null)
        {
            if (string.IsNullOrEmpty(response))
            {
                log.Warn("For input " + data + " response is empty");
                return new IngestResponse() { Status = ERROR_STATUS };
            }

            if (ingestResponse == null)
            {
                ingestResponse = new IngestResponse() { IngestStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() } };
            }

            try
            {
                if (ingestResponse.IngestStatus.Code == (int)eResponseStatus.OK)
                {
                    string sImporterResponse = "<importer>" + response + "</importer>";

                    XmlDocument theRes = new XmlDocument();
                    theRes.LoadXml(sImporterResponse);

                    XmlNodeList theItems = theRes.SelectNodes("/importer/media");

                    if (theItems != null && theItems.Count > 0)
                    {
                        XmlNode theNode = theItems[0];

                        ingestResponse.AssetID = GetItemParameterVal(ref theNode, "co_guid");
                        ingestResponse.Description = GetItemParameterVal(ref theNode, "message");
                        ingestResponse.Status = GetItemParameterVal(ref theNode, "status");
                        ingestResponse.TvmID = GetItemParameterVal(ref theNode, "tvm_id");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("For input " + data + " response is " + response, ex);
                return new IngestResponse() { Status = ERROR_STATUS };
            }

            return ingestResponse;
        }
    }
}