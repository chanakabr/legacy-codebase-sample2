using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Summary description for AdvertisingProvider
/// </summary>
/// 

namespace TVPApi
{
    public class AdvertisingProvider
    {
        private int m_ID;

        private string m_Name;

        public AdvertisingProvider()
        {
        }
         
        public AdvertisingProvider(int id, string name)
        {
            this.m_ID = id;
            this.m_Name = name;
        }

        public int ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }
    }
}
