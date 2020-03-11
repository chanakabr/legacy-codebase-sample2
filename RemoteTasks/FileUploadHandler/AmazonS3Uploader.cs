using System;
using System.Text;
using Amazon.S3;
using System.IO;
using Amazon.S3.Model;
using Amazon;
using System.Security.Cryptography;

namespace FileUploadHandler
{
    public class AmazonS3Uploader : BaseUploader
    {
        private string m_sRegion;

        public AmazonS3Uploader(string sAddress, string sUN, string sPass, string sPrefix, string sRegion)
        {
            m_sAddress = sAddress;
            m_sUserName = sUN;
            m_sPass = sPass;
            m_sPrefix = sPrefix;
            m_sRegion = sRegion;
        }

        public override void Upload(string file, string fileName)
        {
            byte[] bytes = Convert.FromBase64String(file);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                AmazonS3Config amazonS3Config = new AmazonS3Config();

                if (!string.IsNullOrEmpty(m_sRegion))
                    amazonS3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(m_sRegion);

                using (var client = new AmazonS3Client(m_sUserName, m_sPass, amazonS3Config))
                {
                    PutObjectRequest request = new PutObjectRequest
                    {
                        InputStream = ms,
                        BucketName = m_sAddress,
                        Key = m_sPrefix + "/" + fileName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    PutObjectResponse putObjectResponse = client.PutObject(request);

                    using (var md5Hasher = MD5.Create())
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (Byte b in md5Hasher.ComputeHash(bytes))
                            sb.Append(b.ToString("x2").ToLower());

                        if (putObjectResponse.ETag.Replace("\"", string.Empty) != sb.ToString())
                            throw new Exception("Failed to copy file to Amazon S3");
                    }
                }
            }
        }
    }

}
