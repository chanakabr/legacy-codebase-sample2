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
        private int m_nObjecrtID;
        private string m_sStateCode;
        private string m_sStateName;

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