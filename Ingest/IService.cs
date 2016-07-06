using ApiObjects;
using Ingest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Ingest
{
    [ServiceContract]
    public interface IService
    {

        [OperationContract]
        BusinessModuleIngestResponse IngestBusinessModules(string username, string password, string xml);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "InjestAdiData", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestAdiData(IngestRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "InjestTvinciData", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestTvinciData(IngestRequest request);

        [OperationContract]
        [ServiceKnownType(typeof(EpgIngestResponse))]
        [WebInvoke(Method = "POST", UriTemplate = "IngestKalturaEpg", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestKalturaEpg(IngestRequest request);

    }
}
