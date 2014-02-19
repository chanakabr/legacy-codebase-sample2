using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3;
using System.IO;
using Amazon.S3.Model;
using System.Threading;
using Amazon;
using System.Security.Cryptography;

namespace Uploader
{
    public class AmazonUploader : BaseUploader
    {
        private string m_sRegion;

        public AmazonUploader(int nGroupID,string sAddress, string sUN, string sPass, string sPrefix, string sRegion)
        {
            m_nGroupID = nGroupID;
            m_sAddress = sAddress;
            m_sUserName = sUN;
            m_sPass = sPass;
            m_sPrefix = sPrefix;
            m_sRegion = sRegion;
        }

        public override bool Upload(string fileToUpload, bool deleteFileAfterUpload)
        {
            bool isSuccess = false;

            if (m_sAddress.Trim() == "")
                return true;

            Logger.Logger.Log("Upload - Start.", "File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass, "AmazonUploader");

            FileStream fs = null;

            try
            {
                FileInfo fileInf = new FileInfo(fileToUpload);

                if (!fileInf.Exists)
                {
                    throw new Exception("File does not exist : " + fileToUpload);
                }
                else
                {
                    fs = fileInf.OpenRead();

                    AmazonS3Config amazonS3Config = new AmazonS3Config();

                    if (!string.IsNullOrEmpty(m_sRegion))
                        amazonS3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(m_sRegion);

                    using (IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(m_sUserName, m_sPass, amazonS3Config))
                    {
                        PutObjectRequest request = new PutObjectRequest();

                        request.InputStream = fs;
                        request.BucketName = m_sAddress;
                        request.Key = m_sPrefix + "/" + fileInf.Name;
                        request.CannedACL = S3CannedACL.PublicRead;

                        PutObjectResponse putObjectResponse = client.PutObject(request);

                        using (var md5Hasher = MD5.Create())
                        {
                            using (var fs2 = fileInf.OpenRead())
                            {
                                StringBuilder sb = new StringBuilder();

                                foreach (Byte b in md5Hasher.ComputeHash(fs2))
                                    sb.Append(b.ToString("x2").ToLower());

                                if (putObjectResponse.ETag.Replace("\"", string.Empty) != sb.ToString())
                                    throw new Exception("Failed to copy file to Amazon S3");
                            }
                        }
                    }

                    Logger.Logger.Log("Upload - Finish.", "File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass, "AmazonUploader");

                    if (deleteFileAfterUpload)
                    {
                        fileInf.Delete();

                        Logger.Logger.Log("Upload - Delete File After Upload.", "File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass, "AmazonUploader");
                    }

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;

                Logger.Logger.Log("Upload - Error.", "File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass + ", Exception: " + " || " + ex.Message + " || " + ex.StackTrace, "AmazonUploader");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();

                    //Logger.Logger.Log("ProccessJob, File Stream Closed: ", fileToUpload, "AmazonUploader");
                }
            }

            return isSuccess;
        }

        public override void UploadDirectory(string directoryToUpload)
        {
            if (m_sAddress.Trim() == "")
                return;

            if (!Directory.Exists(directoryToUpload))
            {
                Logger.Logger.Log("UploadDirectory - Dirctory does not exist.", "Directory: " + directoryToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass, "AmazonUploader");

                return;
            }

            int failCount = 0;

            string[] files = Directory.GetFiles(directoryToUpload);

            if (files != null && files.Length > 0)
            {
                files = Directory.GetFiles(directoryToUpload);

                AddUploadGroup();

                Logger.Logger.Log("UploadDirectory - Start.", "Directory: " + directoryToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass, "AmazonUploader");

                foreach (string file in files)
                {
                    if (failCount > 3)
                    {
                        Logger.Logger.Log("UploadDirectory - Fail Count Limit Exceeded.", "Directory: " + directoryToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass, "AmazonUploader");

                        RemoveUploadGroup();

                        break;
                    }

                    if (!Upload(file, false))
                    {
                        failCount++;

                        Logger.Logger.Log("UploadDirectory - Error.", "Directory: " + directoryToUpload + ", File: " + file + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass, "AmazonUploader");
                    }
                }

                RemoveUploadGroup();
            }
        }

        protected override void ProccessJob(UploadJob job, ref int nFailCount)
        {
            string file = string.Format("{0}{1}", m_sBasePath, job.file_name);

            FileStream fs = null;
            FileInfo fileInf = null;

            try
            {
                Logger.Logger.Log("ProccessJob - Start.", "Job: " + job.ToString(), "AmazonUploader");

                fileInf = new FileInfo(file);

                if (!fileInf.Exists)
                {
                    throw new Exception("File does not exist : " + file);
                }
                else
                {
                    fs = fileInf.OpenRead();

                    AmazonS3Config amazonS3Config = new AmazonS3Config();

                    if (!string.IsNullOrEmpty(m_sRegion))
                        amazonS3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(m_sRegion);

                    using (IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(m_sUserName, m_sPass, amazonS3Config))
                    {
                        PutObjectRequest request = new PutObjectRequest();

                        request.InputStream = fs;
                        request.BucketName = m_sAddress;
                        //request.WithKey(job.media_id > 0 ? m_sPrefix + "/" + job.media_id + "/" + fileInf.Name : m_sPrefix + "/" + fileInf.Name);
                        request.Key = m_sPrefix + "/" + fileInf.Name;
                        request.CannedACL = S3CannedACL.PublicRead;

                        PutObjectResponse putObjectResponse = client.PutObject(request);

                        using (var md5Hasher = MD5.Create())
                        {
                            using (var fs2 = fileInf.OpenRead())
                            {
                                StringBuilder sb = new StringBuilder();

                                foreach (Byte b in md5Hasher.ComputeHash(fs2))
                                    sb.Append(b.ToString("x2").ToLower());

                                if (putObjectResponse.ETag.Replace("\"", string.Empty) != sb.ToString())
                                    throw new Exception("Failed to copy file to Amazon S3");
                            }
                        }
                    }

                    job.upload_status = UploadJobStatus.FINISHED;

                    Logger.Logger.Log("ProccessJob - Finish.", "Job: " + job.ToString(), "AmazonUploader");
                }
            }
            catch (Exception ex)
            {
                Interlocked.Add(ref nFailCount, 1);

                job.fail_count++;

                Logger.Logger.Log("ProccessJob - Error.", "Job: " + job.ToString() + ", Exception: " + ex.Message, "AmazonUploader");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();

                    //Logger.Logger.Log("File Stream Closed.", "File: " + file, "UploadedPics");
                }
                if (fileInf != null && job.upload_status == UploadJobStatus.FINISHED)
                {
                    fileInf.Delete();

                    //Logger.Logger.Log("DeleteFile.", "File: " + file, "UploadedPics");
                }

                UploadHelper.UpdateJob(job);
            }
        }

    }

}
