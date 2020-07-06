using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.ObjectsConvertor
{
    public static class KalturaOTTFileExtensions
    {
     
            public static ApiLogic.Catalog.OTTBasicFile ConvertToOttFileType(this Models.General.KalturaOTTFile fileData)
            {
                if (!string.IsNullOrEmpty(fileData.path))
                {
                    return new ApiLogic.Catalog.OTTFile(fileData.path, fileData.name);
                }

                return new ApiLogic.Catalog.OTTStreamFile(fileData.File, fileData.name);

            }
        
    }
}
