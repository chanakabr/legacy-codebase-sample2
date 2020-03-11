using Core.Users;

namespace TVPApiModule.Objects.CRM
{
    public class UserResponseObjectDTO
    {
        public ResponseStatusDTO m_RespStatus;
        public UserDTO m_user;
        public int ExternalCode;


        public static UserResponseObjectDTO ConvertToDTO(UserResponseObject userResponseObject)
        {
            if (userResponseObject == null)
            {
                return null;
            }
            var res = new UserResponseObjectDTO();
            res.m_RespStatus = (ResponseStatusDTO)userResponseObject.m_RespStatus;
            
            res.m_user = UserDTO.ConvertToDTO(userResponseObject.m_user);
            res.ExternalCode = userResponseObject.ExternalCode;
            return res;
        }
    }
}
