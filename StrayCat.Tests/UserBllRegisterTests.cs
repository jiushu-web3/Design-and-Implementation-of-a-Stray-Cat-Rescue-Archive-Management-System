using System;
using System.Data.SqlClient;
using StrayCat.BLL;
using StrayCat.DAL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class UserBllRegisterTests
    {
        private const string ConnectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True";

        [Fact]
        public void Register_rejects_a_mismatched_password()
        {
            OperationResult<int> result = new UserBll(delegate(string name) { return (SysUser)null; })
                .Register("staff01", "Secret1", "Secret2", "测试", null);

            Assert.False(result.Success);
            Assert.Equal(Messages.PasswordMismatch, result.Message);
        }

        [Fact]
        public void Register_rejects_a_short_username()
        {
            OperationResult<int> result = new UserBll(delegate(string name) { return (SysUser)null; })
                .Register("a", "Secret1", "Secret1", "测试", "");

            Assert.Equal(Messages.UserNameRule, result.Message);
        }

        [Fact]
        public void Register_creates_an_enabled_staff_account()
        {
            string name = "rg" + DateTime.Now.ToString("HHmmssfff");
            try
            {
                OperationResult<int> created = new UserBll(ConnectionString)
                    .Register(name, "Staff@1", "Staff@1", "注册测试", "13800138000");
                Assert.True(created.Success, created.Message);

                SysUser user = new UserDal(ConnectionString).GetByUserName(name);
                Assert.NotNull(user);
                Assert.Equal(RoleCodes.Staff, user.Role);
                Assert.True(user.IsEnabled);
                Assert.Equal("注册测试", user.RealName);

                OperationResult<CurrentUser> login = new UserBll(ConnectionString).Login(name, "Staff@1");
                Assert.True(login.Success);
                Assert.Equal(RoleCodes.Staff, login.Data.Role);
            }
            finally
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(
                        "DELETE FROM dbo.SysUser WHERE UserName = @UserName AND UserName <> N'admin'",
                        connection))
                    {
                        command.Parameters.AddWithValue("@UserName", name);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
