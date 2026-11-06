using System;
using System.Collections.Generic;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class AdoptionBll
    {
        private readonly CatDal _cats;
        private readonly AdoptionDal _adoptions;

        public AdoptionBll()
            : this(new DbHelper())
        {
        }

        public AdoptionBll(string connectionString)
            : this(new DbHelper(connectionString))
        {
        }

        private AdoptionBll(DbHelper db)
        {
            _cats = new CatDal(db);
            _adoptions = new AdoptionDal(db);
        }

        public AdoptionRecord Get(int adoptionId)
        {
            return _adoptions.GetById(adoptionId);
        }

        public List<AdoptionRecord> ListByCat(int catId)
        {
            return _adoptions.ListByCat(catId);
        }

        public OperationResult<int> Add(AdoptionRecord record, int userId)
        {
            Trim(record);
            record.HandlerUserId = userId;
            CatDetail detail = _cats.GetDetail(record.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanAddAdoption(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            string error = ValidateContact(record, detail.Rescue.RescueTime);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _adoptions.Insert(record);
        }

        public OperationResult<int> UpdateContact(AdoptionRecord record)
        {
            Trim(record);
            AdoptionRecord existing = _adoptions.GetById(record.AdoptionId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(existing.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (existing.Status != AdoptionStatuses.Active || !CatRules.CanEditActiveAdoption(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            record.AdoptDate = existing.AdoptDate;
            string error = ValidateContact(record, detail.Rescue.RescueTime);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _adoptions.Update(record);
        }

        public OperationResult<int> Return(int adoptionId, DateTime returnDate, string reason)
        {
            reason = FieldRules.Clean(reason);
            AdoptionRecord existing = _adoptions.GetById(adoptionId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(existing.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (existing.Status != AdoptionStatuses.Active || !CatRules.CanEditActiveAdoption(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            string error = FieldRules.First(
                FieldRules.RealDate(returnDate),
                FieldRules.NotAfterToday(returnDate),
                FieldRules.NotBefore(returnDate, existing.AdoptDate, Messages.ReturnBeforeAdopt),
                FieldRules.Required(reason, 200));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _adoptions.Return(adoptionId, returnDate, reason);
        }

        public OperationResult<int> Delete(int adoptionId)
        {
            AdoptionRecord existing = _adoptions.GetById(adoptionId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(existing.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanDeleteChildren(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            if (existing.FollowUpCount > 0)
            {
                return OperationResult<int>.Fail(Messages.AdoptionHasFollowUp);
            }
            return _adoptions.Delete(adoptionId);
        }

        private static void Trim(AdoptionRecord record)
        {
            record.AdopterName = FieldRules.Clean(record.AdopterName);
            record.AdopterPhone = FieldRules.Clean(record.AdopterPhone);
            record.AdopterAddress = FieldRules.Clean(record.AdopterAddress);
            record.Remark = FieldRules.Clean(record.Remark);
        }

        private static string ValidateContact(AdoptionRecord record, DateTime rescueTime)
        {
            return FieldRules.First(
                FieldRules.Required(record.AdopterName, 50),
                FieldRules.Phone(record.AdopterPhone, true),
                FieldRules.Required(record.AdopterAddress, 200),
                FieldRules.Optional(record.Remark, 500),
                FieldRules.RealDate(record.AdoptDate),
                FieldRules.NotAfterToday(record.AdoptDate),
                FieldRules.NotBefore(record.AdoptDate, rescueTime, Messages.AdoptBeforeRescue));
        }
    }
}
