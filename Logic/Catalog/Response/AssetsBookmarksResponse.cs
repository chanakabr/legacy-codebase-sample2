using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.Response;
using Core.Catalog.Response;
using ApiObjects;
using Core.Users;
using ApiObjects.Catalog;

namespace Core.Catalog.Response
{
    [DataContract]
    public class AssetsBookmarksResponse : BaseResponse
    {

        [DataMember]
        public List<AssetBookmarks> AssetsBookmarks;

        [DataMember]
        public ApiObjects.Response.Status Status;

        public AssetsBookmarksResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }

    [DataContract]
    public class AssetBookmarks
    {
        [DataMember]
        public eAssetTypes AssetType;

        [DataMember]
        public string AssetID;

        [DataMember]
        public List<Bookmark> Bookmarks;

        public AssetBookmarks()
        {
        }

        public AssetBookmarks(eAssetTypes assetType, string assetID, List<Bookmark> bookmarks)
        {
            AssetType = assetType;
            AssetID = assetID;
            Bookmarks = bookmarks;
        }
    }

    [DataContract]
    public class Bookmark
    {
        [DataMember]
        public User User;

        [DataMember]
        public eUserType UserType;

        [DataMember]
        public int Location;

        [DataMember]
        public bool IsFinishedWatching;

        public Bookmark()
        {
        }

        public Bookmark(User user, eUserType userType, int location, bool isFinishedWatching)
        {
            this.User = user;
            this.UserType = userType;
            this.Location = location;
            this.IsFinishedWatching = isFinishedWatching;
        }
    }

}