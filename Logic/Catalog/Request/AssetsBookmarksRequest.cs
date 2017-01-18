using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog.Response;
using KLogMonitor;
using Core.Users;


namespace Core.Catalog.Request
{
    [DataContract]
    public class AssetsBookmarksRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public AssetsBookmarksRequestData Data { get; set; }

        public AssetsBookmarksRequest()
            : base()
        {

        }

        public AssetsBookmarksRequest(AssetsBookmarksRequest request)
            : base (request.m_nPageSize, request.m_nPageIndex, request.m_sUserIP, request.m_nGroupID, request.m_oFilter, request.m_sSignature, request.m_sSignString)
        {
            Data = request.Data;
        }

        public override BaseResponse GetResponse(BaseRequest baseRequest)
        {
            try
            {
                AssetsBookmarksRequest request = null;
                AssetsBookmarksResponse response = new AssetsBookmarksResponse();

                CheckSignature(baseRequest);

                if (baseRequest != null)
                {
                    request = (AssetsBookmarksRequest)baseRequest;
                    if (request != null && request.Data != null)
                    {
                        response.AssetsBookmarks = new List<AssetBookmarks>();
                        List<int> users = null;
                        List<int> defaultUsers = null;
                        bool isDefaultUser = false;                        
                        int userDomainID = 0;
                        int userID;
                        DomainResponse domainResponse = null;
                        if (CatalogLogic.IsUserValid(request.m_sSiteGuid, request.m_nGroupID, ref userDomainID) && int.TryParse(request.m_sSiteGuid, out userID))
                        {
                            if(userDomainID == request.domainId)
                            {
                                domainResponse = CatalogLogic.GetDomain(request.domainId, request.m_nGroupID);
                                if (domainResponse != null && domainResponse.Status != null &&  domainResponse.Status.Code == (int)eResponseStatus.OK)
                                { 
                                    // Get users list, default users list and check if user is in default users list
                                    GetUsersInfo(userID, domainResponse.Domain, ref users, ref defaultUsers, ref isDefaultUser);
                                    List<int> usersToGet = new List<int>();
                                    usersToGet.AddRange(users);
                                    usersToGet.AddRange(defaultUsers);
                                    Dictionary<string, User> usersDictionary = CatalogLogic.GetUsers(request.m_nGroupID, usersToGet);
                                    foreach (AssetBookmarkRequest asset in request.Data.Assets)
                                    {
                                        AssetBookmarks assetPositionResponseInfo = null;

                                        if (asset.AssetType != eAssetTypes.UNKNOWN)
                                        {
                                            assetPositionResponseInfo = CatalogLogic.GetAssetLastPosition(asset.AssetID, asset.AssetType, userID, isDefaultUser, users, defaultUsers, usersDictionary);
                                        }
                                        else
                                        {
                                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidAssetType, "Invalid Asset Type");
                                            return response;
                                        }
                                        if (assetPositionResponseInfo != null)
                                        {
                                            response.AssetsBookmarks.Add(assetPositionResponseInfo);
                                        }
                                    }
                                    response.m_nTotalItems = response.AssetsBookmarks.Count();
                                }
                                else
                                {
                                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Invalid Parameters In Request");
                                    return response;
                                }                            
                            }
                            else
                            {
                                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UserNotExistsInDomain, eResponseStatus.UserNotExistsInDomain.ToString());
                                return response;
                            }
                        }
                        else
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidUser, eResponseStatus.InvalidUser.ToString());
                            return response;
                        }
                    }
                    else
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Request Is Null");
                        return response;
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Request Is Null");
                    return response;
                }

                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());                
                return response;
            }
            catch (Exception ex)
            {
                log.Error("AssetsPositionRequest.GetResponse - " + string.Format("Failed ex={0}, userID={1}, domainID={2}", ex.Message, baseRequest.m_sSiteGuid, baseRequest.domainId), ex);  
                throw ex;
            }
        }

        private void GetUsersInfo(int userID, Domain domain, ref List<int> users, ref List<int> defaultUsers, ref bool isDefaultUser)
        {
            users = new List<int>();
            defaultUsers = new List<int>();            
            if (domain.m_DefaultUsersIDs != null && domain.m_DefaultUsersIDs.Count > 0)
            {
                defaultUsers = domain.m_DefaultUsersIDs;
                isDefaultUser = defaultUsers.Contains(userID);
            }

            if (domain.m_UsersIDs != null && domain.m_UsersIDs.Count > 0)
            {
                if (isDefaultUser)
                {
                    users = domain.m_UsersIDs;
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