using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class ApplicationDal
    {
        private readonly DbHelper _db;

        public ApplicationDal() : this(new DbHelper()) { }

        public ApplicationDal(DbHelper db)
        {
            _db = db;
        }

        public OperationResult<int> Insert(AdoptionApplication application)
        {
            return _db.ExecuteReturnId(
                "usp_Application_Create",
                "@ApplicationId",
                SqlParams.P("@CatId", application.CatId),
                SqlParams.P("@UserId", application.UserId),
                SqlParams.P("@ApplicantName", application.ApplicantName),
                SqlParams.P("@Phone", application.Phone),
                SqlParams.P("@Address", application.Address),
                SqlParams.P("@Reason", application.Reason));
        }

        public PagedResult<AdoptionApplication> ListByUser(int userId, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Application_ListByUser",
                RecordMaps.Application,
                SqlParams.P("@UserId", userId),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public PagedResult<AdoptionApplication> Search(byte? status, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Application_Search",
                RecordMaps.Application,
                SqlParams.P("@Status", status),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public OperationResult<int> Approve(int applicationId, int auditUserId)
        {
            return _db.ExecuteReturnId(
                "usp_Application_Approve",
                "@AdoptionId",
                SqlParams.P("@ApplicationId", applicationId),
                SqlParams.P("@AuditUserId", auditUserId));
        }

        public OperationResult<int> Reject(int applicationId, int auditUserId, string reason)
        {
            return _db.Execute(
                "usp_Application_Reject",
                SqlParams.P("@ApplicationId", applicationId),
                SqlParams.P("@AuditUserId", auditUserId),
                SqlParams.P("@Reason", reason));
        }
    }
}
