using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class EpgAsset : Asset
    {

        public EPGChannelProgrammeObject Epg { get; set; }

        public EpgAsset()
            : base()
        {
            this.AssetType = eAssetTypes.EPG;
            this.Epg = new EPGChannelProgrammeObject();
        }

        public EpgAsset(ProgramObj programObj)
            :base()
        {
            this.AssetType = eAssetTypes.EPG;
            this.Epg = programObj != null ? programObj.m_oProgram : new EPGChannelProgrammeObject();
            this.Id = this.Epg.EPG_ID;
            this.UpdateDate = programObj != null ? programObj.m_dUpdateDate : DateTime.UtcNow;
        }

    }
}
