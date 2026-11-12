using System;
using StrayCat.BLL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class ReportBllTests
    {
        private const string Cs = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True";

        [Fact]
        public void CreateRequiresFoundPlace()
        {
            var result = new ReportBll(Cs).Create(new StrayReport
            {
                FoundPlace = " ",
                FoundDate = DateTime.Today,
                ContactPhone = "13800138000"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.RequiredField, result.Message);
        }

        [Fact]
        public void CreateRejectsFutureDate()
        {
            var result = new ReportBll(Cs).Create(new StrayReport
            {
                FoundPlace = "一号楼后门",
                FoundDate = DateTime.Today.AddDays(1),
                ContactPhone = "13800138000"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.DateAfterToday, result.Message);
        }

        [Fact]
        public void CreateRejectsBadPhone()
        {
            var result = new ReportBll(Cs).Create(new StrayReport
            {
                FoundPlace = "一号楼后门",
                FoundDate = DateTime.Today,
                ContactPhone = "123"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.PhoneRule, result.Message);
        }
    }
}
