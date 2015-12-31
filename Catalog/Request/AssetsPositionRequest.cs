using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using ApiObjects.Response;
using Catalog.Response;
using KLogMonitor;


namespace Catalog.Request
{
    [DataContract]
    public class AssetsPositionRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public AssetsPositionRequestData Data { get; set; }

        public AssetsPositionRequest()
            : base()
        {

        }

        public AssetsPositionRequest(AssetsPositionRequest request)
            : base (request.m_nPageSize, request.m_nPageIndex, request.m_sUserIP, request.m_nGroupID, request.m_oFilter, request.m_sSignature, request.m_sSignString)
        {
            Data = request.Data;
        }

        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            try
            {
                AssetsPositionRequest request = null;
                AssetsPositionResponse response = new AssetsPositionResponse();

                CheckSignature(baseRequest);

                if (baseRequest != null)
                {
                    request = (AssetsPositionRequest)baseRequest;
                    if (request != null && request.Data != null)
                    {
                        response.AssetsPositions = new List<AssetPositionResponseInfo>();
                        foreach (AssetPositionRequestInfo asset in request.Data.Assets)
                        {
                            AssetPositionResponseInfo assetPositionResponseInfo = null;
                            int userID;
                            if (asset.AssetType != eAssetTypes.UNKNOWN && int.TryParse(request.m_sSiteGuid, out userID) )
                            {                    
                                assetPositionResponseInfo = ProccessAssetLastPositionRequest(userID, request.domainId, request.m_nGroupID, asset.AssetType, asset.AssetID, asset.UserType);
                            }
                            else
                            {
                                response.Status = new Status((int)eResponseStatus.InvalidAssetType, "Invalid Asset Type");
                                return response;  
                            }
                                if(assetPositionResponseInfo != null)
                                {
                                    response.AssetsPositions.Add(assetPositionResponseInfo);
                                }
                        }
                    }
                    else
                    {
                        response.Status = new Status((int)eResponseStatus.Error, "Request Is Null");
                    }
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.Error, "Request Is Null");
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());                
                return response;
            }
            catch (Exception ex)
            {
                log.Error("AssetsPositionRequest.GetResponse", ex);
                throw ex;
            }
        }

        private AssetPositionResponseInfo ProccessAssetLastPositionRequest(int userID, int domainID, int groupID, eAssetTypes assetType, string assetID, eUserType assetUserType)
        {
            AssetPositionResponseInfo assetPositionResponseInfo = null;
            try
            {                
                if (assetID == "0" || userID == 0 || domainID == 0 || groupID == 0)
                {
                    return assetPositionResponseInfo;
                }

                bool isDefaultUser = false; // set false for default , if this user_id return from domains as DeafultUsers change it to true
                List<int> defaultUsers = new List<int>();
                List<int> users = new List<int>();

                if (assetUserType == eUserType.HOUSEHOLD)
                {
                    string sWSUsername = string.Empty;
                    string sWSPassword = string.Empty;
                    string sWSUrl = string.Empty;
                    WS_Domains.Domain domainsResp = null;                    

                    //get username + password from wsCache
                    Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupID, ApiObjects.eWSModules.DOMAINS);
                    if (oCredentials != null)
                    {
                        sWSUsername = oCredentials.m_sUsername;
                        sWSPassword = oCredentials.m_sPassword;
                    }

                    if (sWSUsername.Length == 0 || sWSPassword.Length == 0)
                    {
                        throw new Exception(string.Format("No WS_Domains login parameters were extracted from DB. user={0}, groupid={1}", userID, groupID));
                    }

                    // get domain info - to have the users list in domain + default users in domain
                    using (WS_Domains.module domains = new WS_Domains.module())
                    {
                        sWSUrl = Utils.GetWSURL("ws_domains");
                        if (sWSUrl.Length > 0)
                            domains.Url = sWSUrl;
                        var domainRes = domains.GetDomainInfo(sWSUsername, sWSPassword, domainID);
                        if (domainRes != null)
                        {
                            domainsResp = domainRes.Domain;
                        }
                        else
                        {
                            return assetPositionResponseInfo;
                        }
                    }

                    if (domainsResp != null)
                    {
                        users = domainsResp.m_UsersIDs.ToList();
                        defaultUsers = domainsResp.m_DefaultUsersIDs.ToList();
                        if (defaultUsers != null && defaultUsers.Count > 0)
                        {
                            isDefaultUser = defaultUsers.Contains(userID);
                        }
                    }                    
                }

                assetPositionResponseInfo = Catalog.GetAssetLastPosition(assetID, assetType, userID, isDefaultUser, users, defaultUsers, domainId);
            }
            catch (Exception ex)
            {
                log.Error("ProccessMediaLastPositionRequest - " + string.Format("Failed ex={0}, userID={1}, domainID={2}, groupdID={3}, assetID={4}, assetUserType={5}",
                          ex.Message, userID, domainID, groupID, assetID, assetUserType), ex);     
            }
            
            return assetPositionResponseInfo;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            if (Data != null)
            {
                sb.Append(Data.ToString());
            }
            else
            {
                sb.Append(" Data is null");
            }

            return sb.ToString();
        }

    }
}