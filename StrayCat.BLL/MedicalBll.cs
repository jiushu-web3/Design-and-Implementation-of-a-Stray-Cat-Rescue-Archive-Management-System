using System;
using System.Collections.Generic;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class MedicalBll
    {
        private readonly CatDal _cats;
        private readonly MedicalDal _medical;

        public MedicalBll()
            : this(new DbHelper())
        {
        }

        public MedicalBll(string connectionString)
            : this(new DbHelper(connectionString))
        {
        }

        private MedicalBll(DbHelper db)
        {
            _cats = new CatDal(db);
            _medical = new MedicalDal(db);
        }

        public MedicalRecord Get(int medicalId)
        {
            return _medical.GetById(medicalId);
        }

        public List<MedicalRecord> ListByCat(int catId)
        {
            return _medical.ListByCat(catId);
        }

        public OperationResult<int> Add(MedicalRecord record, int userId)
        {
            Trim(record);
            record.RecordUserId = userId;
            CatDetail detail = _cats.GetDetail(record.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanEditMedical(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            string error = Validate(record, detail.Rescue.RescueTime);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _medical.Insert(record);
        }

        public OperationResult<int> Update(MedicalRecord record)
        {
            Trim(record);
            MedicalRecord existing = _medical.GetById(record.MedicalId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(existing.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanEditMedical(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            record.CatId = existing.CatId;
            string error = Validate(record, detail.Rescue.RescueTime);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _medical.Update(record);
        }

        public OperationResult<int> Delete(int medicalId)
        {
            MedicalRecord existing = _medical.GetById(medicalId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            CatDetail detail = _cats.GetDetail(existing.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanEditMedical(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            return _medical.Delete(medicalId);
        }

        private static void Trim(MedicalRecord record)
        {
            record.Diagnosis = FieldRules.Clean(record.Diagnosis);
            record.Treatment = FieldRules.Clean(record.Treatment);
            record.Medicine = FieldRules.Clean(record.Medicine);
            record.Remark = FieldRules.Clean(record.Remark);
        }

        private static string Validate(MedicalRecord record, DateTime rescueTime)
        {
            return FieldRules.First(
                FieldRules.RealDate(record.MedicalDate),
                FieldRules.NotAfterToday(record.MedicalDate),
                FieldRules.NotBefore(record.MedicalDate, rescueTime, Messages.MedicalBeforeRescue),
                FieldRules.Required(record.Diagnosis, 200),
                FieldRules.Required(record.Treatment, 500),
                FieldRules.Optional(record.Medicine, 200),
                FieldRules.Optional(record.Remark, 500));
        }
    }
}
