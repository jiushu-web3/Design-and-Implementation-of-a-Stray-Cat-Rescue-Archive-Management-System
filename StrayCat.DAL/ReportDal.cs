using System;
using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class ReportDal
    {
        private readonly DbHelper _db;

        public ReportDal() : this(new DbHelper()) { }

        public ReportDal(DbHelper db)
        {
            _db = db;
        }

        public OperationResult<int> Insert(StrayReport report)
        {
            return _db.ExecuteReturnId(
                "usp_Report_Create",
                "@ReportId",
                SqlParams.P("@UserId", report.UserId),
                SqlParams.P("@FoundPlace", report.FoundPlace),
                SqlParams.P("@FoundDate", report.FoundDate),
                SqlParams.P("@Description", report.Description),
                SqlParams.P("@PhotoPath", report.PhotoPath),
                SqlParams.P("@ContactPhone", report.ContactPhone));
        }

        public PagedResult<StrayReport> ListByUser(int userId, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Report_ListByUser",
                RecordMaps.Report,
                SqlParams.P("@UserId", userId),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public PagedResult<StrayReport> Search(byte? status, string keyword, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Report_Search",
                RecordMaps.Report,
                SqlParams.P("@Status", status),
                SqlParams.P("@Keyword", keyword),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public OperationResult<int> Approve(int reportId, int auditUserId, CatArchive cat, DateTime rescueTime, string initialHealth)
        {
            return _db.ExecuteReturnId(
                "usp_Report_Approve",
                "@CatId",
                SqlParams.P("@ReportId", reportId),
                SqlParams.P("@AuditUserId", auditUserId),
                SqlParams.P("@TempName", cat.TempName),
                SqlParams.P("@Gender", cat.Gender),
                SqlParams.P("@EstimatedAge", cat.EstimatedAge),
                SqlParams.P("@Color", cat.Color),
                SqlParams.P("@IsNeutered", cat.IsNeutered),
                SqlParams.P("@PhotoPath", cat.PhotoPath),
                SqlParams.P("@Remark", cat.Remark),
                SqlParams.P("@RescueTime", rescueTime),
                SqlParams.P("@InitialHealth", initialHealth));
        }

        public OperationResult<int> Reject(int reportId, int auditUserId, string reason)
        {
            return _db.Execute(
                "usp_Report_Reject",
                SqlParams.P("@ReportId", reportId),
                SqlParams.P("@AuditUserId", auditUserId),
                SqlParams.P("@Reason", reason));
        }
    }
}
