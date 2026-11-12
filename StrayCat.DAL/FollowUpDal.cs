using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class FollowUpDal
    {
        private readonly DbHelper _db;

        public FollowUpDal()
            : this(new DbHelper())
        {
        }

        public FollowUpDal(DbHelper db)
        {
            _db = db;
        }

        public FollowUpRecord GetById(int followUpId)
        {
            List<FollowUpRecord> rows = _db.Query("usp_FollowUp_GetById", RecordMaps.FollowUp, SqlParams.P("@FollowUpId", followUpId));
            if (rows.Count == 0)
            {
                return null;
            }
            return rows[0];
        }

        public List<FollowUpRecord> ListByAdoption(int adoptionId)
        {
            return _db.Query("usp_FollowUp_ListByAdoption", RecordMaps.FollowUp, SqlParams.P("@AdoptionId", adoptionId));
        }

        public List<FollowUpRecord> Recent()
        {
            return _db.Query("usp_FollowUp_Recent", RecordMaps.FollowUpHistory);
        }

        public int CountByAdoption(int adoptionId)
        {
            return _db.ExecuteScalarInt("usp_FollowUp_CountByAdoption", SqlParams.P("@AdoptionId", adoptionId));
        }

        public OperationResult<int> Insert(FollowUpRecord record)
        {
            return _db.ExecuteReturnId(
                "usp_FollowUp_Insert",
                "@FollowUpId",
                SqlParams.P("@AdoptionId", record.AdoptionId),
                SqlParams.P("@FollowDate", record.FollowDate),
                SqlParams.P("@FollowWay", record.FollowWay),
                SqlParams.P("@CatCondition", record.CatCondition),
                SqlParams.P("@RecordUserId", record.RecordUserId),
                SqlParams.P("@Remark", record.Remark));
        }

        public OperationResult<int> Update(FollowUpRecord record)
        {
            return _db.Execute(
                "usp_FollowUp_Update",
                SqlParams.P("@FollowUpId", record.FollowUpId),
                SqlParams.P("@FollowDate", record.FollowDate),
                SqlParams.P("@FollowWay", record.FollowWay),
                SqlParams.P("@CatCondition", record.CatCondition),
                SqlParams.P("@Remark", record.Remark));
        }

        public OperationResult<int> Delete(int followUpId)
        {
            return _db.Execute("usp_FollowUp_Delete", SqlParams.P("@FollowUpId", followUpId));
        }
    }
}
