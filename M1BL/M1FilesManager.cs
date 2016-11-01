using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DAL;
using System.Net;
using System.Text.RegularExpressions;
using KLogMonitor;
using System.Reflection;

namespace M1BL
{
    public class M1FilesManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private DataTable m_dtTransactions;

        private int m_nGroupID;
        private int m_nHoursOffset;

        private string m_sFtpDirectory;
        private string m_sFtpUser;
        private string m_sFtpPassword;
        private string m_sFtp_ppv_folder;
        private string m_sFtp_subscription_folder;
        private string m_sPPVFilesBasePath;
        private string m_sSubscriptionFilesBasePath;

        private string m_sFilePrefix;       //"M1"
        private string m_sFileExtension;    //".cdr"
        private int m_nFilesStartCounter;
        private int m_nFilesMaxCounter;
        private double m_dGst;


        private string m_sPPVFileNextCounter;
        private string m_sPPVHeaderRecordType;
        private string m_sPPVHeaderContentProviderID;
        private string m_sPPVHeaderContentProviderName;
        private string m_sPPVBodyRecordType;
        private string m_sPPVBodyServiceType;
        private string m_sPPVBodyChargedNumberPrefix;
        private string m_sPPVBodyTaxIndicator;
        private int m_nPPVBodyChargeableUnits;
        private string m_sPPVTrailerRecordType;

        private string m_sSubscriptionFileNextCounter;
        private string m_sSubscriptionHeaderRecordType;
        private string m_sSubscriptionHeaderContentProviderID;
        private string m_sSubscriptionHeaderContentProviderName;
        private string m_sSubscriptionBodyRecordType;
        private string m_sSubscriptionBodyServiceType;
        private string m_sSubscriptionBodyUsageType;
        private string m_sSubscriptionBodyChargedNumberPrefix;
        private string m_sSubscriptionTrailerRecordType;

        public M1FilesManager(int nGroupID)
        {
            Initialize(nGroupID);
        }

