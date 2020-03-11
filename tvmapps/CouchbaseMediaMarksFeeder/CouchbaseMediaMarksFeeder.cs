using ApiObjects.MediaMarks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using KLogMonitor;
using System.Reflection;

namespace CouchbaseMediaMarksFeeder
{
    public class CouchbaseMediaMarksFeeder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object isRunningMutex = new object();
        private static bool isRunning = false;

        private static readonly string JSON_FILE_ENDING = ".json";
        private static readonly string ZIP_FILE_ENDING = ".zip";
        private static readonly string LOG_FILE = "CouchbaseMediaMarksFeeder";
        private static readonly string DATETIME_PRINT_FORMAT = "yyyyMMddHHmmss";
        private static readonly string LOG_HEADER_STATUS = "Status";
        private static readonly string LOG_HEADER_ERROR = "Error";
        private static readonly string LOG_HEADER_EXCEPTION = "Exception";
        private static readonly string USERS_WITH_NO_DOMAIN_LOG_FILE = "UsersWithNoDomain";
        private static readonly string DOMAIN_JSONS_LOG_FILE = "DomainJSONsFailures";
        private static readonly string UM_JSONS_LOG_FILE = "UserMediaJSONsFailures";
        private static readonly string ZIP_LOG_FILE = "ZipProcess";
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
                        if (CatalogDAL.Get_UMMsToCB(groupID, from, to, fromDate, toDate,
                            ref domainIDToMediaMarksMapping,
                            ref userMediaToMediaMarksMapping, ref usersWithNoDomain))
                        {
                            log.Debug(LOG_HEADER_STATUS + " " + GetWorkerLogMsg(GetSuccessStatusMsg(currIndex, domainIDToMediaMarksMapping,
                                userMediaToMediaMarksMapping, usersWithNoDomain), groupID, outputDirectory, numOfUsersPerBulk,
                                fromDate, toDate, currIndex, from, to, null));

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
                                log.Debug(LOG_HEADER_ERROR + string.Format(" Failed to create {0} domain jsons at iteration num {1} , Refer to log file: {2} for more details.", domainFailCount, currIndex, DOMAIN_JSONS_LOG_FILE));
                            }
                            else
                            {
                                log.Debug(LOG_HEADER_STATUS + String.Concat(" Domain JSONS at iteration: ", currIndex, " were created successfully."));
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
                                log.Debug(LOG_HEADER_ERROR + GetWorkerLogMsg(logMsg, groupID, outputDirectory, numOfUsersPerBulk,
                                    fromDate, toDate, currIndex, from, to, null));
                            }
                            else
                            {
                                string logMsg = String.Concat("User Media JSONS at iteration: ", currIndex, " were created successfully.");
                                log.Debug(LOG_HEADER_STATUS + GetWorkerLogMsg(logMsg, groupID, outputDirectory, numOfUsersPerBulk, fromDate, toDate, currIndex, from, to, null));
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

                        log.Debug(LOG_HEADER_STATUS + GetWorkerLogMsg(String.Concat("Terminating Worker. Success: ", res.ToString().ToLower()),
                            groupID, outputDirectory, numOfUsersPerBulk, fromDate, toDate, currIndex, from, to, null));
                        break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(LOG_HEADER_EXCEPTION + GetWorkerLogMsg("Exception occurred. ", groupID, outputDirectory, numOfUsersPerBulk,
                        fromDate, toDate, currIndex, from, to, ex), ex);
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
                log.Debug(LOG_HEADER_ERROR + GetLogMsg(" Process already running", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                return false;
            }

            try
            {
                outputDirectory = outputDirectory.Trim();
                if (string.IsNullOrEmpty(outputDirectory))
                {
                    log.Debug(LOG_HEADER_ERROR + GetLogMsg(" Invalid directory name", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                    return false;
                }
                if (!outputDirectory.EndsWith("\\"))
                {
                    outputDirectory = String.Concat(outputDirectory, "\\");
                }
                if (!Directory.Exists(outputDirectory))
                {
                    log.Debug(LOG_HEADER_ERROR + GetLogMsg("Directory does not exist", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                    return false;
                }

                log.Debug(LOG_HEADER_STATUS + GetLogMsg(" Starting migration process", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                manager = new ChunkManager(from, to, groupID, numOfUsersPerBulk);
                if (!manager.Initialize())
                {
                    throw new Exception("Failed to initialize Chunk Manager. Refer to ODBC logs.");
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
                log.Debug(LOG_HEADER_STATUS + GetLogMsg(" Workers finished processing.", groupID, outputDirectory, numOfUsersPerBulk,
                    from, to, null));

                res = true;
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_EXCEPTION + GetLogMsg("Exception occurred", groupID, outputDirectory, numOfUsersPerBulk, from, to, ex), ex);
            }
            finally
            {
                log.Debug(LOG_HEADER_STATUS + GetLogMsg(" Main thread finally block. ", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                if (manager != null)
                {
                    log.Debug(LOG_HEADER_STATUS + GetLogMsg(" Dropping temp table in SQL DB.", groupID, outputDirectory, numOfUsersPerBulk,
                        from, to, null));
                    manager.Dispose();
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

        private bool UpdateOrCreateUserMediaJSONFile(UserMediaKey umk, List<UserMediaMark> devices, string outputDirectory, int iteration)
        {
            bool res = false;
            string json = string.Empty;
            string newJson = string.Empty;
            string filename = String.Concat(outputDirectory, umk.ToString(), JSON_FILE_ENDING);
            FileStream file = null;
            try
            {
                if (File.Exists(filename))
                {
                    file = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
                    byte[] fileContent = new byte[file.Length];
                    int numBytesToRead = (int)file.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        int readNow = file.Read(fileContent, numBytesRead, numBytesToRead);
                        if (readNow == 0)
                            break;
                        numBytesRead += readNow;
                        numBytesToRead -= readNow;
                    }
                    json = Encoding.UTF8.GetString(fileContent);
                    MediaMarkLog oldMml = JsonConvert.DeserializeObject<MediaMarkLog>(json);
                    oldMml.LastMark = devices.FirstOrDefault();
                    //oldMml.devices = devices;
                    // flush, in order to move from read operation to write operation
                    file.Flush();
                    file.SetLength(0);
                    // flush again, in order to make sure the file is erased.
                    file.Flush();
                    newJson = JsonConvert.SerializeObject(oldMml, Formatting.None);
                    byte[] dataToWrite = Encoding.UTF8.GetBytes(newJson);
                    file.Write(dataToWrite, 0, dataToWrite.Length);
                    file.Flush();
                    res = true;
                }
                else
                {
                    return WriteUserMediaJSONFile(umk, devices, outputDirectory, iteration);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at UpdateOrCreateUserMediaJSONFile. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Filename: |~~~|", filename));
                sb.Append(String.Concat("|~~~| Old JSON: |~%~|", json));
                sb.Append(String.Concat("|~%~| New JSON: ", newJson));
                sb.Append(String.Concat("|~%~| Iteration: ", iteration));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error(LOG_HEADER_EXCEPTION + sb.ToString(), ex);
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

        private bool UpdateOrCreateDomainJSONFile(int domainID, List<UserMediaMark> devices, string outputDirectory, int iteration)
        {
            bool res = false;
            FileStream file = null;
            string filename = string.Empty;
            string json = string.Empty;
            string newJson = string.Empty;
            try
            {
                filename = String.Concat(outputDirectory, "d", domainID, JSON_FILE_ENDING);
                if (File.Exists(filename))
                {
                    file = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
                    byte[] fileContent = new byte[file.Length];
                    int numBytesToRead = (int)file.Length;
                    int numBytesRead = 0;
                    // read file
                    while (numBytesToRead > 0)
                    {
                        int readNow = file.Read(fileContent, numBytesRead, numBytesToRead);
                        if (readNow == 0)
                            break;
                        numBytesRead += readNow;
                        numBytesToRead -= readNow;
                    }

                    //deserialize json
                    json = Encoding.UTF8.GetString(fileContent);
                    DomainMediaMark dmm = JsonConvert.DeserializeObject<DomainMediaMark>(json);

                    // assuming SUS by this move. In MUS we could lose data.
                    SortedSet<UserMediaMark> set = new SortedSet<UserMediaMark>(dmm.devices, new UserMediaMark.UMMMediaComparer());
                    set.SymmetricExceptWith(devices); // we have here old umms xor new umms
                    // now add here all new umms, so the intersection of old umms and new umms will be up-to-date
                    for (int i = 0; i < devices.Count; i++)
                    {
                        set.Add(devices[i]);
                    }

                    // flush, in order to move from read operation to write operation
                    file.Flush();
                    file.SetLength(0);
                    // flush again, in order to make sure the file is erased.
                    file.Flush();

                    DomainMediaMark newDmms = new DomainMediaMark();
                    newDmms.domainID = domainID;
                    newDmms.devices = set.ToList<UserMediaMark>();
                    newJson = JsonConvert.SerializeObject(newDmms, Formatting.None);

                    byte[] dataToWrite = Encoding.UTF8.GetBytes(newJson);
                    file.Write(dataToWrite, 0, dataToWrite.Length);
                    file.Flush();
                    res = true;

                }
                else
                {
                    // domain media mark does not exist. create new one.
                    return WriteDomainJSONFile(domainID, devices, outputDirectory, iteration);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at UpdateOrCreateDomainJSONFile. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Filename: |~~~|", filename));
                sb.Append(String.Concat("|~~~| Old DMM JSON: |~%~|", json));
                sb.Append(String.Concat("|~%~| New DMM JSON: ", newJson));
                sb.Append(String.Concat("|~%~| At Iteration Num: ", iteration));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error(LOG_HEADER_EXCEPTION + sb.ToString(), ex);
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

        private bool UpdateWorkerDelegate(ChunkManager manager, int groupID, string outputDirectory, int numOfUsersPerBulk,
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
                        if (CatalogDAL.Get_UMMsToCB(groupID, from, to, fromDate, toDate,
                            ref domainIDToMediaMarksMapping,
                            ref userMediaToMediaMarksMapping, ref usersWithNoDomain))
                        {
                            log.Debug(LOG_HEADER_STATUS + GetWorkerLogMsg(GetSuccessStatusMsg(currIndex, domainIDToMediaMarksMapping,
                                userMediaToMediaMarksMapping, usersWithNoDomain), groupID, outputDirectory, numOfUsersPerBulk,
                                fromDate, toDate, currIndex, from, to, null));

                            LogUsersWithNoDomain(currIndex, usersWithNoDomain);
                            int domainFailCount = 0;

                            foreach (KeyValuePair<int, List<UserMediaMark>> kvp in domainIDToMediaMarksMapping)
                            {

                                if (!UpdateOrCreateDomainJSONFile(kvp.Key, kvp.Value, outputDirectory, currIndex))
                                {
                                    domainFailCount++;
                                }
                            } // for

                            if (domainFailCount > 0)
                            {
                                log.Debug(LOG_HEADER_ERROR + string.Format("Update. Failed to create {0} domain jsons at iteration num {1} , Refer to log file: {2} for more details.", domainFailCount, currIndex, DOMAIN_JSONS_LOG_FILE));
                            }
                            else
                            {
                                log.Debug(LOG_HEADER_STATUS + String.Concat(" Update. Domain JSONS at iteration: ", currIndex, " were created successfully."));
                            }

                            int userMediaFailCount = 0;

                            foreach (KeyValuePair<UserMediaKey, List<UserMediaMark>> kvp in userMediaToMediaMarksMapping)
                            {
                                if (!UpdateOrCreateUserMediaJSONFile(kvp.Key, kvp.Value, outputDirectory, currIndex))
                                {
                                    userMediaFailCount++;
                                }
                            } // for

                            if (userMediaFailCount > 0)
                            {
                                string logMsg = string.Format("Update. Failed to create {0} user media jsons at iteration num {1} , Refer to log file: {2} for more details.", userMediaFailCount, currIndex, UM_JSONS_LOG_FILE);
                                log.Debug(LOG_HEADER_ERROR + GetWorkerLogMsg(logMsg, groupID, outputDirectory, numOfUsersPerBulk,
                                    fromDate, toDate, currIndex, from, to, null));
                            }
                            else
                            {
                                string logMsg = String.Concat("Update. User Media JSONS at iteration: ", currIndex, " were created successfully.");
                                log.Debug(LOG_HEADER_STATUS + GetWorkerLogMsg(logMsg, groupID, outputDirectory, numOfUsersPerBulk, fromDate, toDate, currIndex, from, to, null));
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

                        log.Debug(LOG_HEADER_STATUS + GetWorkerLogMsg(String.Concat(" Update. Terminating Worker. Success: ", res.ToString().ToLower()),
                            groupID, outputDirectory, numOfUsersPerBulk, fromDate, toDate, currIndex, from, to, null));
                        break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(LOG_HEADER_EXCEPTION + GetWorkerLogMsg(" Update. Exception occurred. ", groupID, outputDirectory, numOfUsersPerBulk,
                        fromDate, toDate, currIndex, from, to, ex), ex);
                }
            } // for



            return res;
        }

        public bool Update(int groupID, string outputDirectory, int numOfUsersPerBulk, DateTime from, DateTime to)
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
                log.Debug(LOG_HEADER_ERROR + GetLogMsg("Update. Process already running", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                return false;
            }
            try
            {
                outputDirectory = outputDirectory.Trim();
                if (string.IsNullOrEmpty(outputDirectory))
                {
                    log.Debug(LOG_HEADER_ERROR + GetLogMsg("Invalid directory name", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                    return false;
                }
                if (!outputDirectory.EndsWith("\\"))
                {
                    outputDirectory = String.Concat(outputDirectory, "\\");
                }
                if (!Directory.Exists(outputDirectory))
                {
                    log.Debug(LOG_HEADER_ERROR + GetLogMsg("Update. Directory does not exist", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                    return false;
                }
                manager = new ChunkManager(from, to, groupID, numOfUsersPerBulk);
                if (!manager.Initialize())
                {
                    throw new Exception("Failed to initialize Chunk Manager. Refer to ODBC logs.");
                }
                Task[] workers = new Task[DEFAULT_NUM_OF_WORKER_THREADS];
                for (int i = 0; i < workers.Length; i++)
                {
                    workers[i] = Task.Factory.StartNew(() => UpdateWorkerDelegate(manager, groupID, outputDirectory, numOfUsersPerBulk,
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
                log.Debug(LOG_HEADER_STATUS + GetLogMsg(" Update. Workers finished processing.", groupID, outputDirectory, numOfUsersPerBulk,
                    from, to, null));

                res = true;
            }
            catch (Exception ex)
            {
                log.Error(LOG_HEADER_ERROR + GetLogMsg(" Exception at Update. ", groupID, outputDirectory, numOfUsersPerBulk,
                    from, to, ex), ex);
            }
            finally
            {
                log.Debug(LOG_HEADER_STATUS + GetLogMsg("Update. Main thread finally block. ", groupID, outputDirectory, numOfUsersPerBulk, from, to, null));
                if (manager != null)
                {
                    log.Debug(LOG_HEADER_STATUS + GetLogMsg("Update. Dropping temp table in SQL DB.", groupID, outputDirectory, numOfUsersPerBulk,
                        from, to, null));
                    manager.Dispose();
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
            MediaMarkLog mml = new MediaMarkLog()
            {
                //devices = devices,
                LastMark = devices.FirstOrDefault()
            };
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
                log.Error(LOG_HEADER_EXCEPTION + sb.ToString(), ex);
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
                log.Error(LOG_HEADER_EXCEPTION + sb.ToString(), ex);
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
                log.Debug(LOG_HEADER_STATUS + string.Format(" No users without domain at iteration: {0} (Iteration count starts from zero)", iteration));
            }
            else
            {
                log.Debug(LOG_HEADER_ERROR + string.Format(" At iteration num: {0} , encountered {1} users with no domain. Refer to {2} log file. (Iteration count starts from zero)", iteration, usersWithNoDomain.Count, USERS_WITH_NO_DOMAIN_LOG_FILE));
                log.Debug(LOG_HEADER_ERROR + String.Concat("Starting. Users with no domain at iteration: ", iteration));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < usersWithNoDomain.Count; i++)
                {
                    sb.Append(String.Concat(usersWithNoDomain[i], ";"));
                    if (sb.Length > MAX_STRINGBUILDER_SIZE)
                    {
                        log.Debug(LOG_HEADER_ERROR + string.Format("Users with no domain at iteration: {0} , users: {1}", iteration, sb.ToString()));
                        sb = new StringBuilder();
                    }
                } // for
                if (sb.Length > 0)
                {
                    log.Debug(LOG_HEADER_ERROR + string.Format("Users with no domain at iteration: {0} , users: {1}", iteration, sb.ToString()));
                }
                log.Debug(LOG_HEADER_ERROR + String.Concat("End. Users with no domain at iteration: ", iteration));
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

        public bool Zip(int numOfCouchbaseInstances, string jsonsDirectory, string zipsDirectory, int numOfJsonsInZip)
        {
            bool isTerminate = false;
            bool res = false;

            if (string.IsNullOrEmpty(jsonsDirectory) || string.IsNullOrEmpty(zipsDirectory) || numOfCouchbaseInstances < 1
                || numOfCouchbaseInstances < 1)
            {
                log.Debug(LOG_HEADER_ERROR + " Zip. Input is invalid.");
                return false;
            }

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
                log.Debug(LOG_HEADER_ERROR + " Different Execute/Update/Zip is already running");
                return false;
            }
            if (!jsonsDirectory.EndsWith("\\"))
            {
                jsonsDirectory = String.Concat(jsonsDirectory, "\\");
            }
            if (!zipsDirectory.EndsWith("\\"))
            {
                zipsDirectory = String.Concat(zipsDirectory, "\\");
            }

            try
            {
                log.Debug(LOG_HEADER_STATUS + " Zip. Entering try block.");
                if (!Directory.Exists(jsonsDirectory) || !Directory.Exists(zipsDirectory))
                {
                    log.Error(LOG_HEADER_ERROR + " Either directory of jsons or target directory of zips does not exist.");
                    return false;
                }
                Task[] domainWorkers = null;
                Task[] umWorkers = null;
                string[] domainsJsonsDir = null;
                string[] umJsonsDir = null;
                CreateZipDirectories(numOfCouchbaseInstances, zipsDirectory, out domainsJsonsDir, out umJsonsDir);
                log.Debug(LOG_HEADER_STATUS + " Created directories for each couchbase instance.");
                // get domains json files
                string[] domainsJsonsFiles = Directory.GetFiles(jsonsDirectory, "d*");
                if (domainsJsonsFiles != null && domainsJsonsFiles.Length > 0)
                {
                    log.Debug(LOG_HEADER_STATUS + String.Concat(" Found: ", domainsJsonsFiles.Length, " domain jsons files."));
                    domainWorkers = new Task[DEFAULT_NUM_OF_WORKER_THREADS >> 1];
                    domainWorkers[0] = Task.Factory.StartNew(() => ZipperWorker(domainsJsonsFiles, 0, domainsJsonsFiles.Length >> 1,
                        numOfCouchbaseInstances, domainsJsonsDir, numOfJsonsInZip));
                    domainWorkers[1] = Task.Factory.StartNew(() => ZipperWorker(domainsJsonsFiles, domainsJsonsFiles.Length >> 1,
                        domainsJsonsFiles.Length, numOfCouchbaseInstances, domainsJsonsDir, numOfJsonsInZip));
                }
                else
                {
                    log.Debug(LOG_HEADER_ERROR + " No domains jsons were found.");
                }
                // attach two worker threads to process the domains jsons.

                // get user-media json files
                string[] umJsonsFiles = Directory.GetFiles(jsonsDirectory, "u*m*");
                if (umJsonsFiles != null && umJsonsFiles.Length > 0)
                {
                    log.Debug(LOG_HEADER_STATUS + String.Concat(" Found: ", umJsonsFiles.Length, " user media jsons files."));
                    umWorkers = new Task[DEFAULT_NUM_OF_WORKER_THREADS >> 1];
                    umWorkers[0] = Task.Factory.StartNew(() => ZipperWorker(umJsonsFiles, 0, umJsonsFiles.Length >> 1, numOfCouchbaseInstances,
                        umJsonsDir, numOfJsonsInZip));
                    umWorkers[1] = Task.Factory.StartNew(() => ZipperWorker(umJsonsFiles, umJsonsFiles.Length >> 1, umJsonsFiles.Length, numOfCouchbaseInstances,
                        umJsonsDir, numOfJsonsInZip));
                }
                else
                {
                    log.Debug(LOG_HEADER_ERROR + " No domains jsons were found.");
                }

                if (domainWorkers != null)
                {
                    Task.WaitAll(domainWorkers);
                    for (int i = 0; i < domainWorkers.Length; i++)
                    {
                        if (domainWorkers[i] != null)
                        {
                            domainWorkers[i].Dispose();
                        }
                    }
                }
                if (umWorkers != null)
                {
                    Task.WaitAll(umWorkers);
                    for (int i = 0; i < umWorkers.Length; i++)
                    {
                        if (umWorkers[i] != null)
                        {
                            umWorkers[i].Dispose();
                        }
                    }
                }
                res = true;
            }
            catch (Exception ex)
            {
                res = false;
                StringBuilder sb = new StringBuilder(String.Concat("Exception at Zip. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Num of CB instances: ", numOfCouchbaseInstances));
                sb.Append(String.Concat(" JSONs Dir: ", jsonsDirectory));
                sb.Append(String.Concat(" Zips Dir: ", zipsDirectory));
                sb.Append(String.Concat(" Num Of JSONs in Zip: ", numOfJsonsInZip));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error(LOG_HEADER_EXCEPTION + sb.ToString(), ex);
            }
            finally
            {
                log.Debug(LOG_HEADER_STATUS + " Entered finally block at Zip method.");
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

        private bool ZipperWorker(string[] jsonsFilenames, int startIndexInclusive, int endIndexExclusive, int numOfCouchbaseInstances,
            string[] jsonsDir, int numOfJsonsInZip)
        {
            int i = 0;
            int roundRobinCounter = 0;
            int jsonFileInZipCounter = 0;
            bool res = false;
            FileStream zip = null;
            ZipArchive archive = null;
            try
            {
                log.Debug(LOG_HEADER_STATUS + GetZipperWorkerLogMsg("Worker thread started.", startIndexInclusive, endIndexExclusive, null));
                List<KeyValuePair<string, string>> filenameToJsonMapping = new List<KeyValuePair<string, string>>(numOfJsonsInZip);
                for (i = startIndexInclusive; i < endIndexExclusive; i++)
                {
                    if (filenameToJsonMapping.Count == numOfJsonsInZip)
                    {
                        // create zip file.
                        string statusMsg = string.Empty;
                        if (!CreateZipFile(numOfCouchbaseInstances, roundRobinCounter, filenameToJsonMapping, jsonsDir,
                            startIndexInclusive, endIndexExclusive, ref statusMsg))
                        {
                            // fail.
                            log.Error(LOG_HEADER_ERROR + statusMsg);
                        }
                        else
                        {
                            log.Debug(LOG_HEADER_STATUS + String.Concat(" Success. ", statusMsg));
                        }
                        // increment round robin counter
                        roundRobinCounter++;
                        // allocate new list.
                        filenameToJsonMapping = new List<KeyValuePair<string, string>>(numOfJsonsInZip);
                    }
                    string errMsg = string.Empty;
                    string jsonFile = GetFilenameOutOfPath(jsonsFilenames[i]);
                    string jsonFileContent = GetJsonFileContent(jsonsFilenames[i], ref errMsg);
                    if (jsonFileContent.Length == 0)
                    {
                        log.Error(LOG_HEADER_ERROR + GetZipperWorkerLogMsg(String.Concat(" Failed to read json out of file: ", jsonsFilenames[i], " Iter num: ", i), startIndexInclusive, endIndexExclusive, null));
                        continue;
                    }
                    filenameToJsonMapping.Add(new KeyValuePair<string, string>(jsonFile, jsonFileContent));

                }
                if (filenameToJsonMapping.Count > 0)
                {
                    // create zip file.
                    string statusMsg = string.Empty;
                    if (!CreateZipFile(numOfCouchbaseInstances, roundRobinCounter, filenameToJsonMapping, jsonsDir,
                        startIndexInclusive, endIndexExclusive, ref statusMsg))
                    {
                        // fail.
                        log.Error(LOG_HEADER_ERROR + statusMsg);
                    }
                    else
                    {
                        log.Debug(LOG_HEADER_STATUS + String.Concat(" Success. ", statusMsg));
                    }
                    // increment round robin counter
                    roundRobinCounter++;
                }
                res = true;
            }
            catch (Exception ex)
            {
                res = false;
                StringBuilder sb = new StringBuilder(String.Concat("Exception at ZipperWorker. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Iter Num: ", i));
                sb.Append(String.Concat(" Thread ID: ", System.Threading.Thread.CurrentThread.ManagedThreadId));
                sb.Append(String.Concat(" startIndex: ", startIndexInclusive));
                sb.Append(String.Concat(" endIndex: ", endIndexExclusive));
                sb.Append(String.Concat(" Num of CB instances: ", numOfCouchbaseInstances));
                sb.Append(String.Concat(" Num Of Jsons In Zip: ", numOfJsonsInZip));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error(LOG_HEADER_EXCEPTION + sb.ToString(), ex);
            }
            finally
            {
                log.Debug(LOG_HEADER_STATUS + GetZipperWorkerLogMsg(" Worker thread reached finally block.", startIndexInclusive, endIndexExclusive, null));
                if (archive != null)
                {
                    archive.Dispose();
                }
                if (zip != null)
                {
                    zip.Close();
                }
            }

            return res;
        }

        private bool CreateZipFile(int numOfCouchbaseInstances, int roundRobinCounter, List<KeyValuePair<string, string>> jsonFilenameToJsonContent,
            string[] jsonsDir, int startIndexInclusive, int endIndexExclusive, ref string statusMsg)
        {
            bool res = false;
            string zipFilename = String.Concat(jsonsDir[roundRobinCounter % numOfCouchbaseInstances], "\\", startIndexInclusive, "_", endIndexExclusive, "_", Guid.NewGuid().ToString().Replace("-", string.Empty), ZIP_FILE_ENDING);
            FileStream zip = null;
            ZipArchive archive = null;
            int i = 0;
            try
            {
                statusMsg = string.Format("Starting to zip chunk num: {0} startIndex: {1} , endIndex: {2} , num of files: {3} , into file: {4} , Thread ID: {5}", roundRobinCounter, startIndexInclusive, endIndexExclusive, jsonFilenameToJsonContent.Count, zipFilename, System.Threading.Thread.CurrentThread.ManagedThreadId);
                zip = new FileStream(zipFilename, FileMode.CreateNew);
                archive = new ZipArchive(zip, ZipArchiveMode.Create);
                for (i = 0; i < jsonFilenameToJsonContent.Count; i++)
                {
                    string name = jsonFilenameToJsonContent[i].Key;
                    string json = jsonFilenameToJsonContent[i].Value;
                    ZipArchiveEntry entry = archive.CreateEntry(name);
                    using (StreamWriter sw = new StreamWriter(entry.Open()))
                    {
                        sw.Write(json);
                    }
                }
                res = true;
            }
            catch (Exception ex)
            {
                res = false;
                statusMsg = String.Concat(statusMsg, " || ", GetZipperWorkerLogMsg(String.Concat("Exception at iter: ", i, " in CreateZipFile."), startIndexInclusive, endIndexExclusive, ex));
                log.Error(statusMsg, ex);
            }
            finally
            {
                if (archive != null)
                {
                    archive.Dispose();
                }
                if (zip != null)
                {
                    zip.Close();
                }
            }

            return res;
        }

        private string GetFilenameOutOfPath(string path)
        {
            int i = 0;
            for (i = path.Length - 1; i > -1; i--)
            {
                if (path[i] == '\\')
                    break;
            }
            return path.Substring(++i);
        }

        private string GetZipperWorkerLogMsg(string msg, int startIndex, int endIndex, Exception ex)
        {
            StringBuilder sb = new StringBuilder(String.Concat(msg, "."));
            sb.Append(String.Concat(" Thread ID: ", System.Threading.Thread.CurrentThread.ManagedThreadId));
            sb.Append(String.Concat(" SI: ", startIndex));
            sb.Append(String.Concat(" EI: ", endIndex));
            if (ex != null)
            {
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
            }

            return sb.ToString();
        }

        private string GetJsonFileContent(string path, ref string errorMsg)
        {
            string res = string.Empty;
            FileStream file = null;
            try
            {
                file = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] fileContent = new byte[file.Length];
                int numBytesToRead = (int)file.Length;
                int numBytesRead = 0;
                // read file
                while (numBytesToRead > 0)
                {
                    int readNow = file.Read(fileContent, numBytesRead, numBytesToRead);
                    if (readNow == 0)
                        break;
                    numBytesRead += readNow;
                    numBytesToRead -= readNow;
                }

                //deserialize json
                res = Encoding.UTF8.GetString(fileContent);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at GetJsonFileContent. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Path: ", path));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                errorMsg = sb.ToString();
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

        private void CreateZipDirectories(int numOfCouchbaseInstances, string zipsDirectory, out string[] domainJsonsDirs,
            out string[] umJsonsDir)
        {
            domainJsonsDirs = new string[numOfCouchbaseInstances];
            umJsonsDir = new string[numOfCouchbaseInstances];
            for (int i = 0; i < numOfCouchbaseInstances; i++)
            {
                domainJsonsDirs[i] = String.Concat(zipsDirectory, "cb_", i + 1, "_d");
                DirectoryInfo di1 = Directory.CreateDirectory(domainJsonsDirs[i]);
                umJsonsDir[i] = String.Concat(zipsDirectory, "cb_", i + 1, "_um");
                DirectoryInfo di2 = Directory.CreateDirectory(umJsonsDir[i]);
            }

        }
    }
}
