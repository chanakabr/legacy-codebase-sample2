using ApiObjects;
using ApiObjects.Epg;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgIngest
{
    internal static class InitTables
    {
        internal static readonly int MaxDescriptionSize = 1024;
        internal static readonly int MaxNameSize = 255;

        internal static DataTable InitEPGProgramMetaDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_meta_id", typeof(int));
            dt.Columns.Add("program_id", typeof(int));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            dt.Columns.Add("language_id", typeof(long));
            return dt;
        }

        internal static DataTable InitEPGProgramTagsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("program_id", typeof(int));
            dt.Columns.Add("epg_tag_id", typeof(int));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        internal static DataTable InitEPG_Tags_Values()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_tag_type_id", typeof(string));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        internal static DataTable InitEPGProgramPicturesDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("epg_identifier", typeof(string));
            dt.Columns.Add("channel_id", typeof(int));
            dt.Columns.Add("pic_id", typeof(int));
            dt.Columns.Add("ratio_id", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(int));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("UPDATER_ID", typeof(int));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            return dt;
        }

        internal static DataTable InitEPGDataTableWithID()
        {
            DataTable dt = new DataTable();
            // Add three column objects to the table. 
            DataColumn ID = new DataColumn();
            ID.DataType = typeof(long);
            ID.ColumnName = "ID";
            ID.Unique = true;
            dt.Columns.Add(ID);
            dt.Columns.Add("EPG_CHANNEL_ID", typeof(long));
            dt.Columns.Add("EPG_IDENTIFIER", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("START_DATE", typeof(DateTime));
            dt.Columns.Add("END_DATE", typeof(DateTime));
            dt.Columns.Add("PIC_ID", typeof(long));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("IS_ACTIVE", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(long));
            dt.Columns.Add("UPDATER_ID", typeof(long));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            dt.Columns.Add("PUBLISH_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("EPG_TAG", typeof(string));
            dt.Columns.Add("media_id", typeof(long));
            dt.Columns.Add("FB_OBJECT_ID", typeof(string));
            dt.Columns.Add("like_counter", typeof(long));
            dt.Columns.Add("ENABLE_CDVR", typeof(int));
            dt.Columns.Add("ENABLE_CATCH_UP", typeof(int));
            dt.Columns.Add("ENABLE_START_OVER", typeof(int));
            dt.Columns.Add("ENABLE_TRICK_PLAY", typeof(int));
            dt.Columns.Add("CRID", typeof(string));
         
            return dt;
        }
        
        internal static DataTable InitEPGDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EPG_CHANNEL_ID", typeof(long));
            dt.Columns.Add("EPG_IDENTIFIER", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("START_DATE", typeof(DateTime));
            dt.Columns.Add("END_DATE", typeof(DateTime));
            dt.Columns.Add("PIC_ID", typeof(long));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("IS_ACTIVE", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(long));
            dt.Columns.Add("UPDATER_ID", typeof(long));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            dt.Columns.Add("PUBLISH_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("EPG_TAG", typeof(string));
            dt.Columns.Add("media_id", typeof(long));
            dt.Columns.Add("FB_OBJECT_ID", typeof(string));
            dt.Columns.Add("like_counter", typeof(long));
            dt.Columns.Add("ENABLE_CDVR", typeof(int));
            dt.Columns.Add("ENABLE_CATCH_UP", typeof(int));
            dt.Columns.Add("ENABLE_START_OVER", typeof(int));
            dt.Columns.Add("ENABLE_TRICK_PLAY", typeof(int));
            dt.Columns.Add("CRID", typeof(string));
            return dt;
        }

        internal static void FillEPGDataTable(Dictionary<string, EpgCB> epgDic, ref DataTable dtEPG, DateTime dPublishDate)
        {
            if (epgDic != null && epgDic.Count > 0)
            {
                foreach (EpgCB epg in epgDic.Values)
                {
                    if (epg != null)
                    {
                        DataRow row = dtEPG.NewRow();
                        row["EPG_CHANNEL_ID"] = epg.ChannelID;
                        row["EPG_IDENTIFIER"] = epg.EpgIdentifier;

                        epg.Name = epg.Name.Replace("\r", "").Replace("\n", "");
                        if (epg.Name.Length >= MaxNameSize)
                            row["NAME"] = epg.Name.Substring(0, MaxNameSize); //insert only 255 chars (limitation of the column in the DB)
                        else
                            row["NAME"] = epg.Name;
                        epg.Description = epg.Description.Replace("\r", "").Replace("\n", "");
                        if (epg.Description.Length >= MaxDescriptionSize)
                            row["DESCRIPTION"] = epg.Description.Substring(0, MaxDescriptionSize); //insert only 1024 chars (limitation of the column in the DB)
                        else
                            row["DESCRIPTION"] = epg.Description;
                        row["START_DATE"] = epg.StartDate;
                        row["END_DATE"] = epg.EndDate;
                        row["PIC_ID"] = epg.PicID;
                        row["STATUS"] = epg.Status;
                        row["IS_ACTIVE"] = epg.isActive;
                        row["GROUP_ID"] = epg.GroupID;
                        row["UPDATER_ID"] = 400;
                        row["UPDATE_DATE"] = epg.UpdateDate;
                        row["PUBLISH_DATE"] = dPublishDate;
                        row["CREATE_DATE"] = epg.CreateDate;
                        row["EPG_TAG"] = null;
                        row["media_id"] = epg.ExtraData.MediaID;
                        row["FB_OBJECT_ID"] = epg.ExtraData.FBObjectID;
                        row["like_counter"] = epg.Statistics.Likes;

                        if (row.Table.Columns.Contains("ID") && epg.EpgID > 0)
                        {
                            row["ID"] = epg.EpgID;
                        }

                        row["ENABLE_CATCH_UP"] = epg.EnableCatchUp;
                        row["ENABLE_CDVR"] = epg.EnableCDVR;
                        row["ENABLE_START_OVER"] = epg.EnableStartOver;
                        row["ENABLE_TRICK_PLAY"] = epg.EnableTrickPlay;
                        row["CRID"] = epg.Crid;                    

                        dtEPG.Rows.Add(row);
                    }
                }
            }
        }

        internal static void FillEpgExtraDataTable(ref DataTable dtEPGExtra, bool bIsMeta, string sValue, ulong nProgID, int nID, int nGroupID, int nStatus,
                    int nUpdaterID, DateTime dCreateTime, DateTime dUpdateTime, int languageID = 0, bool bFillLanguageID = false)
        {
            DataRow row = dtEPGExtra.NewRow();
            if (bIsMeta)
            {
                row["value"] = sValue;
                row["epg_meta_id"] = nID;
            }
            else
            {
                row["epg_tag_id"] = nID;
            }

            row["program_id"] = nProgID;
            row["group_id"] = nGroupID;
            row["status"] = nStatus;
            row["updater_id"] = nUpdaterID;
            row["create_date"] = dCreateTime;
            row["update_date"] = dUpdateTime;
            if (bFillLanguageID)
            {
                row["language_id"] = languageID;
            }
            dtEPGExtra.Rows.Add(row);
        }

        internal static void FillEpgTagValueTable(ref DataTable dtEPGTagValue, string sValue, ulong nProgID, int nTagTypeID, int nGroupID, int nStatus, int nUpdaterID,
            DateTime dCreateTime, DateTime dUpdateTime)
        {
            DataRow row = dtEPGTagValue.NewRow();
            row["value"] = sValue;
            row["epg_tag_type_id"] = nTagTypeID;
            row["group_id"] = nGroupID;
            row["status"] = nStatus;
            row["updater_id"] = nUpdaterID;
            row["create_date"] = dCreateTime;
            row["update_date"] = dUpdateTime;
            dtEPGTagValue.Rows.Add(row);
        }

        internal static void FillEpgPictureTable(ref DataTable dtEpgPictures, EpgCB epg, Dictionary<int, string> ratios)
        {
            if (epg != null)
            {
                foreach (EpgPicture epgPicture in epg.pictures)
                {
                    DataRow row = dtEpgPictures.NewRow();
                    row["channel_id"] = epg.ChannelID;
                    row["epg_identifier"] = epg.EpgIdentifier;
                    row["pic_id"] = epgPicture.PicID;

                    if (!string.IsNullOrEmpty(epgPicture.Ratio))
                    {
                        int ratioID = ratios.Where(x => x.Value == epgPicture.Ratio).First().Key;
                        row["ratio_id"] = ratioID;
                    }
                    else
                    {
                        row["ratio_id"] = 0;
                    }
                 
                    row["STATUS"] = epg.Status;
                    row["GROUP_ID"] = epg.GroupID;
                    row["UPDATER_ID"] = 400;
                    row["UPDATE_DATE"] = epg.UpdateDate;
                    row["CREATE_DATE"] = epg.CreateDate;
                    dtEpgPictures.Rows.Add(row);
                }
            }          
        }

     
    }
}
