using System;
using System.Collections.Generic;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class ReportBll
    {
        private readonly ReportDal _reports;

        public ReportBll() : this(new DbHelper()) { }

        public ReportBll(string connectionString) : this(new DbHelper(connectionString)) { }

        private ReportBll(DbHelper db)
        {
            _reports = new ReportDal(db);
        }

        public OperationResult<int> Create(StrayReport report, int userId)
        {
            report.FoundPlace = FieldRules.Clean(report.FoundPlace);
            report.Description = FieldRules.Clean(report.Description);
            report.ContactPhone = FieldRules.Clean(report.ContactPhone);
            report.PhotoPath = FieldRules.Clean(report.PhotoPath);

            string error = FieldRules.First(
                FieldRules.Required(report.FoundPlace, 200),
                FieldRules.RealDate(report.FoundDate),
                FieldRules.NotAfterToday(report.FoundDate),
                FieldRules.Optional(report.Description, 500),
                FieldRules.Phone(report.ContactPhone, true),
                FieldRules.Optional(report.PhotoPath, 200));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }

            report.UserId = userId;
            report.Status = ReportStatuses.Pending;
            return _reports.Insert(report);
        }

        public PagedResult<StrayReport> ListByUser(int userId, int pageIndex, int pageSize)
        {
            return _reports.ListByUser(userId, pageIndex, pageSize);
        }

        public PagedResult<StrayReport> Search(byte? status, string keyword, int pageIndex, int pageSize)
        {
            return _reports.Search(status, FieldRules.Clean(keyword), pageIndex, pageSize);
        }

        public OperationResult<int> Approve(int reportId, CatArchive cat, DateTime rescueTime, string initialHealth, int adminId)
        {
            cat.TempName = FieldRules.Clean(cat.TempName);
            cat.EstimatedAge = FieldRules.Clean(cat.EstimatedAge);
            cat.Color = FieldRules.Clean(cat.Color);
            cat.PhotoPath = FieldRules.Clean(cat.PhotoPath);
            cat.Remark = FieldRules.Clean(cat.Remark);
            initialHealth = FieldRules.Clean(initialHealth);

            string error = FieldRules.First(
                FieldRules.Required(cat.TempName, 50),
                FieldRules.Optional(cat.EstimatedAge, 20),
                FieldRules.Optional(cat.Color, 50),
                FieldRules.Optional(cat.Remark, 500),
                FieldRules.Required(initialHealth, 500),
                FieldRules.RealDate(rescueTime),
                FieldRules.NotAfterToday(rescueTime));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            if (cat.Gender < 1 || cat.Gender > 2)
            {
                return OperationResult<int>.Fail(Messages.RequiredField);
            }
            if (cat.IsNeutered < 0 || cat.IsNeutered > 2)
            {
                return OperationResult<int>.Fail(Messages.RequiredField);
            }
            return _reports.Approve(reportId, adminId, cat, rescueTime, initialHealth);
        }

        public OperationResult<int> Reject(int reportId, string reason, int adminId)
        {
            reason = FieldRules.Clean(reason);
            string error = FieldRules.Required(reason, 200);
            if (error != null)
            {
                return OperationResult<int>.Fail(Messages.RejectReasonRequired);
            }
            return _reports.Reject(reportId, adminId, reason);
        }
    }
}
