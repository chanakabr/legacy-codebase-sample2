using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TVinciShared;

namespace Users
{
    public class UserDynamicData : ICloneable
    {
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

        protected bool UpdateUserDynamicData(Int32 nUserID, string sType , string sValue , Int32 nGroupID)
        {
            bool res = false;

            try
            {
                Int32 nID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetCachedSec(0);
                selectQuery += "SELECT ID FROM USERS_DYNAMIC_DATA WITH (NOLOCK) WHERE STATUS=0 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DATA_TYPE", "=", sType);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                if (nID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_dynamic_data");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DATA_TYPE", "=", sType);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DATA_VALUE", "=", sValue);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += " WHERE ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                    res = updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery inserQuery = new ODBCWrapper.InsertQuery("users_dynamic_data");
                    inserQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    inserQuery += ODBCWrapper.Parameter.NEW_PARAM("DATA_TYPE", "=", sType);
                    inserQuery += ODBCWrapper.Parameter.NEW_PARAM("DATA_VALUE", "=", sValue);
                    inserQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    inserQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserID);
                    inserQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                    res = inserQuery.Execute();
                    inserQuery.Finish();
                    inserQuery = null;
                }

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

            try
            {
                nGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("users", "group_id", nUserID).ToString());
            }
            catch { }
            
            Int32 nCount = m_sUserData.Length;
            saved = UpdateUserDynamicDataStatus(nUserID, 0);
            if (!saved) { return false; } 

            for (int i = 0; i < nCount; i++)
            {
                UserDynamicDataContainer t = (UserDynamicDataContainer)(m_sUserData[i]);
                string sType = t.m_sDataType;
                string sVal = t.m_sValue;
                
                if (sType != "")
                {
                    saved = UpdateUserDynamicData(nUserID, sType, sVal, nGroupID);

                    if (!saved) { return false; } 
                }
            }

            return saved;
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

        
        public object Clone()
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


        public UserDynamicDataContainer[] m_sUserData;

    }

}
