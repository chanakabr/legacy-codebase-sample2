using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public abstract class BasePPVModuleDecorator : BasePPVModule
    {
        protected BasePPVModule originalBasePPVModule;

        public BasePPVModuleDecorator(BasePPVModule originalBasePPVModule)
        {
            this.originalBasePPVModule = originalBasePPVModule;
        }

        public abstract PPVModuleContainer[] GetPPVModuleListForAdmin(Int32 nMediaFileID, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);

        public abstract MediaFilePPVModule[] GetPPVModuleListForMediaFiles(Int32[] nMediaFileIDs, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);

        public abstract MediaFilePPVContainer[] GetPPVModuleListForMediaFilesWithExpiry(Int32[] nMediaFileIDs, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
    }
}
