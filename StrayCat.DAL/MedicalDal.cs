using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class MedicalDal
    {
        private readonly DbHelper _db;

        public MedicalDal()
            : this(new DbHelper())
        {
        }

        public MedicalDal(DbHelper db)
        {
            _db = db;
        }

        public MedicalRecord GetById(int medicalId)
        {
            List<MedicalRecord> rows = _db.Query("usp_Medical_GetById", RecordMaps.Medical, SqlParams.P("@MedicalId", medicalId));
            if (rows.Count == 0)
            {
                return null;
            }
            return rows[0];
        }

        public List<MedicalRecord> ListByCat(int catId)
        {
            return _db.Query("usp_Medical_ListByCat", RecordMaps.Medical, SqlParams.P("@CatId", catId));
        }

        public OperationResult<int> Insert(MedicalRecord record)
        {
            return _db.ExecuteReturnId(
                "usp_Medical_Insert",
                "@MedicalId",
                SqlParams.P("@CatId", record.CatId),
                SqlParams.P("@MedicalDate", record.MedicalDate),
                SqlParams.P("@Diagnosis", record.Diagnosis),
                SqlParams.P("@Treatment", record.Treatment),
                SqlParams.P("@Medicine", record.Medicine),
                SqlParams.P("@RecordUserId", record.RecordUserId),
                SqlParams.P("@Remark", record.Remark));
        }

        public OperationResult<int> Update(MedicalRecord record)
        {
            return _db.Execute(
                "usp_Medical_Update",
                SqlParams.P("@MedicalId", record.MedicalId),
                SqlParams.P("@MedicalDate", record.MedicalDate),
                SqlParams.P("@Diagnosis", record.Diagnosis),
                SqlParams.P("@Treatment", record.Treatment),
                SqlParams.P("@Medicine", record.Medicine),
                SqlParams.P("@Remark", record.Remark));
        }

        public OperationResult<int> Delete(int medicalId)
        {
            return _db.Execute("usp_Medical_Delete", SqlParams.P("@MedicalId", medicalId));
        }
    }
}
