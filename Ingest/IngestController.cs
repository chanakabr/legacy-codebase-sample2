using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using TVinciShared;

namespace Ingest
{
    public class IngestController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string BAD_REQUEST = "Bad request";
        private const string UNKNOWN_INGEST_TYPE = "Unknown ingest type";
        private const string INVALID_CREDENTIALS = "Invalid credentials";

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
                    Status = "ERROR",
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
                    Status = "ERROR",
                    Description = "No username or password",
                    IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = "No username or password" }
                };
            }

            log.Debug("Start request - input is " + request.Data);

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
                                OperationContext.Current.IncomingMessageProperties[Constants.TOPIC] = "VOD Ingest";

                                if (TvinciImporter.ImporterImpl.DoTheWorkInner(request.Data, groupID, string.Empty, ref response, false, out ingestResponse))
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
                                OperationContext.Current.IncomingMessageProperties[Constants.TOPIC] = "EPG Ingest";
                                
                                bool isSucceeded = false;
                                EpgIngest.Ingest ingest = new EpgIngest.Ingest();
                                
                                isSucceeded = ingest.Initialize(request.Data, groupID, ingestResponse);
                                if (isSucceeded)
                                {
                                    response = ingest.SaveChannelPrograms();
                                }
                                return new EpgIngestResponse()
                                {
                                    Status = response
                                };
                            }
                        default:
                            return new IngestResponse()
                            {
                                Status = "ERROR",
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
                        Status = "ERROR",
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
                    Status = "ERROR",
                    IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
                };
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
                return new IngestResponse() { Status = "ERROR" };
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
                return new IngestResponse() { Status = "ERROR" };
            }

            return ingestResponse;
        }
    }
}