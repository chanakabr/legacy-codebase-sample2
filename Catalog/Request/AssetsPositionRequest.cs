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

        [DataMember]
        public eUserType UserType;

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
                        response.AssetsPositions = new List<AssetPositionsInfo>();
                        List<int> users = null;
                        List<int> defaultUsers = null;
                        bool isDefaultUser = false;                        
                        int userDomainID = 0;
                        int userID;
                        WS_Domains.DomainResponse domainResponse = null;
                        if (Catalog.IsUserValid(request.m_sSiteGuid, request.m_nGroupID, ref userDomainID) && int.TryParse(request.m_sSiteGuid, out userID))
                        {
                            if(userDomainID == request.domainId)
                            {
                                domainResponse = Catalog.GetDomain(request.domainId, request.m_nGroupID);
                                if (domainResponse != null && domainResponse.Status != null &&  domainResponse.Status.Code == (int)eResponseStatus.OK)
                                { 
                                    // Get users list, default users list and check if user is in default users list
                                    GetUsersInfo(userID, domainResponse.Domain, UserType, ref users, ref defaultUsers, ref isDefaultUser);

                                    foreach (AssetPositionRequestInfo asset in request.Data.Assets)
                                    {
                                        AssetPositionsInfo assetPositionResponseInfo = null;

                                        if (asset.AssetType != eAssetTypes.UNKNOWN)
                                        {
                                            assetPositionResponseInfo = Catalog.GetAssetLastPosition(asset.AssetID, asset.AssetType, userID, isDefaultUser, users, defaultUsers);
                                        }
                                        else
                                        {
                                            response.Status = new Status((int)eResponseStatus.InvalidAssetType, "Invalid Asset Type");
                                            return response;
                                        }
                                        if (assetPositionResponseInfo != null)
                                        {
                                            response.AssetsPositions.Add(assetPositionResponseInfo);
                                        }
                                    }                                
                                }
                                else
                                {
                                    response.Status = new Status((int)eResponseStatus.Error, "Invalid Parameters In Request");
                                    return response;
                                }                            
                            }
                            else
                            {
                                response.Status = new Status((int)eResponseStatus.UserNotExistsInDomain, eResponseStatus.UserNotExistsInDomain.ToString());
                                return response;
                            }
                        }
                        else
                        {
                            response.Status = new Status((int)eResponseStatus.InvalidUser, eResponseStatus.InvalidUser.ToString());
                            return response;
                        }
                    }
                    else
                    {
                        response.Status = new Status((int)eResponseStatus.Error, "Request Is Null");
                        return response;
                    }
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.Error, "Request Is Null");
                    return response;
                }

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());                
                return response;
            }
            catch (Exception ex)
            {
                log.Error("AssetsPositionRequest.GetResponse - " + string.Format("Failed ex={0}, userID={1}, domainID={2}", ex.Message, baseRequest.m_sSiteGuid, baseRequest.domainId), ex);  
                throw ex;
            }
        }

        private void GetUsersInfo(int userID, WS_Domains.Domain domain,  eUserType assetUserType, ref List<int> users, ref List<int> defaultUsers, ref bool isDefaultUser)
        {
            users = new List<int>();
            defaultUsers = new List<int>();
            if (domain.m_DefaultUsersIDs != null && domain.m_DefaultUsersIDs.Length > 0)
            {
                defaultUsers = domain.m_DefaultUsersIDs.ToList();
                isDefaultUser = defaultUsers.Contains(userID);
            }
            if (domain.m_UsersIDs != null && domain.m_UsersIDs.Length > 0)
            {
                users = domain.m_UsersIDs.ToList();
            }
            
            // if userType is PERSONAL we only want the specific user position
            if (assetUserType == eUserType.PERSONAL)
            {
                users.Clear();
                defaultUsers.Clear();
                if (isDefaultUser)
                {
                    defaultUsers.Add(userID);
                }
                else
                {
                    users.Add(userID);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Concat("UserType :", UserType));
            if (Data != null)
            {
                sb.Append(string.Concat("Data :", Data.ToString()));                      
            }
            else
            {
                sb.Append(" Data is null");
            }

            return sb.ToString();
        }

    }
}