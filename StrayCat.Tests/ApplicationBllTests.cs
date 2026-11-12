using StrayCat.BLL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class ApplicationBllTests
    {
        private const string Cs = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True";

        [Fact]
        public void CreateRequiresApplicantName()
        {
            var result = new ApplicationBll(Cs).Create(new AdoptionApplication
            {
                CatId = 1,
                ApplicantName = " ",
                Phone = "13800138000",
                Address = "幸福路 1 号"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.RequiredField, result.Message);
        }

        [Fact]
        public void RejectRequiresReason()
        {
            var result = new ApplicationBll(Cs).Reject(1, " ", 1);
            Assert.False(result.Success);
            Assert.Equal(Messages.RejectReasonRequired, result.Message);
        }
    }
}
