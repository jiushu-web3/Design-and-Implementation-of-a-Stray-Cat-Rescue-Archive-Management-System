using StrayCat.DAL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class SqlErrorMapperTests
    {
        [Fact]
        public void User_error_keeps_the_chinese_message()
        {
            string message = SqlErrorMapper.ToMessage(50000, Messages.FollowUpRequired);

            Assert.Equal(Messages.FollowUpRequired, message);
        }

        [Fact]
        public void Known_unique_indexes_become_fixed_messages()
        {
            Assert.Equal(Messages.UserNameExists, SqlErrorMapper.ToMessage(2627, "Violation of UNIQUE KEY constraint 'UQ_SysUser_UserName'."));
            Assert.Equal(Messages.CatNoConflict, SqlErrorMapper.ToMessage(2601, "Cannot insert duplicate key row in object with unique index 'UQ_CatArchive_CatNo'."));
            Assert.Equal(Messages.RescueExists, SqlErrorMapper.ToMessage(2627, "UNIQUE KEY constraint 'UQ_RescueRecord_CatId'."));
            Assert.Equal(Messages.ActiveAdoptionExists, SqlErrorMapper.ToMessage(2601, "unique index 'UX_Adoption_OneActive'."));
        }

        [Fact]
        public void Other_sql_errors_are_not_business_messages()
        {
            Assert.Null(SqlErrorMapper.ToMessage(547, "FK conflict"));
            Assert.Null(SqlErrorMapper.ToMessage(2627, "some other unique index"));
        }
    }
}
