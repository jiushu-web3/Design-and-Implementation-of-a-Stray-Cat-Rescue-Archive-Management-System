using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using StrayCat.BLL;
using StrayCat.DAL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class BusinessFlowTests
    {
        private const string ConnectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True";

        [Fact]
        public void Lifecycle_follows_status_delete_and_account_rules()
        {
            string mark = "T" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string staffName = "u" + mark;
            List<int> cats = new List<int>();
            try
            {
                UserBll users = new UserBll(ConnectionString);
                CatBll catBll = new CatBll(ConnectionString);
                MedicalBll medical = new MedicalBll(ConnectionString);
                AdoptionBll adoption = new AdoptionBll(ConnectionString);
                FollowUpBll follow = new FollowUpBll(ConnectionString);
                StatBll stats = new StatBll(ConnectionString);
                SysUser admin = new UserDal(ConnectionString).GetByUserName("admin");
                Assert.NotNull(admin);

                DateTime rescueDay = DateTime.Today.AddDays(-1);
                int beforeRescue = MonthCount(stats, rescueDay, true);
                OperationResult<int> created = catBll.Create(Archive(mark + "A"), Rescue(rescueDay), admin.UserId);
                Assert.True(created.Success, created.Message);
                cats.Add(created.Data);

                CatDetail detail = catBll.GetDetail(created.Data);
                Assert.StartsWith("CAT" + DateTime.Today.Year, detail.Cat.CatNo);
                Assert.Equal("校园东门", detail.Rescue.Location);
                Assert.Equal(CatStatuses.InCare, detail.Cat.Status);
                Assert.Equal(beforeRescue + 1, MonthCount(stats, rescueDay, true));

                PagedResult<CatListItem> found = catBll.Search(new CatSearch
                {
                    Keyword = mark + "A",
                    Status = CatStatuses.InCare,
                    Gender = 1,
                    RescueFrom = rescueDay,
                    RescueTo = rescueDay,
                    PageIndex = 1
                });
                Assert.Equal(1, found.TotalCount);
                Assert.Equal(0, catBll.Search(new CatSearch { Keyword = mark + "A", Gender = 2, PageIndex = 1 }).TotalCount);

                Assert.True(catBll.Delete(created.Data).Success);
                cats.Remove(created.Data);

                OperationResult<int> second = catBll.Create(Archive(mark + "B"), Rescue(rescueDay), admin.UserId);
                Assert.True(second.Success, second.Message);
                cats.Add(second.Data);
                int catId = second.Data;

                Assert.Equal(Messages.StatusNotAllowed, adoption.Add(Adopt(catId, rescueDay), admin.UserId).Message);
                MedicalRecord early = Medical(catId, rescueDay.AddDays(-2));
                Assert.Equal(Messages.MedicalBeforeRescue, medical.Add(early, admin.UserId).Message);

                OperationResult<int> med = medical.Add(Medical(catId, rescueDay), admin.UserId);
                Assert.True(med.Success, med.Message);
                Assert.Equal(Messages.CatHasChildren, catBll.Delete(catId).Message);
                OperationResult<int> removedMedical = medical.Delete(med.Data);
                Assert.True(removedMedical.Success, removedMedical.Message);

                Assert.True(catBll.ChangeStatus(catId, CatStatuses.WaitingAdoption, null).Success);
                int beforeAdopt = MonthCount(stats, rescueDay, false);
                OperationResult<int> adopted = adoption.Add(Adopt(catId, rescueDay), admin.UserId);
                Assert.True(adopted.Success, adopted.Message);
                Assert.Equal(CatStatuses.Adopted, catBll.GetDetail(catId).Cat.Status);
                Assert.Equal(beforeAdopt + 1, MonthCount(stats, rescueDay, false));
                Assert.Equal(Messages.FollowUpRequired, catBll.ChangeStatus(catId, CatStatuses.Closed, null).Message);
                Assert.Equal(Messages.FollowUpRequired, new CatDal(ConnectionString).ChangeStatus(catId, CatStatuses.Closed, null).Message);

                OperationResult<int> visit = follow.Add(Visit(adopted.Data, DateTime.Today), admin.UserId);
                Assert.True(visit.Success, visit.Message);
                Assert.Equal(Messages.AdoptionHasFollowUp, adoption.Delete(adopted.Data).Message);
                Assert.True(catBll.ChangeStatus(catId, CatStatuses.Closed, null).Success);
                Assert.Equal(Messages.StatusNotAllowed, catBll.Update(Archive(mark + "B", catId)).Message);

                OperationResult<int> third = catBll.Create(Archive(mark + "C"), Rescue(rescueDay), admin.UserId);
                Assert.True(third.Success, third.Message);
                cats.Add(third.Data);
                Assert.True(catBll.ChangeStatus(third.Data, CatStatuses.WaitingAdoption, null).Success);
                OperationResult<int> firstAdopt = adoption.Add(Adopt(third.Data, rescueDay), admin.UserId);
                Assert.True(firstAdopt.Success, firstAdopt.Message);
                OperationResult<int> extraVisit = follow.Add(Visit(firstAdopt.Data, DateTime.Today), admin.UserId);
                Assert.True(extraVisit.Success, extraVisit.Message);
                Assert.Equal(Messages.AdoptionHasFollowUp, adoption.Delete(firstAdopt.Data).Message);
                Assert.True(follow.Delete(extraVisit.Data).Success);
                Assert.True(adoption.Return(firstAdopt.Data, DateTime.Today, "试养不合适").Success);
                Assert.Equal(CatStatuses.WaitingAdoption, catBll.GetDetail(third.Data).Cat.Status);
                OperationResult<int> again = adoption.Add(Adopt(third.Data, rescueDay), admin.UserId);
                Assert.True(again.Success, again.Message);
                Assert.True(adoption.Delete(again.Data).Success);
                Assert.True(adoption.Delete(firstAdopt.Data).Success);
                Assert.True(catBll.Delete(third.Data).Success);
                cats.Remove(third.Data);

                OperationResult<int> fourth = catBll.Create(Archive(mark + "D"), Rescue(rescueDay), admin.UserId);
                Assert.True(fourth.Success, fourth.Message);
                cats.Add(fourth.Data);
                Assert.Equal(Messages.TerminateReasonRequired, catBll.ChangeStatus(fourth.Data, CatStatuses.Terminated, "  ").Message);
                Assert.Equal(Messages.TerminateReasonRequired, new CatDal(ConnectionString).ChangeStatus(fourth.Data, CatStatuses.Terminated, null).Message);
                Assert.True(catBll.ChangeStatus(fourth.Data, CatStatuses.Terminated, "送回原地点").Success);
                Assert.True(catBll.Delete(fourth.Data).Success);
                cats.Remove(fourth.Data);

                OperationResult<int> staff = users.Create(staffName, "Staff@1", "测试员", null, RoleCodes.Staff, true);
                Assert.True(staff.Success, staff.Message);
                Assert.Equal(Messages.UserNameExists, users.Create(staffName, "Staff@1", "测试员", null, RoleCodes.Staff, true).Message);
                Assert.Equal(Messages.BadLogin, users.ChangePassword(staff.Data, "wrongxx", "NewPass1").Message);
                Assert.True(users.ChangePassword(staff.Data, "Staff@1", "NewPass1").Success);
                SysUser changed = new UserDal(ConnectionString).GetByUserName(staffName);
                Assert.True(new UserBll(delegate { return changed; }).Authenticate(changed, "NewPass1").Success);
                Assert.Equal(Messages.CannotDisableLastAdmin, users.Update(admin.UserId, admin.UserId, admin.RealName, admin.Phone, RoleCodes.Staff, true, null).Message);
                Assert.Equal(Messages.CannotDisableSelf, users.SetEnabled(admin.UserId, admin.UserId, false).Message);
                Assert.True(users.SetEnabled(staff.Data, admin.UserId, false).Success);
                SysUser disabled = new UserDal(ConnectionString).GetByUserName(staffName);
                Assert.Equal(Messages.Disabled, new UserBll(delegate { return disabled; }).Authenticate(disabled, "wrong").Message);
            }
            finally
            {
                Cleanup(cats, staffName);
                RestoreAdmin();
            }
        }

        private static int MonthCount(StatBll stats, DateTime day, bool rescue)
        {
            List<MonthStat> rows = stats.Monthly(day.Year);
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Month == day.Month)
                {
                    return rescue ? rows[i].RescueCount : rows[i].AdoptionCount;
                }
            }
            return 0;
        }

        private static CatArchive Archive(string name)
        {
            return Archive(name, 0);
        }

        private static CatArchive Archive(string name, int catId)
        {
            return new CatArchive
            {
                CatId = catId,
                TempName = name,
                Gender = 1,
                EstimatedAge = "约1岁",
                Color = "橘白",
                IsNeutered = 0,
                Remark = null
            };
        }

        private static RescueRecord Rescue(DateTime day)
        {
            return new RescueRecord
            {
                RescueTime = day.Date.AddHours(10),
                Location = "校园东门",
                FoundWay = "市民送交",
                InitialHealth = "健康，待观察"
            };
        }

        private static MedicalRecord Medical(int catId, DateTime day)
        {
            return new MedicalRecord
            {
                CatId = catId,
                MedicalDate = day.Date,
                Diagnosis = "体外寄生虫",
                Treatment = "体外驱虫",
                Medicine = "滴剂"
            };
        }

        private static AdoptionRecord Adopt(int catId, DateTime day)
        {
            return new AdoptionRecord
            {
                CatId = catId,
                AdopterName = "张三",
                AdopterPhone = "13800138000",
                AdopterAddress = "大连市中山区",
                AdoptDate = day.Date
            };
        }

        private static FollowUpRecord Visit(int adoptionId, DateTime day)
        {
            return new FollowUpRecord
            {
                AdoptionId = adoptionId,
                FollowDate = day.Date,
                FollowWay = "电话",
                CatCondition = "精神好，饮食正常"
            };
        }

        private static void Cleanup(IList<int> catIds, string userName)
        {
            string ids = catIds == null || catIds.Count == 0 ? "0" : string.Join(",", catIds);
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                Execute(connection, "DELETE FROM dbo.FollowUpRecord WHERE AdoptionId IN (SELECT AdoptionId FROM dbo.AdoptionRecord WHERE CatId IN (" + ids + "))");
                Execute(connection, "DELETE FROM dbo.AdoptionRecord WHERE CatId IN (" + ids + ")");
                Execute(connection, "DELETE FROM dbo.MedicalRecord WHERE CatId IN (" + ids + ")");
                Execute(connection, "DELETE FROM dbo.RescueRecord WHERE CatId IN (" + ids + ")");
                Execute(connection, "DELETE FROM dbo.CatArchive WHERE CatId IN (" + ids + ")");
                using (SqlCommand command = new SqlCommand("DELETE FROM dbo.SysUser WHERE UserName = @UserName AND UserName <> N'admin'", connection))
                {
                    command.Parameters.AddWithValue("@UserName", userName ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void RestoreAdmin()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                Execute(connection, "UPDATE dbo.SysUser SET Role = 1, IsEnabled = 1, RealName = N'系统管理员' WHERE UserName = N'admin'");
            }
        }

        private static void Execute(SqlConnection connection, string sql)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
