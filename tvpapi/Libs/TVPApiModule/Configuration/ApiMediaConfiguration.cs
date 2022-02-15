using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Threading;
using System.Configuration;
using Tvinci.Configuration.ConfigSvc;
using TVPPro.Configuration.Media;
using Phx.Lib.Log;
using System.Reflection;
using Phx.Lib.Appconfig;

namespace TVPApi.Configuration.Media
{
    public class ApiMediaConfiguration : ConfigurationManager<MediaData>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ApiMediaConfiguration()
        {
            base.SyncFromFile("", true);
            m_syncFile = "";
        }

        public ApiMediaConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

        public ApiMediaConfiguration(int nGroupID, string sPlatform, string sEnvironment)
            : base(eSource.Service)
        {
            SyncFromService(nGroupID, sPlatform, sEnvironment, eConfigType.Media, CreateMediaConfig);
        }

        private MediaData CreateMediaConfig(IEnumerable<ConfigKeyVal> source)
        {
            MediaData retVal = new MediaData();

            retVal.TVM.SearchValues.Tags = DbConfigManager.GetValFromConfig(source, "SearchValues_Tags");
            retVal.TVM.SearchValues.Metadata = DbConfigManager.GetValFromConfig(source, "SearchValues_Metadata");
            retVal.TVM.MediaInfoStruct.Tags = DbConfigManager.GetValFromConfig(source, "MediaInfoStruct_Tags");
            retVal.TVM.MediaInfoStruct.Metadata = DbConfigManager.GetValFromConfig(source, "MediaInfoStruct_Metadata");
            retVal.TVM.GalleryMediaInfoStruct.Tags = DbConfigManager.GetValFromConfig(source, "GalleryMediaInfoStruct_Tags");
            retVal.TVM.GalleryMediaInfoStruct.Metadata = DbConfigManager.GetValFromConfig(source, "GalleryMediaInfoStruct_Metadata");
            retVal.TVM.FlashMediaInfoStruct.Tags = DbConfigManager.GetValFromConfig(source, "FlashMediaInfoStruct_Tags");
            retVal.TVM.FlashMediaInfoStruct.Metadata = DbConfigManager.GetValFromConfig(source, "FlashMediaInfoStruct_Metadata");
            retVal.TVM.MediaListTags.Tags = DbConfigManager.GetValFromConfig(source, "MediaListTags_Tags");
            retVal.TVM.TagsGallery.Tags = DbConfigManager.GetValFromConfig(source, "TagsGallery_Tags");
            retVal.TVM.AutoCompleteValues.Tags = DbConfigManager.GetValFromConfig(source, "AutoCompleteValues_Tags");
            retVal.TVM.AutoCompleteValues.Metadata = DbConfigManager.GetValFromConfig(source, "AutoCompleteValues_Metadata");
            retVal.TVM.AutoCompleteValues.MediaTypeIDs = DbConfigManager.GetValFromConfig(source, "AutoCompleteValues_MediaTypeIDs");

            return retVal;
        }



    }
}
