using ApiObjects.MediaMarks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace CouchbaseMediaMarksFeeder
{
    public class CouchbaseMediaMarksFeeder
    {
        private static object isRunningMutex = new object();
        private static bool isRunning = false;

        private static readonly string JSON_FILE_ENDING = ".json";
        private static readonly string LOG_FILE = "CouchbaseMediaMarksFeeder";
        private static readonly string DATETIME_PRINT_FORMAT = "yyyyMMddHHmmss";
        private static readonly string LOG_HEADER_STATUS = "Status";
        private static readonly string LOG_HEADER_ERROR = "Error";
        private static readonly string LOG_HEADER_EXCEPTION = "Exception";
        private static readonly string USERS_WITH_NO_DOMAIN_LOG_FILE = "UsersWithNoDomain";
        private static readonly string DOMAIN_JSONS_LOG_FILE = "DomainJSONsFailures";
        private static readonly string UM_JSONS_LOG_FILE = "UserMediaJSONsFailures";
        private static readonly int MAX_STRINGBUILDER_SIZE = 2048;
        private static readonly int MAX_DB_FAIL_COUNT = 10;
        private static readonly int DEFAULT_NUM_OF_WORKER_THREADS = 4;

        public CouchbaseMediaMarksFeeder()
        {
        }

        private string GetWorkerLogMsg(string msg, int groupID, string outputDirectory, int numOfUsersPerBulk, DateTime fromDate, 
            DateTime toDate, int currIndex, int fromUserInclusive, int toUserInclusive, Exception ex)
        {
            StringBuilder sb = new StringBuilder(String.Concat(msg, ". "));
            sb.Append(String.Concat(" On Thread ID: ", System.Threading.Thread.CurrentThread.ManagedThreadId));
            sb.Append(String.Concat(" Group ID: ", groupID));
            sb.Append(String.Concat(" Output Dir: ", outputDirectory));
            sb.Append(String.Concat(" User Bulk Size: ", numOfUsersPerBulk));
            sb.Append(String.Concat(" From Date: ", fromDate.ToString(DATETIME_PRINT_FORMAT)));
            sb.Append(String.Concat(" To Date: ", toDate.ToString(DATETIME_PRINT_FORMAT)));
            sb.Append(String.Concat(" Curr Index: ", currIndex));
            sb.Append(String.Concat(" From User (Inclusive): ", fromUserInclusive));
            sb.Append(String.Concat(" To User (Inclusive): ", toUserInclusive));
            if (ex != null)
            {
                sb.Append(String.Concat("Exception occurred. Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
            }

            return sb.ToString();
        }

        private bool WorkerDelegate(ChunkManager manager, int groupID, string outputDirectory, int numOfUsersPerBulk,
            DateTime fromDate, DateTime toDate)
        {
            bool res = false;
            bool isLastIterationSuccessful = true;
            int currFailCount = 0;
            int currIndex = -1;
            int from = 0;
            int to = 0;
            bool isTerminateWorker = false;
            for (int i = 0; ; i++)
            {
                try
                {
                    if (isLastIterationSuccessful)
                    {
                        isTerminateWorker = !manager.GetNextOffsets(ref from, ref to, ref currIndex);
                    }
                    else
                    {
                        isTerminateWorker = currFailCount > MAX_DB_FAIL_COUNT;
                    }
                    if (!isTerminateWorker)
                    {
                        Dictionary<int, List<UserMediaMark>> domainIDToMediaMarksMapping = null;
                        Dictionary<UserMediaKey, List<UserMediaMark>> userMediaToMediaMarksMapping = null;
                        List<int> usersWithNoDomain = null;
                        if (CatalogDAL.Get_UMMsToCB(groupID, from, to, numOfUsersPerBulk, fromDate, toDate,
                            ref domainIDToMediaMarksMapping,
                            ref userMediaToMediaMarksMapping, ref usersWithNoDomain))
                        {
                            //Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg(GetSuccessStatusMsg(i, domainIDToMediaMarksMapping, userMediaToMediaMarksMapping,
                            //    usersWithNoDomain), groupID, outputDirectory, numOfUsersPerBulk, fromDate, null), LOG_FILE);
                            Logger.Logger.Log(LOG_HEADER_STATUS, GetWorkerLogMsg(GetSuccessStatusMsg(currIndex, domainIDToMediaMarksMapping,
                                userMediaToMediaMarksMapping, usersWithNoDomain), groupID, outputDirectory, numOfUsersPerBulk,
                                fromDate, toDate, currIndex, from, to, null), LOG_FILE);

                            LogUsersWithNoDomain(currIndex, usersWithNoDomain);
                            int domainFailCount = 0;

                            foreach (KeyValuePair<int, List<UserMediaMark>> kvp in domainIDToMediaMarksMapping)
                            {
                                if (!WriteDomainJSONFile(kvp.Key, kvp.Value, outputDirectory, currIndex))
                                {
                                    domainFailCount++;
                                }
                            } // for

                            if (domainFailCount > 0)
                            {
                                Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("Failed to create {0} domain jsons at iteration num {1} , Refer to log file: {2} for more details.", domainFailCount, currIndex, DOMAIN_JSONS_LOG_FILE), LOG_FILE);
                            }
                            else
                            {
                                Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("Domain JSONS at iteration: ", currIndex, " were created successfully."), LOG_FILE);
                            }

                            int userMediaFailCount = 0;

                            foreach (KeyValuePair<UserMediaKey, List<UserMediaMark>> kvp in userMediaToMediaMarksMapping)
                            {
                                if (!WriteUserMediaJSONFile(kvp.Key, kvp.Value, outputDirectory, currIndex))
                                {
                                    userMediaFailCount++;
                                }
                            } // for

                            if (userMediaFailCount > 0)
                            {
                                string logMsg = string.Format("Failed to create {0} user media jsons at iteration num {1} , Refer to log file: {2} for more details.", userMediaFailCount, currIndex, UM_JSONS_LOG_FILE);
                                Logger.Logger.Log(LOG_HEADER_ERROR, GetWorkerLogMsg(logMsg, groupID, outputDirectory, numOfUsersPerBulk,
                                    fromDate, toDate, currIndex, from, to, null), LOG_FILE);
                            }
                            else
                            {
                                string logMsg = String.Concat("User Media JSONS at iteration: ", currIndex, " were created successfully.");
                                Logger.Logger.Log(LOG_HEADER_STATUS, GetWorkerLogMsg(logMsg, groupID, outputDirectory, numOfUsersPerBulk, fromDate, toDate, currIndex, from, to, null), LOG_FILE);
                            }

                            isLastIterationSuccessful = true;
                            currFailCount = 0;
                        }
                        else
                        {
                            // increment failcount.
                            currFailCount++;
                            isLastIterationSuccessful = false;
                        }
                    }
                    else
                    {
                        // terminated. understand whether it was terminated due to max fail count or due to success
                        if (currFailCount > MAX_DB_FAIL_COUNT)
                        {
                            res = false;
                        }
                        else
                        {
                            res = true; // finished consuming the data.
                        }

                        Logger.Logger.Log(LOG_HEADER_STATUS, GetWorkerLogMsg(String.Concat("Terminating Worker. Success: ", res.ToString().ToLower()),
                            groupID, outputDirectory, numOfUsersPerBulk, fromDate, toDate, currIndex, from, to, null), LOG_FILE);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetWorkerLogMsg("Exception occurred. ", groupID, outputDirectory, numOfUsersPerBulk,
                        fromDate, toDate, currIndex, from, to, ex), LOG_FILE);
                }
            } // for



            return res;
        }

        public bool Execute(int groupID, string outputDirectory, int numOfUsersPerBulk, DateTime from, DateTime to)
        {
            bool res = false;
            bool isTerminate = false;
            ChunkManager manager = null;
            lock (isRunningMutex)
            {
                if (isRunning)
                {
                    isTerminate = true;
                }
                else
                {
                    isRunning = true;
                }
            }

            if (isTerminate)
            {
                Logger.Logger.Log(LOG_HEADER_ERROR, GetLogMsg("Process already running", groupID, outputDirectory, numOfUsersPerBulk, from, to, null), LOG_FILE);
                return false;
            }

            try
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Logger.Logger.Log(LOG_HEADER_ERROR, GetLogMsg("Directory does not exist", groupID, outputDirectory, numOfUsersPerBulk, from, to, null), LOG_FILE);
                    return false;
                }

                Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg("Starting migration process", groupID, outputDirectory, numOfUsersPerBulk, from, to, null), LOG_FILE);
                manager = new ChunkManager();
                if (!manager.Initialize(numOfUsersPerBulk))
                {
                    throw new Exception("Failed to initialize Chunk Manager.");
                }
                Task[] workers = new Task[DEFAULT_NUM_OF_WORKER_THREADS];
                for (int i = 0; i < workers.Length; i++)
                {
                    workers[i] = Task.Factory.StartNew(() => WorkerDelegate(manager, groupID, outputDirectory, numOfUsersPerBulk,
                        from, to));
                }
                Task.WaitAll(workers);
                for (int i = 0; i < workers.Length; i++)
                {
                    if (workers[i] != null)
                    {
                        workers[i].Dispose();
                    }
                }
                Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg("Workers finished processing.", groupID, outputDirectory, numOfUsersPerBulk,
                    from, to, null), LOG_FILE);
                //bool keepRunning = true;
                //int databaseFailCount = 0;
                //for (int i = 0; keepRunning; i++)
                //{
                //    Dictionary<int, List<UserMediaMark>> domainIDToMediaMarksMapping = null;
                //    Dictionary<UserMediaKey, List<UserMediaMark>> userMediaToMediaMarksMapping = null;
                //    List<int> usersWithNoDomain = null;
                //    if (CatalogDAL.Get_UMMsToCB(groupID, from, numOfUsersPerBulk, ref domainIDToMediaMarksMapping,
                //        ref userMediaToMediaMarksMapping, ref usersWithNoDomain))
                //    {
                //        Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg(GetSuccessStatusMsg(i, domainIDToMediaMarksMapping, userMediaToMediaMarksMapping,
                //            usersWithNoDomain), groupID, outputDirectory, numOfUsersPerBulk, from, null), LOG_FILE);

                //        keepRunning = domainIDToMediaMarksMapping.Count > 0 || userMediaToMediaMarksMapping.Count > 0 ||
                //            usersWithNoDomain.Count > 0;

                //        if (!keepRunning)
                //        {
                //            Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg(String.Concat("Breaking loop at iteration num: ", i), groupID, outputDirectory, numOfUsersPerBulk, from, null), LOG_FILE);
                //            break;
                //        }
                //        LogUsersWithNoDomain(i, usersWithNoDomain);
                //        int domainFailCount = 0;


                //        foreach(KeyValuePair<int, List<UserMediaMark>> kvp in domainIDToMediaMarksMapping) 
                //        {
                //            if (!WriteDomainJSONFile(kvp.Key, kvp.Value, outputDirectory, i))
                //            {
                //                domainFailCount++;
                //            }
                //        } // for

                //        if (domainFailCount > 0)
                //        {
                //            Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("Failed to create {0} domain jsons at iteration num {1} , Refer to log file: {2} for more details.", domainFailCount, i, DOMAIN_JSONS_LOG_FILE), LOG_FILE);
                //        }
                //        else
                //        {
                //            Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("Domain JSONS at iteration: ", i, " were created successfully."), LOG_FILE);
                //        }

                //        int userMediaFailCount = 0;

                //        foreach (KeyValuePair<UserMediaKey, List<UserMediaMark>> kvp in userMediaToMediaMarksMapping)
                //        {
                //            if (!WriteUserMediaJSONFile(kvp.Key, kvp.Value, outputDirectory, i))
                //            {
                //                userMediaFailCount++;
                //            }
                //        } // for
                //        if (userMediaFailCount > 0)
                //        {
                //            Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("Failed to create {0} user media jsons at iteration num {1} , Refer to log file: {2} for more details.", userMediaFailCount, i, UM_JSONS_LOG_FILE), LOG_FILE);
                //        }
                //        else
                //        {
                //            Logger.Logger.Log(LOG_HEADER_STATUS, String.Concat("User Media JSONS at iteration: ", i, " were created successfully."), LOG_FILE);
                //        }
                //    }
                //    else
                //    {
                //        // failed to access SQL DB.
                //        Logger.Logger.Log(LOG_HEADER_ERROR, GetLogMsg(String.Concat("Failed to fetch data from DB. Iteration num: ", i, " (Iteration count starts from zero)"),
                //            groupID, outputDirectory, numOfUsersPerBulk, from, null), LOG_FILE);
                //        if (++databaseFailCount > MAX_DB_FAIL_COUNT)
                //        {
                //            Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg("Reached max DB fail count. Terminating process.", groupID, outputDirectory, numOfUsersPerBulk, from, null), LOG_FILE);
                //            break;
                //        }
                //    }
                //} // for

            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception occurred", groupID, outputDirectory, numOfUsersPerBulk, from, to, ex), LOG_FILE);
            }
            finally
            {
                Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg("Main thread finally block. ", groupID, outputDirectory, numOfUsersPerBulk, from, to, null), LOG_FILE);
                if (manager != null)
                {
                    Logger.Logger.Log(LOG_HEADER_STATUS, GetLogMsg("Dropping temp table in SQL DB.", groupID, outputDirectory, numOfUsersPerBulk,
                        from, to, null), LOG_FILE);
                }
                lock (isRunningMutex)
                {
                    if (isRunning)
                    {
                        isRunning = false;
                    }
                }
            }

            return res;
        }

        private bool WriteUserMediaJSONFile(UserMediaKey umk, List<UserMediaMark> devices, string outputDirectory, int iteration)
        {
            bool res = false;
            StreamWriter file = null;
            string outputJson = string.Empty;
            MediaMarkLog mml = new MediaMarkLog() { devices = devices, LastMark = devices.LastOrDefault() };
            string filename = String.Concat(outputDirectory, umk.ToString(), JSON_FILE_ENDING);
            try
            {
                outputJson = JsonConvert.SerializeObject(mml, Formatting.None);
                file = new StreamWriter(filename);
                file.Write(outputJson);
                file.Flush();
                res = true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at WriteUserMediaJSONFile. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Filename: |~~~|", filename));
                sb.Append(String.Concat("|~~~| JSON: |~%~|", outputJson));
                sb.Append(String.Concat("|~%~| At iteration num: ", iteration));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, sb.ToString(), UM_JSONS_LOG_FILE);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            return res;
        }

        private bool WriteDomainJSONFile(int domainID, List<UserMediaMark> devices, string outputDirectory, int iteration)
        {
            bool res = false;
            StreamWriter file = null;
            string outputJson = string.Empty;
            DomainMediaMark dmm = new DomainMediaMark() { domainID = domainID, devices = devices };
            string filename = String.Concat(outputDirectory, dmm.ToString(), JSON_FILE_ENDING);
            try
            {
                outputJson = JsonConvert.SerializeObject(dmm, Formatting.None);
                file = new StreamWriter(filename);
                file.Write(outputJson);
                file.Flush();
                res = true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at WriteDomainJSONFile. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Filename: |~~~|", filename));
                sb.Append(String.Concat("|~~~| DMM JSON: |~%~|", outputJson));
                sb.Append(String.Concat("|~%~| At Iteration Num: ", iteration));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, sb.ToString(), DOMAIN_JSONS_LOG_FILE);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            return res;
        }

        private void LogUsersWithNoDomain(int iteration, List<int> usersWithNoDomain)
        {
            if (usersWithNoDomain == null || usersWithNoDomain.Count == 0)
            {
                Logger.Logger.Log(LOG_HEADER_STATUS, string.Format("No users without domain at iteration: {0} (Iteration count starts from zero)", iteration), LOG_FILE);
            }
            else 
            {
                Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("At iteration num: {0} , encountered {1} users with no domain. Refer to {2} log file. (Iteration count starts from zero)", iteration, usersWithNoDomain.Count, USERS_WITH_NO_DOMAIN_LOG_FILE), LOG_FILE);
                Logger.Logger.Log(LOG_HEADER_ERROR, String.Concat("Starting. Users with no domain at iteration: ", iteration), USERS_WITH_NO_DOMAIN_LOG_FILE);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < usersWithNoDomain.Count; i++)
                {
                    sb.Append(String.Concat(usersWithNoDomain[i], ";"));
                    if (sb.Length > MAX_STRINGBUILDER_SIZE)
                    {
                        Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("Users with no domain at iteration: {0} , users: {1}", iteration, sb.ToString()), USERS_WITH_NO_DOMAIN_LOG_FILE);
                        sb = new StringBuilder();
                    }
                } // for
                if (sb.Length > 0)
                {
                    Logger.Logger.Log(LOG_HEADER_ERROR, string.Format("Users with no domain at iteration: {0} , users: {1}", iteration, sb.ToString()), USERS_WITH_NO_DOMAIN_LOG_FILE);
                }
                Logger.Logger.Log(LOG_HEADER_ERROR, String.Concat("End. Users with no domain at iteration: ", iteration), USERS_WITH_NO_DOMAIN_LOG_FILE);
            }
        }

        private string GetSuccessStatusMsg(int iteration, Dictionary<int, List<UserMediaMark>> domainIDToMediaMarksMapping,
            Dictionary<UserMediaKey, List<UserMediaMark>> userMediaToMediaMarksMapping, List<int> usersWithNoDomain)
        {
            StringBuilder sb = new StringBuilder(String.Concat("Extracted data successfully from DB at iteration num: ", iteration));
            sb.Append(String.Concat(" Domain ID to Media Marks Dictionary size: ", domainIDToMediaMarksMapping.Count));
            sb.Append(String.Concat(" UserMedia to Media Marks Dictionary size: ", userMediaToMediaMarksMapping.Count));
            sb.Append(String.Concat(" Users with no domain count: ", usersWithNoDomain.Count));

            return sb.ToString();
        }

        private string GetLogMsg(string msg, int groupID, string outputDir, int numOfUsersPerBulk, DateTime from, DateTime to, Exception ex)
        {
            StringBuilder sb = new StringBuilder(String.Concat(msg, ". "));
            sb.Append(String.Concat(" G ID: ", groupID));
            sb.Append(String.Concat(" Output Dir: ", outputDir));
            sb.Append(String.Concat(" Num Of Users Per Bulk: ", numOfUsersPerBulk));
            sb.Append(String.Concat(" From: ", from.ToString(DATETIME_PRINT_FORMAT)));
            sb.Append(String.Concat(" To: ", to.ToString(DATETIME_PRINT_FORMAT)));
            if (ex != null)
            {
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
            }
            return sb.ToString();
        }
    }
}
