using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class PortalStatusTests
    {
        [Fact]
        public void ReportStatusNames()
        {
            Assert.Equal("待审核", ReportStatuses.Name(ReportStatuses.Pending));
            Assert.Equal("已采纳", ReportStatuses.Name(ReportStatuses.Approved));
            Assert.Equal("已驳回", ReportStatuses.Name(ReportStatuses.Rejected));
        }

        [Fact]
        public void ApplicationStatusNames()
        {
            Assert.Equal("待审核", ApplicationStatuses.Name(ApplicationStatuses.Pending));
            Assert.Equal("已通过", ApplicationStatuses.Name(ApplicationStatuses.Approved));
            Assert.Equal("已驳回", ApplicationStatuses.Name(ApplicationStatuses.Rejected));
        }
    }
}
