using StrayCat.BLL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class CatRulesTests
    {
        [Fact]
        public void Status_page_allows_only_the_documented_moves()
        {
            Assert.Null(CatRules.ChangeStatus(CatStatuses.InCare, CatStatuses.WaitingAdoption, 0, null));
            Assert.Null(CatRules.ChangeStatus(CatStatuses.WaitingAdoption, CatStatuses.InCare, 0, null));
            Assert.Equal(Messages.FollowUpRequired, CatRules.ChangeStatus(CatStatuses.Adopted, CatStatuses.Closed, 0, null));
            Assert.Null(CatRules.ChangeStatus(CatStatuses.Adopted, CatStatuses.Closed, 1, null));
            Assert.Equal(Messages.TerminateReasonRequired, CatRules.ChangeStatus(CatStatuses.InCare, CatStatuses.Terminated, 0, " "));
            Assert.Null(CatRules.ChangeStatus(CatStatuses.WaitingAdoption, CatStatuses.Terminated, 0, "转出"));
        }

        [Fact]
        public void Adoption_and_close_are_not_status_page_actions()
        {
            Assert.Equal(Messages.StatusNotAllowed, CatRules.ChangeStatus(CatStatuses.WaitingAdoption, CatStatuses.Adopted, 0, null));
            Assert.Equal(Messages.StatusNotAllowed, CatRules.ChangeStatus(CatStatuses.Adopted, CatStatuses.WaitingAdoption, 1, null));
            Assert.Equal(Messages.StatusNotAllowed, CatRules.ChangeStatus(CatStatuses.Closed, CatStatuses.InCare, 0, null));
        }

        [Fact]
        public void Write_permissions_follow_status()
        {
            Assert.True(CatRules.CanAddAdoption(CatStatuses.WaitingAdoption));
            Assert.False(CatRules.CanAddAdoption(CatStatuses.InCare));
            Assert.True(CatRules.CanEditFollowUp(CatStatuses.Adopted, AdoptionStatuses.Active));
            Assert.False(CatRules.CanEditFollowUp(CatStatuses.Adopted, AdoptionStatuses.Returned));
            Assert.False(CatRules.CanEditArchive(CatStatuses.Closed));
            Assert.False(CatRules.CanDeleteChildren(CatStatuses.Terminated));
            Assert.True(CatRules.CanDeleteCat(0, 0));
            Assert.False(CatRules.CanDeleteCat(1, 0));
        }
    }
}
