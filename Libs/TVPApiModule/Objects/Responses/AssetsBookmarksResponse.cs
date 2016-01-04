using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using Newtonsoft.Json;
using TVPApiModule.Objects.Requests;

namespace TVPApiModule.Objects.Responses
{
    /// <summary>
    /// Summary description for AssetsBookmarksResponse
    /// </summary>
    public class AssetsBookmarksResponse
    {
        [JsonProperty(PropertyName = "AssetsBookmarks")]
        public List<AssetBookmarksResponse> AssetsBookmarks { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public Status Status { get; set; }

        [JsonProperty(PropertyName = "TotalItems")]
        public int TotalItems { get; set; }

        public AssetsBookmarksResponse(List<AssetBookmarks> assetsBookmarks, int statusCode, string statusMessage, int totalItems)
        {
            AssetsBookmarks = new List<AssetBookmarksResponse>();
            if (assetsBookmarks != null)
            {
                foreach (AssetBookmarks assetBookmarks in assetsBookmarks)
                {
                    AssetBookmarksResponse assetBookmarksResponse = new AssetBookmarksResponse(assetBookmarks);
                    AssetsBookmarks.Add(assetBookmarksResponse);
                }
            }
            Status = new Status(statusCode, statusMessage);
            TotalItems = totalItems;
        }
    }

    /// <summary>
    /// Summary description for AssetBookmarkResponse
    /// </summary>
    public class AssetBookmarksResponse
    {
        [JsonProperty(PropertyName = "Bookmarks")]
        public List<AssetBookmarkResponse> Bookmarks { get; set; }

        [JsonProperty(PropertyName = "AssetID")]
        public string AssetID { get; set; }

        [JsonProperty(PropertyName = "AssetType")]
        public AssetTypes AssetType { get; set; }

        public AssetBookmarksResponse(AssetBookmarks assetBookmarks)
        {
            Bookmarks = new List<AssetBookmarkResponse>();
            if (assetBookmarks != null)
            {
                AssetID = assetBookmarks.AssetID;
                switch (assetBookmarks.AssetType)
	            {		                                 
                    case eAssetTypes.EPG:
                        AssetType = AssetTypes.EPG;
                        break;
                    case eAssetTypes.NPVR:
                        AssetType = AssetTypes.NPVR;
                        break;
                    case eAssetTypes.MEDIA:
                        AssetType = AssetTypes.Media;
                        break;
                    default:
                        AssetType = AssetTypes.UNKNOWN;
                        break;
	            }

                foreach (Bookmark bookmark in assetBookmarks.Bookmarks)
                {
                    AssetBookmarkResponse assetBookmarkResponse = new AssetBookmarkResponse(bookmark);
                }
            }
        }
    }

    /// <summary>
    /// Summary description for AssetBookmarkResponse
    /// </summary>
    public class AssetBookmarkResponse
    {
        [JsonProperty(PropertyName = "User")]
        public AssetBookmarkUserResponse User { get; set; }

        [JsonProperty(PropertyName = "Position")]
        public int Position { get; set; }

        [JsonProperty(PropertyName = "PositionOwner")]
        public AssetPositionOwner PositionOwner { get; set; }

        public AssetBookmarkResponse(Bookmark bookmark)
        {
            if (bookmark != null)
            {
                Position = bookmark.Location;
                switch (bookmark.UserType)
                {
                    case eUserType.HOUSEHOLD:
                        PositionOwner = AssetPositionOwner.Household;
                        break;
                    case eUserType.PERSONAL:
                        PositionOwner = AssetPositionOwner.User;
                        break;
                    default:
                        PositionOwner = AssetPositionOwner.UNKNOWN;
                        break;
                }

                User = new AssetBookmarkUserResponse(bookmark.User);
            }
        }

    }

    /// <summary>
    /// Summary description for AssetBookmarkUserResponse
    /// </summary>
    public class AssetBookmarkUserResponse
    {
        [JsonProperty(PropertyName = "UserID")]
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "Username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string LastName { get; set; }

        public AssetBookmarkUserResponse(User user)
        {
            if (user != null && user.m_oBasicDataField != null)
            {
                UserID = user.m_sSiteGUIDField;
                Username = user.m_oBasicDataField.m_sUserNameField;
                FirstName = user.m_oBasicDataField.m_sFirstNameField;
                LastName = user.m_oBasicDataField.m_sLastNameField;
            }
        }
    }

    public enum AssetPositionOwner
    {
        Household,
        User,
        UNKNOWN
    }

}