using ApiLogic.Catalog;
using ApiObjects.Response;
using Core.Catalog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ApiLogic.Api.Managers.Handlers
{
    public interface IFileHandler
    {
        GenericResponse<string> Save(string fileName, OTTBasicFile file, string subDir, string prefix = "");
        GenericResponse<byte[]> Get(string fileName, string fileUrl, string subDir, int groupId = 0, Image image = null);
        GenericResponse<string> GetSubDir(string id, string typeName);
        GenericResponse<string> GetSubDir(long id, string typeName);
        string GetUrl(string subDir, string fileName);
        Status Delete(string fileURL);
        Status ValidateFileContent(FileInfo file, string filePath);
    }
}
