using ApiObjects;
using Core.Catalog;
using Ingest.Models;
using System.ServiceModel;

namespace Ingest
{
    [ServiceContract]
    public interface IService
    {

        [OperationContract]
        BusinessModuleIngestResponse IngestBusinessModules(string username, string password, string xml);

        [OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "InjestAdiData", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestAdiData(IngestRequest request);

        [OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "IngestTvinciData", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestTvinciData(IngestRequest request);

        [OperationContract]
        [ServiceKnownType(typeof(EpgIngestResponse))]
        //[WebInvoke(Method = "POST", UriTemplate = "IngestKalturaEpg", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml)]
        IngestResponse IngestKalturaEpg(IngestRequest request);

    }
}
