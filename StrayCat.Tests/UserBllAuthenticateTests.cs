using StrayCat.BLL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class UserBllAuthenticateTests
    {
        private const string Salt = "0123456789ABCDEF0123456789ABCDEF";
        private const string Password = "Admin@123";
        private const string Hash = "9ddc3e471e45a5eab820bdecae8f62255d38a83619b4ce81caa8456909af3fa1";

        [Fact]
        public void Missing_user_is_bad_login()
        {
            OperationResult<CurrentUser> result = new UserBll().Authenticate(null, Password);

            Assert.False(result.Success);
            Assert.Equal(Messages.BadLogin, result.Message);
        }

        [Fact]
        public void Wrong_password_is_bad_login()
        {
            OperationResult<CurrentUser> result = new UserBll().Authenticate(EnabledUser(), "wrong-pass");

            Assert.False(result.Success);
            Assert.Equal(Messages.BadLogin, result.Message);
        }

        [Fact]
        public void Disabled_account_is_reported_even_when_password_is_wrong()
        {
            SysUser user = EnabledUser();
            user.IsEnabled = false;

            OperationResult<CurrentUser> result = new UserBll().Authenticate(user, "wrong-pass");

            Assert.False(result.Success);
            Assert.Equal(Messages.Disabled, result.Message);
        }

        [Fact]
        public void Correct_password_returns_current_user()
        {
            OperationResult<CurrentUser> result = new UserBll().Authenticate(EnabledUser(), Password);

            Assert.True(result.Success);
            Assert.Equal(7, result.Data.UserId);
            Assert.Equal("admin", result.Data.UserName);
            Assert.Equal("系统管理员", result.Data.RealName);
            Assert.Equal(RoleCodes.Admin, result.Data.Role);
        }

        private static SysUser EnabledUser()
        {
            return new SysUser
            {
                UserId = 7,
                UserName = "admin",
                RealName = "系统管理员",
                Role = RoleCodes.Admin,
                Salt = Salt,
                PasswordHash = Hash,
                IsEnabled = true
            };
        }
    }
}
