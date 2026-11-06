using StrayCat.BLL;
using StrayCat.DAL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class LoginDatabaseTests
    {
        private const string ConnectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True";

        [Fact]
        public void Seed_admin_logs_in_with_the_documented_password()
        {
            SysUser user = new UserDal(ConnectionString).GetByUserName("admin");
            OperationResult<CurrentUser> result = new UserBll(delegate { return user; }).Authenticate(user, "Admin@123");

            Assert.NotNull(user);
            Assert.True(result.Success);
            Assert.Equal("系统管理员", result.Data.RealName);
            Assert.Equal(RoleCodes.Admin, result.Data.Role);
        }
    }
}
