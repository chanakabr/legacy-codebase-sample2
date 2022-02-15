using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Phx.Lib.Log;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Uploader
{
    public class AmazonUploader : BaseUploader
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string m_sRegion;

        public AmazonUploader(int nGroupID, string sAddress, string sUN, string sPass, string sPrefix, string sRegion)
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

            log.Debug("Upload - Start. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

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

                    using (IAmazonS3 client = new AmazonS3Client(m_sUserName, m_sPass, amazonS3Config))
                    {
                        PutObjectRequest request = new PutObjectRequest
                        {
                            InputStream = fs,
                            BucketName = m_sAddress,
                            Key = m_sPrefix + "/" + fileInf.Name,
                            CannedACL = S3CannedACL.PublicRead
                        };

                        PutObjectResponse putObjectResponse = client.PutObjectAsync(request).GetAwaiter().GetResult();

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

                    log.Debug("Upload - Finish. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

                    if (deleteFileAfterUpload)
                    {
                        fileInf.Delete();

                        log.Debug("Upload - Delete File After Upload. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);
                    }

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;

                log.Error("Upload - Error. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass + ", Exception: " + " || " + ex.Message + " || " + ex.StackTrace, ex);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
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
                log.Debug("UploadDirectory - Directory does not exist. - Directory: " + directoryToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

                return;
            }

            int failCount = 0;

            string[] files = Directory.GetFiles(directoryToUpload);

            if (files != null && files.Length > 0)
            {
                files = Directory.GetFiles(directoryToUpload);

                AddUploadGroup();

                log.Debug("UploadDirectory - Start. - Directory: " + directoryToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

                foreach (string file in files)
                {
                    if (failCount > 3)
                    {
                        log.Debug("UploadDirectory - Fail Count Limit Exceeded. - Directory: " + directoryToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

                        RemoveUploadGroup();

                        break;
                    }

                    if (!Upload(file, false))
                    {
                        failCount++;

                        log.Debug("UploadDirectory - Error. - Directory: " + directoryToUpload + ", File: " + file + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);
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
                log.Debug("ProccessJob - Start. - Job: " + job.ToString());

                fileInf = new FileInfo(file);

                if (!fileInf.Exists)
                {
                    throw new Exception("File does not exist : " + file);
                }
                else
                {
                    try
                    {
                        fs = fileInf.OpenRead();

                        AmazonS3Config amazonS3Config = new AmazonS3Config();

                        if (!string.IsNullOrEmpty(m_sRegion))
                            amazonS3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(m_sRegion);

                        using (IAmazonS3 client = new AmazonS3Client(m_sUserName, m_sPass, amazonS3Config))
                        {
                            PutObjectRequest request = new PutObjectRequest();

                            request.InputStream = fs;
                            request.BucketName = m_sAddress;
                            //request.WithKey(job.media_id > 0 ? m_sPrefix + "/" + job.media_id + "/" + fileInf.Name : m_sPrefix + "/" + fileInf.Name);
                            request.Key = m_sPrefix + "/" + fileInf.Name;
                            request.CannedACL = S3CannedACL.PublicRead;

                            PutObjectResponse putObjectResponse = client.PutObjectAsync(request).GetAwaiter().GetResult();

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

                        log.Debug("ProccessJob - Finish. - Job: " + job.ToString());
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Add(ref nFailCount, 1);

                        job.fail_count++;

                        log.Error("ProccessJob - Error. - Job: " + job.ToString() + ", Exception: " + ex.Message, ex);
                    }
                    finally
                    {
                        if (fileInf != null && job.upload_status == UploadJobStatus.FINISHED)
                        {
                            fileInf.Delete();

                            log.Debug("ProccessJob - Delete. - Job: " + job.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Interlocked.Add(ref nFailCount, 1);
                job.fail_count++;
                log.Error("ProccessJob - Error. - Job: " + job.ToString() + ", Exception: " + ex.Message, ex);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }

                UploadHelper.UpdateJob(job);
            }
        }

    }

}
