using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// instructions for upload data (the data file type, how to read the file, etc)
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadJobData : KalturaOTTObject
    {
        /// <summary>
        /// Validate the specifics of the job data
        /// Will throw an exception if not valid
        /// </summary>
        internal abstract void Validate(KalturaOTTFile fileData);
    }

    /// <summary>
    /// instructions for upload data type with Excel
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadExcelJobData : KalturaBulkUploadJobData
    {
        internal override void Validate(KalturaOTTFile fileData)
        {
            if (!fileData.path.EndsWith(ExcelFormatter.EXCEL_EXTENTION))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "fileData.path");
            }
        }
    }
}