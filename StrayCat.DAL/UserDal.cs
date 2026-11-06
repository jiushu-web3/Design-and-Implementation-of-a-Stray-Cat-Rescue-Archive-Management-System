using System.Collections.Generic;
using System.Data.SqlClient;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class UserDal
    {
        private readonly DbHelper _db;

        public UserDal()
            : this(new DbHelper())
        {
        }

        public UserDal(string connectionString)
            : this(new DbHelper(connectionString))
        {
        }

        public UserDal(DbHelper db)
        {
            _db = db;
        }

        public SysUser GetByUserName(string userName)
        {
            return First(_db.Query("usp_User_GetByUserName", DataMaps.User, SqlParams.P("@UserName", userName ?? "")));
        }

        public SysUser GetById(int userId)
        {
            return First(_db.Query("usp_User_GetById", DataMaps.User, SqlParams.P("@UserId", userId)));
        }

        public int CountEnabled()
        {
            return _db.ExecuteScalarInt("usp_User_CountEnabled");
        }

        public int CountEnabledAdmins()
        {
            return _db.ExecuteScalarInt("usp_User_CountEnabledAdmins");
        }

        public PagedResult<SysUser> Search(string keyword, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_User_Search",
                DataMaps.User,
                SqlParams.P("@Keyword", keyword),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public OperationResult<int> Insert(string userName, string passwordHash, string salt, string realName, byte role, string phone, bool enabled)
        {
            return _db.ExecuteReturnId(
                "usp_User_Insert",
                "@UserId",
                SqlParams.P("@UserName", userName),
                SqlParams.P("@PasswordHash", passwordHash),
                SqlParams.P("@Salt", salt),
                SqlParams.P("@RealName", realName),
                SqlParams.Tiny("@Role", role),
                SqlParams.P("@Phone", phone),
                SqlParams.Bit("@IsEnabled", enabled));
        }

        public OperationResult<int> Update(int userId, int operatorId, string realName, string phone, byte role, bool enabled, string passwordHash, string salt)
        {
            return _db.Execute(
                "usp_User_Update",
                SqlParams.P("@UserId", userId),
                SqlParams.P("@OperatorId", operatorId),
                SqlParams.P("@RealName", realName),
                SqlParams.P("@Phone", phone),
                SqlParams.Tiny("@Role", role),
                SqlParams.Bit("@IsEnabled", enabled),
                SqlParams.P("@PasswordHash", passwordHash),
                SqlParams.P("@Salt", salt));
        }

        public OperationResult<int> SetEnabled(int userId, int operatorId, bool enabled)
        {
            return _db.Execute(
                "usp_User_SetEnabled",
                SqlParams.P("@UserId", userId),
                SqlParams.P("@OperatorId", operatorId),
                SqlParams.Bit("@IsEnabled", enabled));
        }

        public OperationResult<int> ChangePassword(int userId, string passwordHash, string salt)
        {
            return _db.Execute(
                "usp_User_ChangePassword",
                SqlParams.P("@UserId", userId),
                SqlParams.P("@PasswordHash", passwordHash),
                SqlParams.P("@Salt", salt));
        }

        private static SysUser First(List<SysUser> rows)
        {
            if (rows.Count == 0)
            {
                return null;
            }
            return rows[0];
        }
    }
}
