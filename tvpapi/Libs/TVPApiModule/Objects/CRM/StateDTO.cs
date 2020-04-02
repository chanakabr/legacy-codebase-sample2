using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.Users;

namespace TVPApiModule.Objects.CRM
{
    public class StateDTO
    {
        private CountryDTO m_Country;
        public CountryDTO m_CountryField { get { return m_Country; } set { m_Country = value; } }
        private int m_nObjecrtID;
        public int m_nObjecrtIDField { get { return m_nObjecrtID; } set { m_nObjecrtID = value; } }
        private string m_sStateCode;
        public string m_sStateCodeField { get { return m_sStateCode; } set { m_sStateCode = value; } }
        private string m_sStateName;
        public string m_sStateNameField { get { return m_sStateName; } set { m_sStateName = value; } }

        public static StateDTO ConvertToDTO(State state)
        {
            if(state == null)
            {
                return null;
            }
            StateDTO res = new StateDTO()
            {
                m_Country = CountryDTO.ConvertToDTO(state .m_Country),
                m_nObjecrtID = state.m_nObjecrtID,
                m_sStateCode = state.m_sStateCode,
                m_sStateName = state.m_sStateName
            };
            return res;
        }

        public static State ConvertToCore(StateDTO state)
        {
            if(state == null)
            {
                return null;
            }
            var res = new State();
            res.m_Country = CountryDTO.ConvertToCore(state.m_Country);
            res.m_nObjecrtID = state.m_nObjecrtID;
            res.m_sStateCode = state.m_sStateCode;
            res.m_sStateName = state.m_sStateName;

            return res;
        }
    }
}