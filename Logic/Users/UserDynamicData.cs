using ApiObjects;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using TVinciShared;

namespace Core.Users
{
    public class UserDynamicData : ApiObjects.CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        Dictionary<int, KeyValuePair<string, string>> dynamicDataToUpdate = new Dictionary<int, KeyValuePair<string, string>>(); //rows to update in DB
        List<int> dynamicDataToRemoved = new List<int>(); //rows to remove from DB
        List<KeyValuePair<string, string>> dynamicDataToInsert = new List<KeyValuePair<string, string>>(); //rows to insert in DB
        int groupId = 0;
        public int UserId = 0;
            
        public UserDynamicData()
        {
            m_sUserData = null;
        }

        public UserDynamicDataContainer[] GetDynamicData()
        {
            return m_sUserData;
        }

        protected bool UpdateUserDynamicDataStatus(Int32 nUserID, Int32 nStatus)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_dynamic_data");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserID);
                res = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

                return res;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return false;
        }

        public bool Save(Int32 nUserID)
        {
            bool saved = false;
            int nGroupID = 0;
            int nID = 0;
            string sDataType = ""; //the data_type from the DB
            string sDataValue = ""; //the data_value from the DB                   
            List<int> lTypesNotInDB = new List<int>(); //types that are in the dynamic data and not in DB        
            //by default, each cell has the value of its index, but if this dataType is already in DB, it is marked with -1
            int[] TypesNotInDB = new int[m_sUserData.Length];
            for (int h = 0; h < m_sUserData.Length; h++)
                TypesNotInDB[h] = h;

            KeyValuePair<string, string> kvp;
            try
            {
                nGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("users", "group_id", nUserID, "USERS_CONNECTION_STRING").ToString());
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery.SetCachedSec(0);
                selectQuery += "SELECT ID, DATA_TYPE, DATA_VALUE FROM USERS_DYNAMIC_DATA WITH (NOLOCK) WHERE STATUS=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    //going over all the values in the DB and comparing to the the values in the user object (which includes the new\updated data)
                    for (int j = 0; j < nCount; j++)
                    {
                        nID = int.Parse(selectQuery.Table("query").DefaultView[j].Row["ID"].ToString());
                        sDataType = selectQuery.Table("query").DefaultView[j].Row["DATA_TYPE"].ToString();
                        sDataValue = selectQuery.Table("query").DefaultView[j].Row["DATA_VALUE"].ToString();
                        bool bFound = false;
                        for (int i = 0; i < m_sUserData.Length && !bFound; i++)
                        {
                            UserDynamicDataContainer t = (UserDynamicDataContainer)(m_sUserData[i]);
                            string sType = t.m_sDataType;
                            string sVal = t.m_sValue;

                            if (sType != "" && sType == sDataType)
                            {
                                TypesNotInDB[i] = -1;
                                bFound = true;
                                if (sVal != sDataValue) //update only rows that have differrent data 
                                {
                                    kvp = new KeyValuePair<string, string>(sType, sVal);
                                    dynamicDataToUpdate.Add(nID, kvp);
                                }
                            }
                        }
                        if (!bFound) //set status to 2 in DB
                        {
                            dynamicDataToRemoved.Add(nID);
                        }
                    }

                    //insert new dynamic data            
                    lTypesNotInDB = TypesNotInDB.Where(x => x > -1).ToList();
                    foreach (int index in lTypesNotInDB)
                    {
                        kvp = new KeyValuePair<string, string>(m_sUserData[index].m_sDataType, m_sUserData[index].m_sValue);
                        dynamicDataToInsert.Add(kvp);
                    }

                    saved = UpdateAllDynamicData(nUserID, nGroupID);
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch { }

            #region previous Code
            //Int32 nCount = m_sUserData.Length;
            //saved = UpdateUserDynamicDataStatus(nUserID, 0);
            //if (!saved) { return false; } 

            //for (int i = 0; i < nCount; i++)
            //{
            //    UserDynamicDataContainer t = (UserDynamicDataContainer)(m_sUserData[i]);
            //    string sType = t.m_sDataType;
            //    string sVal = t.m_sValue;

            //    if (sType != "")
            //    {
            //        saved = UpdateUserDynamicData(nUserID, sType, sVal, nGroupID);

            //        if (!saved) { return false; } 
            //    }
            //} 
            #endregion

            return saved;
        }

        /// <summary>
        /// New Save (21.01.2015, Michael)
        /// </summary>
        /// <param name="nUserID"></param>
        /// <returns></returns>
        public bool Save(int nUserID, int nGroupID)
        {
            bool saved = false;

            try
            {
                Dictionary<string, string> dTypeValue = new Dictionary<string, string>();

                for (int i = 0; i < m_sUserData.Length; i++)
                {
                    UserDynamicDataContainer dc = (UserDynamicDataContainer)(m_sUserData[i]);
                    string sType = dc.m_sDataType;
                    string sVal = dc.m_sValue;

                    if (!string.IsNullOrEmpty(sType))
                    {
                        dTypeValue[sType] = sVal;
                    }
                }

                dTypeValue = dTypeValue
                    .Where(tv => !string.IsNullOrEmpty(tv.Key))
                    .ToDictionary(tv => tv.Key, tv => tv.Value);

                XElement root = new XElement("root");  //     dTypeValue.Select(kv => new XElement(kv.Key, kv.Value)));

                if (dTypeValue != null && dTypeValue.Count > 0)
                {
                    foreach (var typeVal in dTypeValue)
                    {
                        XElement cElement = new XElement("row");

                        //cElement.SetAttributeValue("site_guid", nUserID.ToString());
                        cElement.SetAttributeValue("data_type", typeVal.Key);
                        cElement.SetAttributeValue("data_value", typeVal.Value);
                        //cElement.SetAttributeValue("group_id", nGroupID);                       

                        root.Add(cElement);
                    }
                }

                int res = DAL.UsersDal.Update_UserDynamicData(nUserID, nGroupID, root.ToString());
                saved = true;
            }
            catch
            {
            }

            return saved;
        }

        //generate a temporary table and insert to it all the data that need to be updated\inserted
        private bool UpdateAllDynamicData(int nUserID, int nGroupID)
        {
            bool insertResult = false;
            bool updateResult = false;
            bool deleteResult = false;
            UserId = nUserID;
            groupId = nGroupID;

            if (dynamicDataToInsert?.Count > 0)
            {
                insertResult = this.Insert();
                if (!insertResult)
                {
                    log.ErrorFormat("Error inserting dynamicData: {0}. userId: {1}, groupId: {2}", JsonConvert.SerializeObject(dynamicDataToInsert), UserId, groupId);
                }
            }

            if (dynamicDataToRemoved?.Count > 0)
            {
                deleteResult = this.Delete();
                if (!deleteResult)
                {
                    log.ErrorFormat("Error deleting dynamicData: {0}. userId: {1}, groupId: {2}", string.Join(",", dynamicDataToRemoved), UserId, groupId);
                }
            }

            if (dynamicDataToUpdate?.Count > 0)
            {
                updateResult = this.Update();
                if (!updateResult)
                {
                    log.ErrorFormat("Error updating dynamicData: {0}. userId: {1}, groupId: {2}", string.Join(",", dynamicDataToUpdate), UserId, groupId);
                }
            }

            return insertResult & deleteResult & updateResult;
        }

        public bool Initialize(string sXML)
        {
            if (sXML == "")
                return true;
            /*
                <dynamicdata>
                    <container>
                        <type></type>
                        <value></value>
                    </container>
                    <container>
                        <type></type>
                        <value></value>
                    </container>
                    <container>
                        <type></type>
                        <value></value>
                    </container>
               </dynamicdata>
            */

            System.Xml.XmlDocument theDoc = new System.Xml.XmlDocument();
            XmlNodeList l = null;
            try
            {
                theDoc.LoadXml(sXML);
                l = ((XmlElement)(theDoc.GetElementsByTagName("dynamicdata")[0])).GetElementsByTagName("container");
            }
            catch
            {
                return false;
            }
            m_sUserData = new UserDynamicDataContainer[l.Count];
            System.Collections.IEnumerator entryIter = l.GetEnumerator();
            Int32 nIndex = 0;
            while (entryIter.MoveNext())
            {
                XmlNode theEntry = (XmlNode)(entryIter.Current);
                string sType = WS_Utils.GetNodeValue(ref theEntry, "type");
                string sVal = WS_Utils.GetNodeValue(ref theEntry, "value");
                UserDynamicDataContainer c = new UserDynamicDataContainer();
                c.Initialize(sType, sVal);
                m_sUserData[nIndex] = c;
                nIndex++;
            }
            return true;

        }

        public bool Initialize(Int32 nUserID, Int32 nGroupID)
        {
            bool res = true;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "SELECT DATA_TYPE, DATA_VALUE FROM USERS_DYNAMIC_DATA WITH (NOLOCK) WHERE IS_ACTIVE=1 AND STATUS=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        m_sUserData = new UserDynamicDataContainer[nCount];

                        int nIndex = 0;

                        for (int i = 0; i < nCount; i++)
                        {
                            string sType = selectQuery.Table("query").DefaultView[i].Row["DATA_TYPE"].ToString();
                            string sVal = selectQuery.Table("query").DefaultView[i].Row["DATA_VALUE"].ToString();
                            UserDynamicDataContainer c = new UserDynamicDataContainer();
                            c.Initialize(sType, sVal);
                            m_sUserData[nIndex] = c;
                            nIndex++;
                        }
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                res = false;
                HandleException(ex);
            }

            return res;
        }

        public string GetValByKey(string key)
        {
            if (m_sUserData != null && m_sUserData.Length > 0)
            {
                foreach (UserDynamicDataContainer d in m_sUserData)
                {
                    if (d.m_sDataType.ToLower().Equals(key))
                    {
                        return d.m_sValue;
                    }
                }
            }

            return string.Empty;
        }


        public UserDynamicData Clone()
        {
            return CloneImpl();
        }

        protected virtual UserDynamicData CloneImpl()
        {
            var copy = (UserDynamicData)MemberwiseClone();

            return copy;
        }

        private void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            bool success = false;

            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery.SetConnectionKey("USERS_CONNECTION_STRING");

            directQuery += "create table #x ( id int, user_id int, data_type nvarchar(50), data_value nvarchar(512), is_active int,";
            directQuery += "status int, group_id int, create_date datetime, update_date datetime,  publish_date datetime)";

            foreach (KeyValuePair<string, string> kvp in dynamicDataToInsert)
            {
                directQuery += "insert into #x (id, user_id, data_type, data_value, is_active, status, group_id, create_date, update_date, publish_date) values";
                directQuery += "(0, " + UserId.ToString() + ", '" + kvp.Key + "' , '" + kvp.Value + "' ,1 , 1, " + groupId.ToString() + ",";
                directQuery += "getdate() , getdate(), getdate() )";
            }

            //insert the new rows
            directQuery += "insert into [users_dynamic_data] (USER_ID, DATA_TYPE, DATA_VALUE, IS_ACTIVE, STATUS, group_id, CREATE_DATE, UPDATE_DATE, PUBLISH_DATE)";
            directQuery += "select user_id, DATA_TYPE, DATA_VALUE, IS_ACTIVE ,STATUS, group_id, CREATE_DATE, UPDATE_DATE, PUBLISH_DATE from #x where #x.id = 0";

            success = directQuery.Execute();
            directQuery.Finish();
            directQuery = null;

            return success;
        }

        protected override bool DoUpdate()
        {
            bool success = false;

            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery.SetConnectionKey("USERS_CONNECTION_STRING");

            directQuery += "create table #x ( id int, user_id int, data_type nvarchar(50), data_value nvarchar(512), is_active int,";
            directQuery += "status int, group_id int, create_date datetime, update_date datetime,  publish_date datetime)";

            foreach (int id in dynamicDataToUpdate.Keys)
            {
                directQuery += "insert into #x (id, data_type, data_value,status, update_date) values (" + id.ToString() + ", '" + dynamicDataToUpdate[id].Key + "' , '" + dynamicDataToUpdate[id].Value + "', 1, getdate())";
            }

            //update the rows that already exist
            directQuery += "update users_dynamic_data set DATA_TYPE = #x.data_type, data_value = #x.data_value, STATUS = #x.status, UPDATE_DATE = #x.update_date";
            directQuery += "from  #x inner join [users_dynamic_data] on #x.id = [users_dynamic_data].id";

            success = directQuery.Execute();
            directQuery.Finish();
            directQuery = null;

            return success;
        }

        protected override bool DoDelete()
        {
            bool success = false;

            ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
            directQuery.SetConnectionKey("USERS_CONNECTION_STRING");

            directQuery += "create table #x ( id int, user_id int, data_type nvarchar(50), data_value nvarchar(512), is_active int,";
            directQuery += "status int, group_id int, create_date datetime, update_date datetime,  publish_date datetime)";

            //set the status of rows that needs to be removed to 2            
            foreach (int id in dynamicDataToRemoved)
            {
                directQuery += "insert into #x (id, data_type, data_value,status, update_date) values (" + id.ToString() + ",'','', 2 , getdate())";
            }

            //update the rows that already exist
            directQuery += "update users_dynamic_data set DATA_TYPE = #x.data_type, data_value = #x.data_value, STATUS = #x.status, UPDATE_DATE = #x.update_date";
            directQuery += "from  #x inner join [users_dynamic_data] on #x.id = [users_dynamic_data].id";

            success = directQuery.Execute();
            directQuery.Finish();
            directQuery = null;

            return success;

        }

        public override CoreObject CoreClone()
        {
            return this.Clone();
        }

        public UserDynamicDataContainer[] m_sUserData;
    }
}
