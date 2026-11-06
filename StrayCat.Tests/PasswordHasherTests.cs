using StrayCat.BLL;
using Xunit;

namespace StrayCat.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_uses_seed_salt_and_admin_password()
        {
            string hash = PasswordHasher.Hash(
                "0123456789ABCDEF0123456789ABCDEF",
                "Admin@123");

            Assert.Equal("9ddc3e471e45a5eab820bdecae8f62255d38a83619b4ce81caa8456909af3fa1", hash);
        }
    }
}
