using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class AdminAccountObj
    {

        public int m_id;

        public string m_name;

        public List<AdminAccountObj> m_subGroups;

        public string m_logo;

        public int m_parentGroupID;

        public AdminAccountObj()
        {
        }
        public AdminAccountObj(int id, string name, string logo, int parentGroupID)
        {
            m_id = id;
            m_name = name;
            m_logo = logo;
            m_parentGroupID = parentGroupID;
            m_subGroups = new List<AdminAccountObj>();
        }

      

    }
}
