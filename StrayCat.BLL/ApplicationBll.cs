using System;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class ApplicationBll
    {
        private readonly ApplicationDal _applications;
        private readonly CatDal _cats;

        public ApplicationBll() : this(new DbHelper()) { }

        public ApplicationBll(string connectionString) : this(new DbHelper(connectionString)) { }

        private ApplicationBll(DbHelper db)
        {
            _applications = new ApplicationDal(db);
            _cats = new CatDal(db);
        }

        public OperationResult<int> Create(AdoptionApplication application, int userId)
        {
            application.ApplicantName = FieldRules.Clean(application.ApplicantName);
            application.Phone = FieldRules.Clean(application.Phone);
            application.Address = FieldRules.Clean(application.Address);
            application.Reason = FieldRules.Clean(application.Reason);

            string error = FieldRules.First(
                FieldRules.Required(application.ApplicantName, 50),
                FieldRules.Phone(application.Phone, true),
                FieldRules.Required(application.Address, 200),
                FieldRules.Optional(application.Reason, 500));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }

            CatDetail detail = _cats.GetDetail(application.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (detail.Cat.Status != CatStatuses.WaitingAdoption)
            {
                return OperationResult<int>.Fail(Messages.CatNotAdoptable);
            }

            application.UserId = userId;
            application.Status = ApplicationStatuses.Pending;
            return _applications.Insert(application);
        }

        public PagedResult<AdoptionApplication> ListByUser(int userId, int pageIndex, int pageSize)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            PagedResult<AdoptionApplication> page = _applications.ListByUser(userId, pageIndex, pageSize);
            page.PageIndex = pageIndex;
            page.PageSize = pageSize;
            return page;
        }

        public PagedResult<AdoptionApplication> Search(byte? status, int pageIndex, int pageSize)
        {
            return _applications.Search(status, pageIndex, pageSize);
        }

        public OperationResult<int> Approve(int applicationId, int adminId)
        {
            return _applications.Approve(applicationId, adminId);
        }

        public OperationResult<int> Reject(int applicationId, string reason, int adminId)
        {
            reason = FieldRules.Clean(reason);
            if (FieldRules.Required(reason, 200) != null)
            {
                return OperationResult<int>.Fail(Messages.RejectReasonRequired);
            }
            return _applications.Reject(applicationId, adminId, reason);
        }
    }
}
