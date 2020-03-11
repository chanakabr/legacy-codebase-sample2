using DAL;
using Core.Users;

namespace TVPApiModule.Objects.CRM
{
    public class UserDTO
    {
        public UserBasicDataDTO m_oBasicData;
        public int m_domianID;
        public UserDynamicDataDTO m_oDynamicData;
        public string m_sSiteGUID;
        public DomainSuspentionStatus m_eSuspendState;
        public bool m_isDomainMaster;
        public bool IsActivationGracePeriod;
        public UserStateDTO m_eUserState;
        public int m_nSSOOperatorID;
        public int GroupId;
        public long Id; 


        public static UserDTO ConvertToDTO(User user)
        {
            if(user == null)
            {
                return null;
            }
            UserDTO res = new UserDTO();
            res.m_oBasicData = UserBasicDataDTO.ConvertToDTO(user.m_oBasicData);
            res.m_eUserState = (UserStateDTO)user.m_eUserState;
            res.m_eSuspendState = user.m_eSuspendState;
            res.m_sSiteGUID = user.m_sSiteGUID;
            res.m_domianID = user.m_domianID;
            res.m_isDomainMaster = user.m_isDomainMaster;
            res.m_nSSOOperatorID = user.m_nSSOOperatorID;
            res.IsActivationGracePeriod = user.IsActivationGracePeriod;
            res.GroupId = user.GroupId;
            res.Id = user.Id;
            
            res.m_oDynamicData = UserDynamicDataDTO.ConvertToDTO(user.m_oDynamicData);
            return res;
        }

      
    }
}