        public string ProcessCdrFile(M1ItemType fileType)
        {
            string sFileName = string.Empty;
            string sFolderPath = string.Empty;
            string sFtpFolder = string.Empty;
            int nNextFileCounter = 0;
            string sData = string.Empty;
            string sNextCounter = string.Empty;
            List<int> transactionsIDsList = new List<int>();
            DateTime dFileCreatedDate = DateTime.UtcNow;
            string sContentProviderID = string.Empty;

            try
            {
                if (fileType == M1ItemType.PPV)
                {
                    int.TryParse(m_sPPVFileNextCounter, out nNextFileCounter);
                    sData = GetPPVFileData(dFileCreatedDate, m_sPPVFileNextCounter, ref transactionsIDsList);
                    sNextCounter = m_sPPVFileNextCounter;
                    sContentProviderID = m_sPPVHeaderContentProviderID.PadLeft(PPVFileStructure.FILE_CONTENT_PROVIDER_ID, '0');
                    sFolderPath = m_sPPVFilesBasePath;
                    sFtpFolder = m_sFtp_ppv_folder;
                }
                else if (fileType == M1ItemType.Subscription)
                {
                    int.TryParse(m_sSubscriptionFileNextCounter, out nNextFileCounter);
                    sData = GetSubscriptionFileData(dFileCreatedDate, m_sSubscriptionFileNextCounter, ref transactionsIDsList);
                    sNextCounter = m_sSubscriptionFileNextCounter;
                    sContentProviderID = m_sSubscriptionHeaderContentProviderID.PadLeft(SubscriptionFileStructure.FILE_CONTENT_PROVIDER_ID, '0');
                    sFolderPath = m_sSubscriptionFilesBasePath;
                    sFtpFolder = m_sFtp_subscription_folder;
                }
                sFileName = GetFileName(fileType, sContentProviderID, GetFormattedShortDateTime(dFileCreatedDate), sNextCounter);
                bool bAsciiCreationResult = CreateAsciiFile(sFolderPath, sFileName, sData);
                if (bAsciiCreationResult)
                {
                    bool bFtpSentResult = SendFileViaFtp(sFileName, sFtpFolder);
                    if (bFtpSentResult)
                    {
                        int nFileID = SaveFileHistoryRecord(fileType, nNextFileCounter, sFileName);
                        UpdateTransactionsStatus(m_nGroupID, transactionsIDsList, M1TransactionStatus.Success, nFileID);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error on processing m1 cdr file: " + sFileName + ", exception:" + ex.Message + " || " + ex.StackTrace, ex);
            }

            return sFileName;
        }

        public int SaveFileHistoryRecord(M1ItemType fileType, int nFileCounter, string sFileName)
        {
            int nFileID = BillingDAL.Insert_M1FileHistoryRecord(m_nGroupID, (int)fileType, nFileCounter, sFileName);
            return nFileID;
        }

        public void UpdateTransactionsStatus(int nGroupID, List<int> transactionsIDsList, M1TransactionStatus transactionStatus, int nFileID)
        {
            if (transactionsIDsList != null && transactionsIDsList.Count > 0)
            {
                BillingDAL.UpdateM1Transactions(nGroupID, transactionsIDsList, (int)transactionStatus, nFileID);
            }
        }


        private void Initialize(int nGroupID)
        {
            try
            {
                m_nGroupID = nGroupID;
                m_nHoursOffset = 0;

                string sGMTOffset = GetTcmConfigValue(string.Format("GMTOffset_{0}", m_nGroupID.ToString()));
                if (!string.IsNullOrEmpty(sGMTOffset))
                {
                    m_nHoursOffset = int.Parse(sGMTOffset);
                }

                DataSet dsGroupParams = BillingDAL.Get_M1GroupParameters(m_nGroupID, null);
                if (dsGroupParams != null && dsGroupParams.Tables.Count > 0)
                {
                    DataTable dtGroupParams = dsGroupParams.Tables[0];
                    DataTable dtFilesCounters = dsGroupParams.Tables[1];

                    ReadGroupParametersData(dtGroupParams);
                    ReadFilesCountersData(dtFilesCounters);

                    m_dtTransactions = BillingDAL.Get_M1Transactions(nGroupID, (int)M1TransactionStatus.Pending);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error on initializing m1 files manager, group id:" + m_nGroupID.ToString() + ", exception:" + ex.Message + " || " + ex.StackTrace, ex);
            }
        }

        private void ReadGroupParametersData(DataTable dtGroupParams)
        {
            if (dtGroupParams != null && dtGroupParams.Rows.Count > 0)
            {
                DataRow groupParameterRow = dtGroupParams.Rows[0];

                m_sFtpDirectory = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_ftp"]);
                m_sFtpUser = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_ftp_user"]);
                m_sFtpPassword = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_ftp_password"]);
                m_sFtp_ppv_folder = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_ftp_ppv_folder"]);
                m_sFtp_subscription_folder = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_ftp_subscription_folder"]);
                m_sPPVFilesBasePath = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_ppv_base_path"]);
                m_sSubscriptionFilesBasePath = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_subscription_base_path"]);
                m_sFilePrefix = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_Prefix"]);
                m_sFileExtension = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["cdrFiles_Extension"]);
                m_nFilesStartCounter = ODBCWrapper.Utils.GetIntSafeVal(groupParameterRow["cdrFiles_start_counter"]);
                m_nFilesMaxCounter = ODBCWrapper.Utils.GetIntSafeVal(groupParameterRow["cdrFiles_max_counter"]);
                m_dGst = ODBCWrapper.Utils.GetDoubleSafeVal(groupParameterRow["gst"]);

                m_sPPVHeaderContentProviderID = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_header_content_provider_id"]);
                m_sPPVHeaderContentProviderName = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_header_content_provider_name"]);
                m_sPPVHeaderRecordType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_header_record_type"]);
                m_sPPVBodyRecordType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_body_record_type"]);
                m_sPPVBodyServiceType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_body_service_type"]);
                m_sPPVBodyChargedNumberPrefix = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_body_charged_number_prefix"]);
                m_sPPVBodyTaxIndicator = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_body_tax_indictaor"]);
                m_nPPVBodyChargeableUnits = ODBCWrapper.Utils.GetIntSafeVal(groupParameterRow["ppv_body_chargeable_units"]);
                m_sPPVTrailerRecordType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["ppv_trailer_record_type"]);

                m_sSubscriptionHeaderContentProviderID = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_header_content_provider_id"]);
                m_sSubscriptionHeaderContentProviderName = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_header_content_provider_name"]);
                m_sSubscriptionHeaderRecordType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_header_record_type"]);
                m_sSubscriptionBodyRecordType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_body_record_type"]);
                m_sSubscriptionBodyServiceType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_body_service_type"]);
                m_sSubscriptionBodyUsageType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_body_usage_type"]);
                m_sSubscriptionBodyChargedNumberPrefix = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_body_charged_number_prefix"]);
                m_sSubscriptionTrailerRecordType = ODBCWrapper.Utils.GetSafeStr(groupParameterRow["subscription_trailer_record_type"]);
            }
        }

        private void ReadFilesCountersData(DataTable dtFilesCounters)
        {
            if (dtFilesCounters != null && dtFilesCounters.Rows.Count > 0)
            {
                foreach (DataRow rowFile in dtFilesCounters.Rows)
                {
                    M1ItemType fileType = (M1ItemType)ODBCWrapper.Utils.GetIntSafeVal(rowFile["item_Type"]);
                    if (fileType == M1ItemType.PPV)
                    {
                        int nCurrentPPVCounter = ODBCWrapper.Utils.GetIntSafeVal(rowFile["currentFileCounter"]);
                        m_sPPVFileNextCounter = GetFormattedNextFileCounter(nCurrentPPVCounter, m_nFilesStartCounter, m_nFilesMaxCounter, PPVFileStructure.FILE_SEQUENCE_NUMBER);

                    }
                    else if (fileType == M1ItemType.Subscription)
                    {
                        int nCurrentSubscriptionCounter = ODBCWrapper.Utils.GetIntSafeVal(rowFile["currentFileCounter"]);
                        m_sSubscriptionFileNextCounter = GetFormattedNextFileCounter(nCurrentSubscriptionCounter, m_nFilesStartCounter, m_nFilesMaxCounter, SubscriptionFileStructure.FILE_SEQUENCE_NUMBER);
                    }
                }
            }

            if (string.IsNullOrEmpty(m_sPPVFileNextCounter))
            {
                m_sPPVFileNextCounter = GetFormattedNextFileCounter(0, m_nFilesStartCounter, m_nFilesMaxCounter, PPVFileStructure.FILE_SEQUENCE_NUMBER);
            }

            if (string.IsNullOrEmpty(m_sSubscriptionFileNextCounter))
            {
                m_sSubscriptionFileNextCounter = GetFormattedNextFileCounter(0, m_nFilesStartCounter, m_nFilesMaxCounter, SubscriptionFileStructure.FILE_SEQUENCE_NUMBER);
            }
        }

        private string GetFormattedLongDateTime(DateTime dateToFormat)
        {
            return dateToFormat.ToString("yyyyMMddHHmmss");
        }

        private string GetFormattedShortDateTime(DateTime dateToFormat)
        {
            return dateToFormat.ToString("yyyyMMdd");
        }

        private string GetFormattedNextFileCounter(int currentCounter, int startCounter, int maxCounter, int len)
        {
            //int nNextCounter = (currentCounter < maxCounter) ? ((currentCounter % maxCounter) + 1) : startCounter;
            int nNextCounter = (currentCounter % maxCounter) + 1;
            return nNextCounter.ToString().PadLeft(len, '0');
        }

        private bool CreateAsciiFile(string sFolderPath, string sFileName, string sData)
        {
            bool result = true;
            try
            {
                if (!Directory.Exists(sFolderPath))
                {
                    Directory.CreateDirectory(sFolderPath);
                }

                ASCIIEncoding asciiEncoding = new ASCIIEncoding();

                using (StreamWriter sw = new StreamWriter(sFileName, false, asciiEncoding))
                {
                    sw.Write(sData);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error on creating m1 cdr file: " + sFileName + ", exception:" + ex.Message + " || " + ex.StackTrace, ex);
                result = false;
            }
            return result;
        }

        private string GetPPVFileData(DateTime dFileCreatedDate, string sNextFileCounter, ref List<int> transactionsIDsList)
        {
            StringBuilder sbData = new StringBuilder();
            string sFileCreatedDate = GetFormattedLongDateTime(dFileCreatedDate);

            double totalPrice = 0;
            int totalRecords = 0;

            sbData.Append(ParseStringToSize(m_sPPVHeaderRecordType, PPVFileStructure.HEADER_RECORD_TYPE));
            sbData.Append(m_sPPVHeaderContentProviderID.PadLeft(PPVFileStructure.HEADER_CONTENT_PROVIDER_ID, '0'));
            sbData.Append(m_sPPVHeaderContentProviderName.PadRight(PPVFileStructure.HEADER_CONTENT_PROVIDER_NAME, ' '));
            sbData.Append(sNextFileCounter);
            sbData.Append(sFileCreatedDate);

            sbData.Append(Environment.NewLine);

            if (m_dtTransactions != null && m_dtTransactions.Rows.Count > 0)
            {
                DataRow[] ppvTransactionsRows = m_dtTransactions.Select("item_type=" + (int)M1ItemType.PPV);
                
                foreach (DataRow rowPPV in ppvTransactionsRows)
                {
                    int nM1TransactionID = ODBCWrapper.Utils.GetIntSafeVal(rowPPV["id"]);
                    long nBillingTransactionID = ODBCWrapper.Utils.GetIntSafeVal(rowPPV["BillingTransactionID"]);
                    string sChargedNumber = ODBCWrapper.Utils.GetSafeStr(rowPPV["charged_mobile_number"]);
                    //string sFormattedChargedNumber = m_sSubscriptionBodyChargedNumberPrefix + sChargedNumber;

                    DateTime dCallDateTime = ODBCWrapper.Utils.GetDateSafeVal(rowPPV["create_date"]);
                    dCallDateTime = dCallDateTime.AddHours(m_nHoursOffset);

                    //string sServiceDescription = ODBCWrapper.Utils.GetSafeStr(rowPPV["item_description"]);
                    double nPrice = ODBCWrapper.Utils.GetDoubleSafeVal(rowPPV["price"]);
                    nPrice = nPrice / (1 + (m_dGst / 100));

                    string sFormattedChargedNumber = string.Empty;
                    string sRateCode = string.Empty;
                    string sAnnotation = string.Empty;
                    //string sSpareField = string.Empty;
                    string sFormattedBillDescription = GetFormattedBillDescription(nBillingTransactionID);

                    sbData.Append(ParseStringToSize(m_sPPVBodyRecordType, PPVFileStructure.BODY_RECORD_TYPE));
                    sbData.Append(ParseStringToSize(m_sPPVBodyServiceType, PPVFileStructure.BODY_SERVICE_TYPE));
                    sbData.Append(sFormattedChargedNumber.PadRight(PPVFileStructure.BODY_CHARGED_NUMBER, ' '));
                    sbData.Append(GetFormattedLongDateTime(dCallDateTime));
                    sbData.Append(sFormattedBillDescription.PadRight(PPVFileStructure.BODY_SERVICE_DESC, ' '));
                    sbData.Append(ParseStringToSize(m_sPPVBodyTaxIndicator, PPVFileStructure.BODY_TAX_INDICATOR));
                    sbData.Append(GetFormattedPrice(nPrice, PPVFileStructure.BODY_PRICE));
                    sbData.Append(m_nPPVBodyChargeableUnits.ToString().PadLeft(PPVFileStructure.BODY_CHARGEABLE_UNITS, '0'));
                    sbData.Append(GetFormattedPrice(nPrice, PPVFileStructure.BODY_UNIT_PRICE));
                    sbData.Append(sRateCode.PadRight(PPVFileStructure.BODY_RATE_CODE, ' '));
                    sbData.Append(sAnnotation.PadRight(PPVFileStructure.BODY_ANNOTATION, ' '));
                    sbData.Append(sChargedNumber.PadRight(PPVFileStructure.BODY_SPARE_FIELD, ' ')); // Spare field
                    sbData.Append(Environment.NewLine);

                    totalPrice += Math.Round(nPrice, 2);
                    totalRecords++;
                    transactionsIDsList.Add(nM1TransactionID);
                }
            }

            sbData.Append(ParseStringToSize(m_sPPVTrailerRecordType, PPVFileStructure.TRAILER_RECORD_TYPE));
            sbData.Append(ParseStringToSize(sFileCreatedDate, PPVFileStructure.TRAILER_CREATED_DATE_TIME));
            sbData.Append(sNextFileCounter);
            sbData.Append(totalRecords.ToString().PadLeft(PPVFileStructure.TRAILER_TOTAL_RECORDS, '0'));
            sbData.Append(totalRecords.ToString().PadLeft(PPVFileStructure.TRAILER_TOTAL_CHARGEABLE_UNITS, '0'));
            sbData.Append(GetFormattedPrice(totalPrice, PPVFileStructure.TRAILER_TOTAL_PRICE));

            return sbData.ToString();
        }

        private string GetSubscriptionFileData(DateTime dFileCreatedDate, string sNextFileCounter, ref List<int> transactionsIDsList)
        {
            StringBuilder sbData = new StringBuilder();
            string sFileCreatedDate = GetFormattedLongDateTime(dFileCreatedDate);

            double totalPrice = 0;
            int totalRecords = 0;

            sbData.Append(ParseStringToSize(m_sSubscriptionHeaderRecordType, SubscriptionFileStructure.HEADER_RECORD_TYPE));
            sbData.Append(m_sSubscriptionHeaderContentProviderID.PadLeft(SubscriptionFileStructure.HEADER_CONTENT_PROVIDER_ID, '0'));
            sbData.Append(m_sSubscriptionHeaderContentProviderName.PadRight(SubscriptionFileStructure.HEADER_CONTENT_PROVIDER_NAME, ' '));
            sbData.Append(sNextFileCounter);
            sbData.Append(sFileCreatedDate);

            sbData.Append(Environment.NewLine);

            if (m_dtTransactions != null && m_dtTransactions.Rows.Count > 0)
            {
                DataRow[] subscriptionTransactionsRows = m_dtTransactions.Select("item_type=" + (int)M1ItemType.Subscription);
                foreach (DataRow rowSubscription in subscriptionTransactionsRows)
                {
                    int nTransactionID = ODBCWrapper.Utils.GetIntSafeVal(rowSubscription["id"]);
                    long nBillingTransactionID = ODBCWrapper.Utils.GetIntSafeVal(rowSubscription["BillingTransactionID"]);
                    string sChargedNumber = ODBCWrapper.Utils.GetSafeStr(rowSubscription["charged_mobile_number"]);
                    // string sFormattedChargedNumber = m_sSubscriptionBodyChargedNumberPrefix + sChargedNumber;
                    DateTime dTransactionDateTime = ODBCWrapper.Utils.GetDateSafeVal(rowSubscription["create_date"]);
                    string sServiceDescription = Regex.Replace(ODBCWrapper.Utils.GetSafeStr(rowSubscription["item_description"]), "[^A-Za-z0-9 - + ( )]", "");
                    DateTime dSubscriptionStartDate = ODBCWrapper.Utils.GetDateSafeVal(rowSubscription["item_start_date"]);
                    DateTime dSubscriptionEndDate = ODBCWrapper.Utils.GetDateSafeVal(rowSubscription["item_end_date"]);

                    dTransactionDateTime = dTransactionDateTime.AddHours(m_nHoursOffset);
                    dSubscriptionStartDate = dSubscriptionStartDate.AddHours(m_nHoursOffset);
                    dSubscriptionEndDate = dSubscriptionEndDate.AddHours(m_nHoursOffset);

                    double nPrice = ODBCWrapper.Utils.GetDoubleSafeVal(rowSubscription["price"]);
                    nPrice = nPrice / (1 + (m_dGst / 100));

                    string sFormattedChargedNumber = string.Empty;
                    string sSpareField = string.Empty;
                    string sFormattedBillDescription = GetFormattedBillDescription(nBillingTransactionID);


                    sbData.Append(ParseStringToSize(m_sSubscriptionBodyRecordType, SubscriptionFileStructure.BODY_RECORD_TYPE));
                    sbData.Append(ParseStringToSize(m_sSubscriptionBodyServiceType, SubscriptionFileStructure.BODY_SERVICE_TYPE));
                    sbData.Append(ParseStringToSize(m_sSubscriptionBodyUsageType, SubscriptionFileStructure.BODY_USAGE_TYPE));
                    sbData.Append(sFormattedChargedNumber.PadRight(SubscriptionFileStructure.BODY_CHARGED_NUMBER));
                    sbData.Append(GetFormattedLongDateTime(dTransactionDateTime));
                    sbData.Append(sFormattedBillDescription.PadRight(SubscriptionFileStructure.BODY_SERVICE_DESC, ' '));
                    sbData.Append(sFormattedBillDescription.PadRight(SubscriptionFileStructure.BODY_BILL_DESC, ' ')); // Bill description
                    sbData.Append(GetFormattedShortDateTime(dSubscriptionStartDate));
                    sbData.Append(GetFormattedShortDateTime(dSubscriptionEndDate));
                    sbData.Append(GetFormattedPrice(nPrice, SubscriptionFileStructure.BODY_PRICE));
                    sbData.Append(sSpareField.PadRight(SubscriptionFileStructure.BODY_SPARE_FIELD_1, '0'));
                    sbData.Append(sChargedNumber.PadRight(SubscriptionFileStructure.BODY_SPARE_FIELD_2, ' '));
                    sbData.Append(Environment.NewLine);

                    totalPrice += Math.Round(nPrice, 2);
                    totalRecords++;
                    transactionsIDsList.Add(nTransactionID);
                }
            }

            sbData.Append(ParseStringToSize(m_sSubscriptionTrailerRecordType, SubscriptionFileStructure.TRAILER_RECORD_TYPE));
            sbData.Append(ParseStringToSize(sFileCreatedDate, SubscriptionFileStructure.TRAILER_CREATED_DATE_TIME));
            sbData.Append(sNextFileCounter);
            sbData.Append(totalRecords.ToString().PadLeft(SubscriptionFileStructure.TRAILER_TOTAL_RECORDS, '0'));
            sbData.Append(GetFormattedPrice(totalPrice, SubscriptionFileStructure.TRAILER_TOTAL_PRICE));

            return sbData.ToString();
        }

        private string GetFormattedPrice(double price, int len)
        {
            //string strNum = price.ToString("0.00#.##");
            double roundedPrice = Math.Round(price, 2);
            string strNum = roundedPrice.ToString("0.00#.##");
            string[] arr = strNum.Split('.');
            string result = arr[0].PadLeft(len - 2, '0') + arr[1].Substring(0, 2).PadLeft(2, '0');
            return result;
        }

        private string GetFormattedBillDescription(long nBillingTransactionID)
        {
            string result = string.Format("Toggle Id:{0} (Tel:63883888)", nBillingTransactionID.ToString());  //ex: Toggle Id:123456 (Tel:63883888)
            return result;
        }

        string DecimalPlaceNoRounding(double d, int decimalPlaces = 2)
        {
            d = d * Math.Pow(10, decimalPlaces);
            d = Math.Truncate(d);
            d = d / Math.Pow(10, decimalPlaces);
            return string.Format("{0:N" + Math.Abs(decimalPlaces) + "}", d);
        }

        private string GetFileName(M1ItemType fileType, string sContentProviderID, string sDateTime, string sFileCounter)
        {
            StringBuilder sbFileName = new StringBuilder();

            if (fileType == M1ItemType.PPV)
            {
                sbFileName.Append(m_sPPVFilesBasePath + @"\");
            }
            else if (fileType == M1ItemType.Subscription)
            {
                sbFileName.Append(m_sSubscriptionFilesBasePath + @"\");
            }
            sbFileName.Append(m_sFilePrefix);
            sbFileName.Append(sContentProviderID);
            sbFileName.Append(sDateTime);
            sbFileName.Append(sFileCounter);
            sbFileName.Append(m_sFileExtension);

            return Path.GetFullPath(sbFileName.ToString());
        }

        private string ParseStringToSize(string sData, int nSize)
        {
            string sResult = string.Empty;
            if (!string.IsNullOrEmpty(sData))
            {
                if (sData.Length >= nSize)
                {
                    sResult = sData.Substring(0, nSize);
                }
                else
                {
                    sResult = sData;
                }
            }
            return sResult;
        }

        private bool SendFileViaFtp(string sFileName, string sFtpFolder)
        {
            bool result = true;

            try
            {

                FileInfo fileInf = new FileInfo(sFileName);
                string uri = m_sFtpDirectory + "/" + sFtpFolder + "/" + fileInf.Name;

                log.Debug("Start - " + string.Format("file:{0}, ftp:{1}", fileInf.Name, uri));

                if (!uri.StartsWith("ftp:"))
                {
                    uri = "ftp://" + uri;
                    log.Debug("Add ftp - " + string.Format("ftp:{0}", uri));
                }

                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

                //reqFTP.EnableSsl = true;
                //reqFTP.RequestUri.Port
                reqFTP.Credentials = new NetworkCredential(m_sFtpUser, m_sFtpPassword);
                //reqFTP.UsePassive = true;
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                reqFTP.UseBinary = true;
                reqFTP.ContentLength = fileInf.Length;
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                FileStream fs = fileInf.OpenRead();
                Stream strm = null;
                try
                {
                    strm = reqFTP.GetRequestStream();
                    contentLen = fs.Read(buff, 0, buffLength);
                    while (contentLen != 0)
                    {
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }
                    log.Debug("finished - " + string.Format("{0}", fileInf.Name));

                }
                catch (Exception ex)
                {
                    log.Error("Upload - Error - " + string.Format("file:{0}, ex:{1}", fileInf.Name, ex.Message));
                    result = false;
                }

                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                    if (strm != null)
                    {
                        strm.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SendFileViaFtp - Error - " + string.Format("ex:{0}", ex.Message));
            }

            return result;
        }

        private struct PPVFileStructure
        {
            public const int FILE_CONTENT_PROVIDER_ID = 8;
            public const int FILE_SEQUENCE_NUMBER = 4;

            public const int HEADER_RECORD_TYPE = 3;
            public const int HEADER_CONTENT_PROVIDER_ID = 8;
            public const int HEADER_CONTENT_PROVIDER_NAME = 20;
            public const int HEADER_FILE_SEQUENCE_NUMBER = 4;
            public const int HEADER_CREATED_DATE_TIME = 14;

            public const int BODY_RECORD_TYPE = 3;
            public const int BODY_SERVICE_TYPE = 6;
            public const int BODY_CHARGED_NUMBER = 12;
            public const int BODY_CALL_DATE_TIME = 14;
            public const int BODY_SERVICE_DESC = 35;
            public const int BODY_TAX_INDICATOR = 1;
            public const int BODY_PRICE = 10;
            public const int BODY_CHARGEABLE_UNITS = 5;
            public const int BODY_UNIT_PRICE = 8;
            public const int BODY_RATE_CODE = 1;
            public const int BODY_ANNOTATION = 20;
            public const int BODY_SPARE_FIELD = 20;

            public const int TRAILER_RECORD_TYPE = 3;
            public const int TRAILER_CREATED_DATE_TIME = 14;
            public const int TRAILER_FILE_SEQUENCE_NUMBER = 4;
            public const int TRAILER_TOTAL_RECORDS = 8;
            public const int TRAILER_TOTAL_CHARGEABLE_UNITS = 10;
            public const int TRAILER_TOTAL_PRICE = 12;
        }

        private struct SubscriptionFileStructure
        {
            public const int FILE_CONTENT_PROVIDER_ID = 8;
            public const int FILE_SEQUENCE_NUMBER = 4;

            public const int HEADER_RECORD_TYPE = 3;
            public const int HEADER_CONTENT_PROVIDER_ID = 8;
            public const int HEADER_CONTENT_PROVIDER_NAME = 20;
            public const int HEADER_TAPE_SEQUENCE_NUMBER = 4;
            public const int HEADER_CREATED_DATE_TIME = 14;

            public const int BODY_RECORD_TYPE = 3;
            public const int BODY_SERVICE_TYPE = 6;
            public const int BODY_USAGE_TYPE = 4;
            public const int BODY_CHARGED_NUMBER = 12;
            public const int BODY_TRANSACTION_DATE_TIME = 14;
            public const int BODY_SERVICE_DESC = 35;
            public const int BODY_BILL_DESC = 50;
            public const int BODY_START_DATE = 8;
            public const int BODY_END_DATE = 8;
            public const int BODY_PRICE = 10;
            public const int BODY_SPARE_FIELD_1 = 4;
            public const int BODY_SPARE_FIELD_2 = 20;

            public const int TRAILER_RECORD_TYPE = 3;
            public const int TRAILER_CREATED_DATE_TIME = 14;
            public const int TRAILER_TAPE_SEQUENCE_NUMBER = 4;
            public const int TRAILER_TOTAL_RECORDS = 8;
            public const int TRAILER_TOTAL_PRICE = 12;
        }

        private string GetTcmConfigValue(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                log.Error("M1FilesManager - Key=" + sKey + "," + ex.Message);
            }
            return result;
        }

    }
}
