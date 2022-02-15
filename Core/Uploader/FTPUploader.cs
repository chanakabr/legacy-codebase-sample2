using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Phx.Lib.Log;
using System.Reflection;

namespace Uploader
{
    public class FTPUploader : BaseUploader
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected Int32 m_nFTPPort;

        public FTPUploader(int nGroupID, string sAddress, string sUN, string sPass, string sPrefix)
        {
            m_sAddress = sAddress;
            if (m_sAddress.StartsWith("ftp://") == true)
                m_sAddress = m_sAddress.Substring(6);
            if (m_sAddress.StartsWith("sftp://") == true)
                m_sAddress = m_sAddress.Substring(7);

            m_sUserName = sUN;
            m_sPass = sPass;
            m_sPrefix = sPrefix;
            m_nFTPPort = 21;
            m_nGroupID = nGroupID;
        }

        public override bool Upload(string fileToUpload, bool deleteFileAfterUpload)
        {
            if (m_sAddress.Trim() == "")
                return false;

            bool res = true;

            while (m_nNumberOfRuningUploads > 10)
            {
                System.Threading.Thread.Sleep(500);

                log.Debug("Upload - Waiting (more then 10 uploads parallel). - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);
            }

            m_nNumberOfRuningUploads++;

            log.Debug("Upload - Start. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

            FileInfo fileInf = new FileInfo(fileToUpload);

            string uri = string.Empty;

            if (string.IsNullOrEmpty(m_sPrefix))
            {
                uri = "ftp://" + m_sAddress + "/" + fileInf.Name;
            }
            else
            {
                uri = "ftp://" + m_sAddress + "/" + m_sPrefix + "/" + fileInf.Name;
            }

            FtpWebRequest reqFTP = null;

            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            reqFTP.Credentials = new NetworkCredential(m_sUserName, m_sPass);
            reqFTP.UsePassive = true;
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
            reqFTP.ConnectionGroupName = "Ftp_" + m_nGroupID.ToString();
            reqFTP.Timeout = 240000;

            Stream strm = reqFTP.GetRequestStream();
            reqFTP.ContentLength = fileInf.Length;

            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            FileStream fs = fileInf.OpenRead();

            try
            {
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }

                log.Debug("Upload - Finish. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

                if (deleteFileAfterUpload)
                {
                    fileInf.Delete();

                    log.Debug("Upload - Delete. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);
                }
            }
            catch (Exception ex)
            {
                res = false;
                log.Error("Upload - Error. - File: " + fileToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass + ", Exception: " + " || " + ex.Message + " || " + ex.StackTrace, ex);
            }
            finally
            {
                if (strm != null)
                {
                    strm.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }

            m_nNumberOfRuningUploads--;

            return res;
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
                    Stream strm = null;
                    FileStream fs = null;

                    try
                    {
                        if (failCount > 3)
                        {
                            log.Debug("UploadDirectory - Fail Count Limit Exceeded. - Directory: " + directoryToUpload + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

                            RemoveUploadGroup();

                            break;
                        }

                        log.Debug("UploadDirectory - Start. - Directory: " + directoryToUpload + ", File: " + file + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);

                        FileInfo fileInf = new FileInfo(file);

                        string uri = string.Empty;

                        if (string.IsNullOrEmpty(m_sPrefix))
                        {
                            uri = "ftp://" + m_sAddress + "/" + fileInf.Name;
                        }
                        else
                        {
                            uri = "ftp://" + m_sAddress + "/" + m_sPrefix + "/" + fileInf.Name;
                        }

                        FtpWebRequest reqFTP = null;

                        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                        reqFTP.Credentials = new NetworkCredential(m_sUserName, m_sPass);
                        reqFTP.UsePassive = true;
                        reqFTP.KeepAlive = false;
                        reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                        reqFTP.UseBinary = true;
                        reqFTP.ConnectionGroupName = "Ftp_" + m_nGroupID.ToString();
                        reqFTP.Timeout = 240000;

                        strm = reqFTP.GetRequestStream();

                        reqFTP.ContentLength = fileInf.Length;
                        int buffLength = 2048;
                        byte[] buff = new byte[buffLength];
                        int contentLen;
                        fs = fileInf.OpenRead();

                        try
                        {

                            contentLen = fs.Read(buff, 0, buffLength);

                            while (contentLen != 0)
                            {
                                strm.Write(buff, 0, contentLen);
                                contentLen = fs.Read(buff, 0, buffLength);
                            }

                            log.Debug("UploadDirectory - Finish. - Directory: " + directoryToUpload + ", File: " + file + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass);
                        }
                        catch (Exception ex)
                        {
                            failCount++;

                            log.Error("UploadDirectory - Error. - Directory: " + directoryToUpload + ", File: " + file + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass + ", Exception: " + " || " + ex.Message + " || " + ex.StackTrace, ex);

                            if (failCount > 3)
                            {

                            }
                        }
                        finally
                        {
                            if (strm != null)
                            {
                                strm.Close();
                            }
                            if (fs != null)
                            {
                                fs.Close();
                            }
                            if (fileInf != null)
                            {
                                fileInf.Delete();
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;

                        log.Error("UploadDirectory - Error. - Directory: " + directoryToUpload + ", File: " + file + ", To: " + m_sAddress + " With Username: " + m_sUserName + ", Password: " + m_sPass + ", Exception: " + " || " + ex.Message + " || " + ex.StackTrace, ex);
                    }
                    finally
                    {
                        if (strm != null)
                        {
                            strm.Close();
                        }
                        if (fs != null)
                        {
                            fs.Close();
                        }

                    }
                }

                RemoveUploadGroup();
            }
        }

        protected override void ProccessJob(UploadJob job, ref int nFailCount)
        {
            string file = string.Format("{0}{1}", m_sBasePath, job.file_name);

            Stream strm = null;
            FileStream fs = null;

            try
            {
                log.Debug("ProccessJob - Start. - Job: " + job.ToString());

                FileInfo fileInf = new FileInfo(file);

                if (!fileInf.Exists)
                {
                    throw new Exception("File does not exist : " + file);
                }
                else
                {
                    string uri = string.Empty;

                    //if (string.IsNullOrEmpty(m_sPrefix))
                    //{
                    //    uri = "ftp://" + m_sAddress;

                    //    uri = job.media_id > 0 ? uri + "/" + job.media_id + "/" + fileInf.Name : uri + "/" + fileInf.Name;
                    //}
                    //else
                    //{
                    //    uri = "ftp://" + m_sAddress + "/" + m_sPrefix;

                    //    uri = job.media_id > 0 ? uri + "/" + job.media_id + "/" + fileInf.Name : uri + "/" + fileInf.Name;
                    //}

                    if (string.IsNullOrEmpty(m_sPrefix))
                    {
                        uri = "ftp://" + m_sAddress + "/" + fileInf.Name;
                    }
                    else
                    {
                        uri = "ftp://" + m_sAddress + "/" + m_sPrefix + "/" + fileInf.Name;
                    }

                    FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

                    reqFTP.Credentials = new NetworkCredential(m_sUserName, m_sPass);
                    reqFTP.UsePassive = true;
                    reqFTP.KeepAlive = false;
                    reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                    reqFTP.UseBinary = true;
                    reqFTP.ConnectionGroupName = "Ftp_" + m_nGroupID.ToString();
                    reqFTP.Timeout = 240000;
                    strm = reqFTP.GetRequestStream();
                    reqFTP.ContentLength = fileInf.Length;

                    int buffLength = 2048;
                    byte[] buff = new byte[buffLength];
                    int contentLen;

                    fs = fileInf.OpenRead();

                    try
                    {
                        contentLen = fs.Read(buff, 0, buffLength);

                        while (contentLen != 0)
                        {
                            strm.Write(buff, 0, contentLen);
                            contentLen = fs.Read(buff, 0, buffLength);
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
                        if (strm != null)
                        {
                            strm.Close();
                        }
                        if (fs != null)
                        {
                            fs.Close();
                        }
                        if (fileInf != null && job.upload_status == UploadJobStatus.FINISHED)
                        {
                            fileInf.Delete();
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
                if (strm != null)
                {
                    strm.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }

                UploadHelper.UpdateJob(job);
            }
        }
    }
}
