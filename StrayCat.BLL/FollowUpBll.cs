using System.Collections.Generic;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class FollowUpBll
    {
        private readonly CatDal _cats;
        private readonly AdoptionDal _adoptions;
        private readonly FollowUpDal _follows;

        public FollowUpBll()
            : this(new DbHelper())
        {
        }

        public FollowUpBll(string connectionString)
            : this(new DbHelper(connectionString))
        {
        }

        private FollowUpBll(DbHelper db)
        {
            _cats = new CatDal(db);
            _adoptions = new AdoptionDal(db);
            _follows = new FollowUpDal(db);
        }

        public FollowUpRecord Get(int followUpId)
        {
            return _follows.GetById(followUpId);
        }

        public List<FollowUpRecord> ListByAdoption(int adoptionId)
        {
            return _follows.ListByAdoption(adoptionId);
        }

        public OperationResult<int> Add(FollowUpRecord record, int userId)
        {
            Trim(record);
            record.RecordUserId = userId;
            AdoptionRecord adoption = _adoptions.GetById(record.AdoptionId);
            if (adoption == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(adoption.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanEditFollowUp(detail.Cat.Status, adoption.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            string error = Validate(record, adoption.AdoptDate);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _follows.Insert(record);
        }

        public OperationResult<int> Update(FollowUpRecord record)
        {
            Trim(record);
            FollowUpRecord existing = _follows.GetById(record.FollowUpId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            AdoptionRecord adoption = _adoptions.GetById(existing.AdoptionId);
            if (adoption == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(adoption.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanEditFollowUp(detail.Cat.Status, adoption.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            record.AdoptionId = existing.AdoptionId;
            string error = Validate(record, adoption.AdoptDate);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _follows.Update(record);
        }

        public OperationResult<int> Delete(int followUpId)
        {
            FollowUpRecord existing = _follows.GetById(followUpId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            AdoptionRecord adoption = _adoptions.GetById(existing.AdoptionId);
            if (adoption == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(adoption.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanDeleteChildren(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            return _follows.Delete(followUpId);
        }

        private static void Trim(FollowUpRecord record)
        {
            record.FollowWay = FieldRules.Clean(record.FollowWay);
            record.CatCondition = FieldRules.Clean(record.CatCondition);
            record.Remark = FieldRules.Clean(record.Remark);
        }

        private static string Validate(FollowUpRecord record, System.DateTime adoptDate)
        {
            return FieldRules.First(
                FieldRules.RealDate(record.FollowDate),
                FieldRules.NotAfterToday(record.FollowDate),
                FieldRules.NotBefore(record.FollowDate, adoptDate, Messages.FollowBeforeAdopt),
                FieldRules.Choice(record.FollowWay, Lookups.FollowWays, Messages.BadFollowWay),
                FieldRules.Required(record.CatCondition, 500),
                FieldRules.Optional(record.Remark, 500));
        }
    }
}
