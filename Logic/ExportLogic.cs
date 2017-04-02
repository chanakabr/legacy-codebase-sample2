using ApiObjects;
using ApiObjects.BulkExport;
using Core.Api;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace APILogic
{
    public class ExportLogic
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static object lockObject = new object();

        private static string exportBasePath = TVinciShared.WS_Utils.GetTcmConfigValue("export.base_path");
        private static string exportPathFormat = TVinciShared.WS_Utils.GetTcmConfigValue("export.path_format"); // {0}/{1}/{2}
        private static string exportFileNameFormat = TVinciShared.WS_Utils.GetTcmConfigValue("export.file_name_format"); // {0}_{1}.xml
        private static string exportFileNameDateFormat = TVinciShared.WS_Utils.GetTcmConfigValue("export.file_name_date_format"); // yyyyMMddHHmmss
        private static int maxAssetsPerTask = TVinciShared.WS_Utils.GetTcmIntValue("export.max_assets_per_thread");
        private static int maxTasks = TVinciShared.WS_Utils.GetTcmIntValue("export.max_threads");
        private static int innerTaskRetriesLimit = TVinciShared.WS_Utils.GetTcmIntValue("export.thread_retry_limit");

        private delegate bool DoTaskJob(int groupId, long taskId, List<long> ids, string exportFullPath, string mainLang, int firstTaskIndex, int numberOfTasks, int index, int retrisCount = 0);

        public static bool Export(int groupId, BulkExportTask task, out string filename)
        {
            filename = null;

            List<long> ids = new List<long>();
            
            DataSet unactiveAssets = null;

            // Get active media ids from catalog with consideration of the task filter KSSQL and the asset last update date.
            ids = GetAssetsIdsByFilter(groupId, task.Filter, task.DataType, task.VodTypes, task.ExportType == eBulkExportExportType.Incremental ? task.LastProcess : null);

            // if export type is incremental - get all deleted / not active since the last task process
            if (task.ExportType == eBulkExportExportType.Incremental && task.LastProcess != null)
            {
                // get the ids of the media that had been deleted/ unactivated since last process time
                switch (task.DataType)
                {
                    case eBulkExportDataType.VOD:
                        unactiveAssets = ApiDAL.GetNotActiveMedia(groupId, task.LastProcess.Value);
                        break;
                    case eBulkExportDataType.EPG:
                        unactiveAssets = ApiDAL.GetNotActivePrograms(groupId, task.LastProcess.Value);
                        break;
                }
            }

            // if both lists are empty - no need to proceed - task is done 
            if ((ids == null || ids.Count == 0) &&
                (unactiveAssets == null || unactiveAssets.Tables == null || unactiveAssets.Tables.Count == 0 || unactiveAssets.Tables[0] == null || unactiveAssets.Tables[0].Rows == null || unactiveAssets.Tables[0].Rows.Count == 0))
            {
                return true;
            }

            switch (task.DataType)
            {
                case eBulkExportDataType.VOD:
                    return ExportVod(groupId, ids, unactiveAssets, task.Id, task.ExternalKey, out filename);
                case eBulkExportDataType.EPG:
                    return ExportEpg(groupId, ids, unactiveAssets, task.Id, task.ExternalKey, out filename);
            }

            return false;
        }

        private static bool ExportVod(int groupId, List<long> updatedAssetsIds, DataSet unactiveAssets, long taskId, string externalKey, out string filename)
        {
            filename = null; 

            // build the full path for saving the export file   
            string exportVodBasePath = string.Format(exportPathFormat, exportBasePath, groupId, "VOD");
            string exportVodFullPath = string.Format("{0}/{1}", exportVodBasePath, string.Format(exportFileNameFormat, DateTime.UtcNow.ToString(exportFileNameDateFormat), externalKey));

            try
            {
                // get main language
                string mainLang = APILogic.Utils.GetGroupDefaultLanguageCode(groupId);

                // create output directory
                string xmlDirectory = Path.GetDirectoryName(exportVodFullPath);
                if (!Directory.Exists(xmlDirectory))
                    Directory.CreateDirectory(xmlDirectory);

                // create the xml with the opening tags
                File.AppendAllText(exportVodFullPath, "<feed><export>");

                // if there are updated assets - get the media objects from catalog and append them to the xml
                if (updatedAssetsIds != null && updatedAssetsIds.Count > 0)
                {
                    RunExportTasks(groupId, updatedAssetsIds, taskId, exportVodFullPath, mainLang, DoExportUpdatedMediaJob);
                }

                // if there are not active / deleted media - the build and append the xml
                if (unactiveAssets != null && unactiveAssets.Tables != null && unactiveAssets.Tables.Count > 0)
                {
                    ExportUnactiveMedia(unactiveAssets, exportVodFullPath);
                }

                // append the ending tags of the xml
                File.AppendAllText(exportVodFullPath, "</export></feed>");

                // compress the file
                if (Utils.CompressFile(exportVodFullPath, exportVodBasePath))
                {
                    filename = Path.GetFileNameWithoutExtension(exportVodFullPath);
                }

                // delete the not compressed xml file
                File.Delete(exportVodFullPath);

            }
            catch (Exception ex)
            {
                // delete the created file
                try
                {
                    File.Delete(exportVodFullPath);
                }
                catch (Exception innerEx)
                {
                    log.Error(string.Format("Export VOD: error on removing file after error in export process. task id = {0}, file full path = {1}", taskId, exportVodFullPath), innerEx);
                }

                log.Error(string.Format("Export VOD: error in XML creation process, file has been removed. task id = {0}", taskId), ex);
                return false;
            }

            return true;
        }

        private static bool ExportEpg(int groupId, List<long> updatedAssetsIds, DataSet unactiveAssets, long taskId, string externalKey, out string filename)
        {
            filename = null;

            // build the full path for saving the export file   
            string exportEpgBasePath = string.Format(exportPathFormat, exportBasePath, groupId, "EPG");
            string exportEpgFullPath = string.Format("{0}/{1}", exportEpgBasePath, string.Format(exportFileNameFormat, DateTime.UtcNow.ToString(exportFileNameDateFormat), externalKey));

            try
            {
                // get main language
                string mainLang = APILogic.Utils.GetGroupDefaultLanguageCode(groupId);

                // create output directory
                string xmlDirectory = Path.GetDirectoryName(exportEpgFullPath);
                if (!Directory.Exists(xmlDirectory))
                    Directory.CreateDirectory(xmlDirectory);

                // create the xml with the opening tags
                string xml = string.Format("<EpgChannels xmlns=\"http://tempuri.org/xmltv\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\""
                + " parent-group-id=\"{0}\" group-id=\"{1}\" main-lang=\"{2}\" updater-id=\"0\">", 
                groupId,    //{0} - parent group id
                groupId,    //{1} - group_id
                mainLang    //{2} - main lang
                );
                File.AppendAllText(exportEpgFullPath, xml);

                // if there are updated assets - get the media objects from catalog and append them to the xml
                if (updatedAssetsIds != null && updatedAssetsIds.Count > 0)
                {
                    RunExportTasks(groupId, updatedAssetsIds, taskId, exportEpgFullPath, mainLang, DoExportUpdatedEpgJob);
                }

                // if there are not active / deleted media - the build and append the xml
                if (unactiveAssets != null && unactiveAssets.Tables != null && unactiveAssets.Tables.Count > 0)
                {
                    ExportUnactiveEpg(unactiveAssets, exportEpgFullPath);
                }

                // append the ending tags of the xml
                File.AppendAllText(exportEpgFullPath, "</EpgChannels>");

                // compress the file
                if (Utils.CompressFile(exportEpgFullPath, exportEpgBasePath))
                {
                    filename = Path.GetFileNameWithoutExtension(exportEpgFullPath);
                }

                // delete the not compressed xml file
                File.Delete(exportEpgFullPath);

            }
            catch (Exception ex)
            {
                // delete the created file
                try
                {
                    File.Delete(exportEpgFullPath);
                }
                catch (Exception innerEx)
                {
                    log.Error(string.Format("Export VOD: error on removing file after error in export process. task id = {0}, file full path = {1}", taskId, exportEpgFullPath), innerEx);
                }

                log.Error(string.Format("Export VOD: error in XML creation process, file has been removed. task id = {0}", taskId), ex);
                return false;
            }

            return true;
        }

        private static void ExportUnactiveMedia(DataSet unactiveAssets, string exportFullPath)
        {
            StringBuilder xml = new StringBuilder();

            DataTable table = unactiveAssets.Tables[0];

            // Run on first table and create initial list of parental rules, without tag values
            if (table != null && table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    long mediaId = ODBCWrapper.Utils.ExtractValue<long>(row, "ID");
                    if (mediaId > 0)
                    {

                        xml.Append("<media ");

                        // attributes
                        xml.AppendFormat("co_guid=\"{0}\" entry_id=\"{1}\" action=\"{2}\" is_active=\"{3}\" erase=\"false\" media_id=\"{4}\">",
                             TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetSafeStr(row, "CO_GUID"), true),                                             // {0} - co guid
                             TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetSafeStr(row, "ENTRY_ID"), true),                                            // {1} - entryId
                             TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetIntSafeVal(row, "STATUS") == 2 ? "delete" : "update", true),                // {2} - action
                             TVinciShared.ProtocolsFuncs.XMLEncode((ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ACTIVE") == 1 ? true : false).ToString(), true),       // {3} - is active
                             TVinciShared.ProtocolsFuncs.XMLEncode(mediaId.ToString(), true)                                                                        // {4} - media id
                        );

                        xml.Append("</media>");
                    }
                }

                File.AppendAllText(exportFullPath, xml.ToString());
            }
        }

        private static void ExportUnactiveEpg(DataSet unactiveAssets, string exportFullPath)
        {
            StringBuilder xml = new StringBuilder();

            DataTable table = unactiveAssets.Tables[0];

            // Run on first table and create initial list of parental rules, without tag values
            if (table != null && table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    // programme
                    xml.AppendFormat("<programme start=\"{0}\" stop=\"{1}\" channel=\"{2}\" external_id=\"{3}\" action=\"{4}\" id=\"{5}\"></programme>",
                        TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetSafeStr(row, "START_DATE"), true),           // {0} - start
                        TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetSafeStr(row, "END_DATE"), true),             // {1} - stop
                        TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetSafeStr(row, "EPG_CHANNEL_ID"), true),       // {2} - channel
                        TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetSafeStr(row, "EPG_IDENTIFIER"), true),       // {3} - external_id
                        TVinciShared.ProtocolsFuncs.XMLEncode("delete", true),                                                  // {4} - action
                        TVinciShared.ProtocolsFuncs.XMLEncode(ODBCWrapper.Utils.GetSafeStr(row, "ID"), true)                    // {5} - id
                    );
                }

                File.AppendAllText(exportFullPath, xml.ToString());
            }
        }

        private static void RunExportTasks(int groupId, List<long> mediaIds, long taskId, string exportFullPath, string mainLang, DoTaskJob doTaskJob)
        {
            // calculate the number of full parts to run by each thread 
            int totalNumberOfParts = mediaIds.Count / (maxAssetsPerTask * maxTasks);

            // calculate the remaining part
            int remainPart = mediaIds.Count % (maxAssetsPerTask * maxTasks);

            int firstTaskIndex = 0;

            // process the full parts
            for (int i = 0; i < totalNumberOfParts; i++)
            {
                // calculate what is the number of the first task in current loop step
                firstTaskIndex = i * (maxTasks * maxAssetsPerTask);

                StartExportJobs(groupId, mediaIds, taskId, exportFullPath, mainLang, maxTasks, firstTaskIndex, doTaskJob);
            }

            // process the full remain part
            if (remainPart > 0)
            {
                // if the remain part is not the only part - calculate the start index
                if (totalNumberOfParts > 0)
                {
                    firstTaskIndex += (maxTasks * maxAssetsPerTask);
                }

                // calculate the number of tasks required
                int numberOfTasks = remainPart / maxAssetsPerTask;
                int remainTask = remainPart % maxAssetsPerTask;

                if (remainTask > 0)
                    numberOfTasks++;

                StartExportJobs(groupId, mediaIds, taskId, exportFullPath, mainLang, numberOfTasks, firstTaskIndex, doTaskJob);
            }
        }

        private static void StartExportJobs(int groupId, List<long> mediaIds, long taskId, string exportFullPath, string mainLang, int numberOfTasks, int firstTaskIndex, DoTaskJob doTaskJob)
        {
            Task[] tasks;
            // create tasks array
            tasks = new Task[numberOfTasks];

            // start current tasks bulk
            for (int i = 0; i < numberOfTasks; i++)
            {
                int index = i;
                tasks[i] = Task.Factory.StartNew(() =>
                    doTaskJob(groupId, taskId, mediaIds, exportFullPath, mainLang, firstTaskIndex, numberOfTasks, index));

            }
            Task.WaitAll(tasks);
        }

        private static bool DoExportUpdatedEpgJob(int groupId, long taskId, List<long> programIds, string exportFullPath, string mainLang, int loopStartIndex, int tasksCount, int taskIndex, int retrisCount = 0)
        {
            // calculate the start index of the media ids array 
            int startIndex = loopStartIndex + (taskIndex * maxAssetsPerTask);

            // calculate the number of ids to export 
            int numberOfIds = startIndex + maxAssetsPerTask <= programIds.Count ? maxAssetsPerTask : programIds.Count - startIndex; 

            try
            {
                ProgramObj[] programs;
                if (tasksCount == 1)
                {
                    programs = GetProgramsByIds(programIds, groupId);
                }
                else
                {
                    var currentIdsRange = programIds.GetRange(startIndex, numberOfIds);
                    
                    // get programs from catalog by ids (only the calculated range of program ids)
                    programs = GetProgramsByIds(currentIdsRange, groupId);
                }

                // if no programs returned - retry / fail the current bulk and continue the export
                if (programs == null || programs.Length == 0)
                {
                    throw new Exception(string.Format("failed to get media objects from catalog for task id = {0}, {1} medias from index = {2}", taskId, numberOfIds, startIndex));
                }

                // build the Programme list for all the retrieved programs
                StringBuilder xml = new StringBuilder();
                foreach (var program in programs)
                {
                    if (program != null)
                    {
                        xml.Append(BuildSingleProgramXml(program, mainLang, taskId));
                    }
                }

                // append the created xml to the export xml file
                lock (lockObject)
                {
                    File.AppendAllText(exportFullPath, xml.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("error in DoExportUpdatedEpgJob. task id = {0}, retry number {1}", taskId, retrisCount), ex);

                // if not exceeded retries limit - try again
                if (retrisCount < innerTaskRetriesLimit)
                {
                    return DoExportUpdatedEpgJob(groupId, taskId, programIds, exportFullPath, mainLang, loopStartIndex, tasksCount, taskIndex, retrisCount++);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DoExportUpdatedMediaJob(int groupId, long taskId, List<long> mediaIds, string exportFullPath, string mainLang, int loopStartIndex, int tasksCount, int taskIndex, int retrisCount = 0)
        {
            // calculate the start index of the media ids array 
            int startIndex = loopStartIndex + (taskIndex * maxAssetsPerTask);

            // calculate the number of ids to export 
            int numberOfIds = startIndex + maxAssetsPerTask <= mediaIds.Count ? maxAssetsPerTask : mediaIds.Count - startIndex; 

            try
            {
                MediaObj[] medias;
                if (tasksCount == 1)
                {
                    medias = GetMediaByIds(mediaIds, groupId);
                }
                else
                {
                    var currentIdsRange = mediaIds.GetRange(startIndex, numberOfIds);
                    log.WarnFormat("start: {0}, numberOfIds, {1}, number of medias: {2}", startIndex, numberOfIds, currentIdsRange.Count);
                    // get medias from catalog by ids (only the calculated range of media ids)
                    medias = GetMediaByIds(currentIdsRange, groupId);
                    log.WarnFormat("requested: {0}, returned: {1}", currentIdsRange.Count, medias.Length);

                }

                // if no medias returned - retry / fail the current bulk and continue the export
                if (medias == null || medias.Length == 0)
                {
                    throw new Exception(string.Format("failed to get media objects from catalog for task id = {0}, {1} medias from index = {2}", taskId, numberOfIds, startIndex));
                }

                // build the xml for all the retrieved medias
                StringBuilder xml = new StringBuilder();
                foreach (var media in medias)
                {
                    if (media != null)
                    {
                        xml.Append(BuildSingleMediaXml(media, mainLang, taskId));
                    }
                }

                // append the created xml to the export xml file
                lock (lockObject)
                {
                    File.AppendAllText(exportFullPath, xml.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("error in DoExportUpdatedMediaJob. task id = {0}, retry number {1}", taskId, retrisCount), ex);

                // if not exceeded retries limit - try again
                if (retrisCount < innerTaskRetriesLimit)
                {
                    return DoExportUpdatedMediaJob(groupId, taskId, mediaIds, exportFullPath, mainLang, loopStartIndex, tasksCount, taskIndex, retrisCount++);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static string BuildSingleMediaXml(MediaObj media, string mainLang, long taskId)
        {
            StringBuilder xml = new StringBuilder();

            try
            {
                xml.Append("<media ");

                // attributes
                xml.AppendFormat("co_guid=\"{0}\" entry_id=\"{1}\" action=\"{2}\" is_active=\"{3}\" erase=\"false\" media_id=\"{4}\">",
                     TVinciShared.ProtocolsFuncs.XMLEncode(media.CoGuid, true),                 // {0} - co guid
                     TVinciShared.ProtocolsFuncs.XMLEncode(media.EntryId, true),                // {1} - entryId
                     TVinciShared.ProtocolsFuncs.XMLEncode("update", true),                     // {2} - action
                     TVinciShared.ProtocolsFuncs.XMLEncode(media.IsActive.ToString(), true),    // {3} - is active
                     TVinciShared.ProtocolsFuncs.XMLEncode(media.AssetId.ToString(), true)      // {4} - media id
                     );

                // basic data
                xml.Append("<basic>");
                xml.AppendFormat("<media_type>{0}</media_type><epg_identifier>{1}</epg_identifier><name><value lang=\"{2}\">{3}</value></name><description><value lang=\"{4}\">{5}</value></description>",
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_oMediaType != null ? media.m_oMediaType.m_sTypeName : string.Empty, true),    // {0} - media type 
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_ExternalIDs, true),                                                           // {1} - epg identifier 
                    TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),                                                                      // {2} - main language
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_sName, true),                                                                 // {3} - name   
                    TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),                                                                      // {4} - main language
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_sDescription, true)                                                           // {5} - description           
                    );

                // thumb + pics
                xml.Append("<thumb url=\"\"/><pic_ratios></pic_ratios>");

                // rules
                xml.AppendFormat("<rules><watch_per_rule>{0}</watch_per_rule><geo_block_rule>{1}</geo_block_rule><players_rule>{2}</players_rule></rules>",
                    TVinciShared.ProtocolsFuncs.XMLEncode(string.Empty, true),      // {0} - watch rule  
                    TVinciShared.ProtocolsFuncs.XMLEncode(string.Empty, true),      // {1} - geo block   
                    TVinciShared.ProtocolsFuncs.XMLEncode(string.Empty, true)       // {2} - players rule
                );

                // dates
                xml.AppendFormat("<dates><catalog_start>{0}</catalog_start><start>{1}</start><catalog_end>{2}</catalog_end><final_end>{3}</final_end></dates>",
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_dCatalogStartDate.ToString("dd/MM/yyyy hh:mm:ss"), true),     // {0} - catalog start date
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_dStartDate.ToString("dd/MM/yyyy hh:mm:ss"), true),            // {1} - start date
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_dEndDate.ToString("dd/MM/yyyy hh:mm:ss"), true),              // {2} - catalog end date
                    TVinciShared.ProtocolsFuncs.XMLEncode(media.m_dFinalDate.ToString("dd/MM/yyyy hh:mm:ss"), true)             // {3} - end date
                    );

                xml.Append("</basic>");

                // metas
                xml.Append("<structure>");

                // strings
                xml.Append("<strings>");
                foreach (var meta in media.m_lMetas.Where(m => m.m_oTagMeta.m_sType == typeof(string).ToString()))
                {
                    xml.Append(GetStringMetaSection(meta, mainLang));
                }
                xml.Append("</strings>");

                // doubles
                xml.Append("<doubles>");
                foreach (var meta in media.m_lMetas.Where(m => m.m_oTagMeta.m_sType == typeof(double).ToString()))
                {
                    xml.Append(GetMetaSection(meta));
                }
                xml.Append("</doubles>");

                // booleans
                xml.Append("<booleans>");
                foreach (var meta in media.m_lMetas.Where(m => m.m_oTagMeta.m_sType == typeof(bool).ToString()))
                {
                    xml.Append(GetMetaSection(meta));
                }
                xml.Append("</booleans>");


                // tags
                xml.Append("<metas>");
                foreach (var tag in media.m_lTags)
                {
                    xml.Append(GetTagSection(tag, mainLang));
                }

                xml.Append("</metas>");
                xml.Append("</structure>");

                // files
                xml.Append("<files>");
                foreach (var file in media.m_lFiles)
                {
                    xml.Append(GetFileSection(file));
                }
                xml.Append("</files>");


                xml.Append("</media>");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("error in BuildSingleMediaXml. task id = {0}, media id = {1}", taskId, media.AssetId), ex);
                return string.Empty;
            }

            return xml.ToString();
        }

        private static string BuildSingleProgramXml(ProgramObj program, string mainLang, long taskId)
        {
            StringBuilder xml = new StringBuilder();
            
            // basic data
            if (program == null || program.m_oProgram == null)
                return string.Empty;

            var prog = program.m_oProgram;

            try
            {
                // programme
                xml.AppendFormat("<programme start=\"{0}\" stop=\"{1}\" channel=\"{2}\" external_id=\"{3}\" action=\"{4}\" id=\"{5}\">",
                    TVinciShared.ProtocolsFuncs.XMLEncode(prog.START_DATE, true),           // {0} - start
                    TVinciShared.ProtocolsFuncs.XMLEncode(prog.END_DATE, true),             // {1} - stop
                    TVinciShared.ProtocolsFuncs.XMLEncode(prog.EPG_CHANNEL_ID, true),       // {2} - channel
                    TVinciShared.ProtocolsFuncs.XMLEncode(prog.EPG_IDENTIFIER, true),       // {3} - external_id
                    TVinciShared.ProtocolsFuncs.XMLEncode("update", true),                  // {4} - action
                    TVinciShared.ProtocolsFuncs.XMLEncode(prog.EPG_ID.ToString(), true)     // {5} - id
                );

                // title
                xml.AppendFormat("<title lang=\"{0}\">{1}</title>",
                    TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),                  // {0} - lang
                    TVinciShared.ProtocolsFuncs.XMLEncode(prog.NAME, true)                  // {1} - title
                );

                // desc
                xml.AppendFormat("<desc lang=\"{0}\">{1}</desc>",
                    TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),                  // {0} - lang
                    TVinciShared.ProtocolsFuncs.XMLEncode(prog.DESCRIPTION, true)           // {1} - desc
                );

                // language
                xml.AppendFormat("<lang lang=\"{0}\">{1}</lang>",
                    TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),                  // {0} - lang
                    TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true)                   // {1} - lang
                );

                // metas
                if (prog.EPG_Meta != null)
                {
                    foreach (var meta in prog.EPG_Meta)
                    {
                        xml.AppendFormat("<metas><MetaType>{0}</MetaType><MetaValues lang=\"{1}\">{2}</MetaValues></metas>",
                            TVinciShared.ProtocolsFuncs.XMLEncode(meta.Key, true),                   // {0} - MetaType
                            TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),                   // {1} - lang
                            TVinciShared.ProtocolsFuncs.XMLEncode(meta.Value, true)                  // {2} - MetaValues
                        );
                    }
                }

                // tags
                if (prog.EPG_TAGS != null)
                {
                    foreach (var tag in prog.EPG_TAGS)
                    {
                        xml.AppendFormat("<tags><TagType>{0}</TagType>",
                            TVinciShared.ProtocolsFuncs.XMLEncode(tag.Key, true)                    // {0} - TagType
                        );

                        string[] tagVals = tag.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var tagVal in tagVals)
                        {
                            xml.AppendFormat("<TagValues lang=\"{0}\">{1}</TagValues>",
                                TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),              // {0} - lang
                                TVinciShared.ProtocolsFuncs.XMLEncode(tagVal, true)                 // {1} - TagValues
                            );
                            xml.Append("</tags>");
                        }
                    }

                    xml.Append("</programme>");
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("error in BuildSingleProgramXml. task id = {0}, media id = {1}", taskId, program.AssetId), ex);
                return string.Empty;
            }
            return xml.ToString();
        }

        private static MediaObj[] GetMediaByIds(List<long> ids, int groupId)
        {
            MediaObj[] result = null;
            AssetInfoResponse assetsResponse = null;

            AssetInfoRequest request = new AssetInfoRequest()
            {
                m_nGroupID = groupId,
                mediaIds = ids,
                m_oFilter = new Filter()
            };

            assetsResponse = request.GetResponse(request) as AssetInfoResponse;

            if (assetsResponse != null)
            {
                result = assetsResponse.mediaList.ToArray();
            }

            return result;
        }

        private static ProgramObj[] GetProgramsByIds(List<long> ids, int groupId)
        {
            ProgramObj[] result = null;
            AssetInfoResponse assetsResponse = null;

            AssetInfoRequest request = new AssetInfoRequest()
            {
                m_nGroupID = groupId,
                epgIds = ids,
                m_oFilter = new Filter()
            };

            assetsResponse = request.GetResponse(request) as AssetInfoResponse;

            if (assetsResponse != null)
            {
                result = assetsResponse.epgList.ToArray();
            }

            return result;
        }

        private static List<long> GetAssetsIdsByFilter(int groupId, string filter, eBulkExportDataType assetType, List<int> vodTypes, DateTime? since = null)
        {
            List<long> ids = null;
            
            // if since is not null - append the update date to the filter query
            if (since != null)
            {
                filter = AppendUpdateDateToFilter(filter, since);
            }

            // build unified search request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                filterQuery = filter,
                m_nGroupID = groupId,
                m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = true
                },
                shouldIgnoreDeviceRuleID = true,
                order = new ApiObjects.SearchObjects.OrderObj()
            };

            // set the request assetTypes parameter
            switch (assetType)
            {
                case eBulkExportDataType.VOD:
                    {
                        List<int> mediaTypes;
                        // if type is VOD and no VOD types supplied - get all available media types for the group
                        if (vodTypes == null || vodTypes.Count == 0)
                        {
                            // get media types IDs
                            mediaTypes = Utils.GetGroupMediaTypesIds(groupId).ToList();
                        }
                        else
                        {
                            mediaTypes = vodTypes;
                        }
                        
                        // if types not found - search cannot be performed (will return all asset types including epg) 
                        // throw exception and fail the process
                        if (mediaTypes == null || mediaTypes.Count == 0)
                        {
                            throw new Exception(string.Format("Export: no media types were found for group {0}", groupId));
                        }

                        request.assetTypes = mediaTypes;
                    }
                    break;
                case eBulkExportDataType.EPG:
                    // epg type = 0
                    request.assetTypes = new List<int> {0};
                    break;
                case eBulkExportDataType.Users:
                    throw new Exception("Export: users export date type not supported");
                default:
                    throw new Exception("Export: unknown export date type");
            }

            // perform the search
            UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;

            // if failed to get response from catalog - export process failed - throw exception
            if (response == null)
            {
                throw new Exception("Export: Failed to get assets from Catalog");
            }

            // get the ids
            if (response.searchResults != null)
            {
                ids = response.searchResults.Select(sr => Convert.ToInt64(sr.AssetId)).ToList();
            }
            
            return ids;
        }

        private static string AppendUpdateDateToFilter(string filter, DateTime? since)
        {
            return string.Format("(and update_date >= '{0}' {1})", ODBCWrapper.Utils.DateTimeToUnixTimestamp(since.Value), filter);
        }

        private static string GetStringMetaSection(Metas meta, string mainLang)
        {
            return string.Format("<meta name=\"{0}\" ml_handling=\"unique\"><value lang=\"{1}\">{2}</value></meta>",
                TVinciShared.ProtocolsFuncs.XMLEncode(meta.m_oTagMeta.m_sName, true),   // {0} - meta name      
                TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),                  // {1} - main language
                TVinciShared.ProtocolsFuncs.XMLEncode(meta.m_sValue, true)              // {2} - meta value     
            );
        }

        private static string GetMetaSection(Metas meta)
        {
            return string.Format("<meta name=\"{0}\" ml_handling=\"unique\">{1}</meta>",
                TVinciShared.ProtocolsFuncs.XMLEncode(meta.m_oTagMeta.m_sName, true),   // {0} - meta name      
                TVinciShared.ProtocolsFuncs.XMLEncode(meta.m_sValue, true)              // {2} - meta value     
            );
        }

        private static string GetTagSection(Tags tag, string mainLang)
        {
            StringBuilder section = new StringBuilder();
            section.AppendFormat("<meta name=\"{0}\" ml_handling=\"unique\">",
                TVinciShared.ProtocolsFuncs.XMLEncode(tag.m_oTagMeta.m_sName, true)   // {0} - tag name      
            );
            foreach (var tagVal in tag.m_lValues)
            {
                section.AppendFormat("<container><value lang=\"{0}\">{1}</value></container>",
                    TVinciShared.ProtocolsFuncs.XMLEncode(mainLang, true),   // {0} - main language 
                    TVinciShared.ProtocolsFuncs.XMLEncode(tagVal, true)      // {1} - tag value
                );
            }
            section.AppendFormat("</meta>");
            return section.ToString();
        }

        private static string GetFileSection(FileMedia file)
        {
            return string.Format("<file co_guid=\"{0}\" handling_type=\"{1}\" assetDuration=\"{2}\" quality=\"{3}\" type=\"{4}\""
            + " billing_type=\"{5}\" PPV_Module=\"{6}\" cdn_code=\"{7}\" cdn_id=\"{8}\" pre_rule=\"{9}\" post_rule=\"{10}\" break_rule=\"{11}\""
            + " break_points=\"{12}\" overlay_rule=\"{13}\" overlay_points=\"{14}\" file_start_date=\"{15}\" file_end_date=\"{16}\" ads_enabled=\"{17}\""
            + " contract_family=\"{18}\" lang=\"{19}\" default=\"{20}\" output_protection_level=\"{21}\" product_code=\"{22}\" alt_cdn_code=\"{23}\" alt_co_guid=\"{24}\" alt_cdn_id=\"{25}\" alt_cdn_name=\"{26}\"/>",
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_sCoGUID, true),                            // {0} - co_guid      
                TVinciShared.ProtocolsFuncs.XMLEncode("Clip", true),                                    // {1} - handling_type
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_nDuration.ToString(), true),               // {2} - assetDuration    
                TVinciShared.ProtocolsFuncs.XMLEncode("HIGH", true),                                    // {3} - quality     
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_sFileFormat, true),                        // {4} - type     
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_sBillingType, true),                       // {5} - billing_type 
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {6} - billing_type 
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_sUrl, true),                               // {7} - cdn_code  
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_nCdnID.ToString(), true),                  // {8} - cdn_id   
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {9} - pre_rule  
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {10} - post_rule  
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {11} - break_rule    
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_sBreakpoints, true),                       // {12} - break_points    
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {13} - overlay_rule     
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_sOverlaypoints, true),                     // {14} - overlay_points
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {15} - file_start_date     
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {16} - file_end_date     
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {17} - ads_enabled    
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {18} - contract_family    
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_sLanguage, true),                          // {19} - lang   
                TVinciShared.ProtocolsFuncs.XMLEncode(file.m_nIsDefaultLanguage.ToString(), true),      // {20} - default   
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {21} - output_protection_level   
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {22} - product_code   
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {23} - alt_cdn_code   
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {24} - alt_co_guid   
                TVinciShared.ProtocolsFuncs.XMLEncode("", true),                                        // {25} - alt_cdn_id   
                TVinciShared.ProtocolsFuncs.XMLEncode("", true)                                         // {26} - alt_cdn_name   
            );
        }

        public static bool SendNotification(long taskId, string notificationUrl, bool success, string filename = null)
        {
            bool result = false;

            // build notification full url
            StringBuilder fullNotificationUrl = new StringBuilder();

            long timestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            fullNotificationUrl.AppendFormat("{0}?task_id={1}&timestamp={2}", notificationUrl, taskId, timestamp);

            if (success)
            {
                fullNotificationUrl.Append("&status=success");

                if (!string.IsNullOrEmpty(filename))
                {
                    fullNotificationUrl.AppendFormat("&file={0}", filename);
                }
            }
            else
            {
                fullNotificationUrl.Append("&status=fail");
            }

            string response = null;
            int statusCode = 0;
            bool sendStatus = Utils.SendGetHttpRequest(fullNotificationUrl.ToString(), ref response, ref statusCode);

            if (!sendStatus || statusCode != 200)
            {
                log.ErrorFormat("Export - SendNotification: failed to send notification for task id = {0}, url = {1}, with status = {2}", taskId, fullNotificationUrl, statusCode);
            }
            else
            {
                log.DebugFormat("Export - SendNotification: sent notification for task id = {0}, url = {1}, with status = {2}", taskId, fullNotificationUrl, statusCode);
                result = true;
            }
            return result;
        }
    }
}
