using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    /// <summary>
    /// Holds information of several types of assets
    /// </summary>
    [DataContract]
    public class AssetInfoResponse : BaseResponse
    {
        #region Data <Members
        
        /// <summary>
        /// List of EPG objects with full information
        /// </summary>
        [DataMember]
        public List<ProgramObj> epgList;

        /// <summary>
        /// List of media objects with full information
        /// </summary>
        [DataMember]
        public List<MediaObj> mediaList;

        #endregion

        #region Ctor

        /// <summary>
        /// Basic initialization
        /// </summary>
        public AssetInfoResponse()
        {
            epgList = new List<ProgramObj>();
            mediaList = new List<MediaObj>();
        }

        #endregion
    }
}
