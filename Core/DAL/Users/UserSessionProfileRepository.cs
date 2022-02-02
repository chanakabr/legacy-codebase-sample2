using ApiObjects.User.SessionProfile;
using Phx.Lib.Log;
using Newtonsoft.Json;
using ODBCWrapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DAL.Users
{
    public interface IUserSessionProfileRepository
    {
        long InsertUserSessionProfile(int groupId, long userId, UserSessionProfile userSessionProfile);
        bool UpdateUserSessionProfile(int groupId, long userId, UserSessionProfile userSessionProfile);
        bool DeleteUserSessionProfile(int groupId, long userId, long userSessionProfileId);
        List<UserSessionProfile> GetUserSessionProfiles(int groupId);
    }

    public class UserSessionProfileRepository: IUserSessionProfileRepository
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly JsonSerializerSettings UserSessionProfileSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private const string USERS_CONNECTION_STRING = "USERS_CONNECTION_STRING";

        public long InsertUserSessionProfile(int groupId, long userId, UserSessionProfile userSessionProfile)
        {
            var sp = new StoredProcedure("Insert_UserSessionProfile");
            sp.SetConnectionKey(USERS_CONNECTION_STRING);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@updaterId", userId);
            sp.AddParameter("@name", userSessionProfile.Name);
            sp.AddParameter("@expression_json", JsonConvert.SerializeObject(userSessionProfile.Expression, UserSessionProfileSettings));

            long id = 0;
            var dt = sp.Execute();
            if (dt?.Rows?.Count > 0)
            {
                var dr = dt.Rows[0];
                id = Utils.GetLongSafeVal(dr, "ID");
            }

            if (id == 0)
            {
                var error = $"failed InsertUserSessionProfile to DB. groupId:{groupId}, name:{userSessionProfile.Name}";
                logger.Warn(error);
                throw new System.Exception(error); //currently phoenix does not hold exception handeling from db
            }

            return id;
        }

        public bool UpdateUserSessionProfile(int groupId, long userId, UserSessionProfile userSessionProfile)
        {
            var sp = new StoredProcedure("Update_UserSessionProfile");

            string expressionJson = null;
            if (userSessionProfile.Expression != null)
            {
                expressionJson = JsonConvert.SerializeObject(userSessionProfile.Expression, UserSessionProfileSettings);
            }

            sp.SetConnectionKey(USERS_CONNECTION_STRING);
            sp.AddParameter("@id", userSessionProfile.Id);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@updaterId", userId);
            sp.AddParameter("@name", userSessionProfile.Name);
            sp.AddParameter("@expression_json", expressionJson);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeleteUserSessionProfile(int groupId, long userId, long userSessionProfileId)
        {
            var sp = new StoredProcedure("Delete_UserSessionProfile");
            sp.SetConnectionKey(USERS_CONNECTION_STRING);
            sp.AddParameter("@id", userSessionProfileId);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@updaterId", userId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public List<UserSessionProfile> GetUserSessionProfiles(int groupId)
        {
            var sp = new StoredProcedure("Get_UserSessionProfiles");
            sp.SetConnectionKey(USERS_CONNECTION_STRING);
            sp.AddParameter("@groupId", groupId);

            var response = new List<UserSessionProfile>();

            var tb = sp.Execute();
            if (tb?.Rows != null)
            {
                foreach (DataRow dr in tb.Rows)
                {
                    if (!Json.TryDeserialize<IUserSessionProfileExpression>(Utils.GetSafeStr(dr, "expression_json"), out var expression))
                    {
                        continue; //support forward comparability - return only available expression types for current BE version
                    }

                    var userSessionProfile = new UserSessionProfile()
                    {
                        Id = Utils.GetLongSafeVal(dr, "ID"),
                        Name = Utils.GetSafeStr(dr, "NAME"),
                        Expression = expression,
                        CreateDate = Utils.DateTimeToUtcUnixTimestampSeconds(Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                        UpdateDate = Utils.DateTimeToUtcUnixTimestampSeconds(Utils.GetDateSafeVal(dr, "UPDATE_DATE"))
                    };

                    response.Add(userSessionProfile);
                }
            }

            return response;
        }

    }
}
