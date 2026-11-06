using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class AdoptionDal
    {
        private readonly DbHelper _db;

        public AdoptionDal()
            : this(new DbHelper())
        {
        }

        public AdoptionDal(DbHelper db)
        {
            _db = db;
        }

        public AdoptionRecord GetById(int adoptionId)
        {
            List<AdoptionRecord> rows = _db.Query("usp_Adoption_GetById", RecordMaps.Adoption, SqlParams.P("@AdoptionId", adoptionId));
            if (rows.Count == 0)
            {
                return null;
            }
            return rows[0];
        }

        public List<AdoptionRecord> ListByCat(int catId)
        {
            return _db.Query("usp_Adoption_ListByCat", RecordMaps.Adoption, SqlParams.P("@CatId", catId));
        }

        public OperationResult<int> Insert(AdoptionRecord record)
        {
            return _db.ExecuteReturnId(
                "usp_Adoption_Create",
                "@AdoptionId",
                SqlParams.P("@CatId", record.CatId),
                SqlParams.P("@AdopterName", record.AdopterName),
                SqlParams.P("@AdopterPhone", record.AdopterPhone),
                SqlParams.P("@AdopterAddress", record.AdopterAddress),
                SqlParams.P("@AdoptDate", record.AdoptDate),
                SqlParams.P("@HandlerUserId", record.HandlerUserId),
                SqlParams.P("@Remark", record.Remark));
        }

        public OperationResult<int> Update(AdoptionRecord record)
        {
            return _db.Execute(
                "usp_Adoption_Update",
                SqlParams.P("@AdoptionId", record.AdoptionId),
                SqlParams.P("@AdopterName", record.AdopterName),
                SqlParams.P("@AdopterPhone", record.AdopterPhone),
                SqlParams.P("@AdopterAddress", record.AdopterAddress),
                SqlParams.P("@Remark", record.Remark));
        }

        public OperationResult<int> Return(int adoptionId, System.DateTime returnDate, string reason)
        {
            return _db.Execute(
                "usp_Adoption_Return",
                SqlParams.P("@AdoptionId", adoptionId),
                SqlParams.P("@ReturnDate", returnDate),
                SqlParams.P("@ReturnReason", reason));
        }

        public OperationResult<int> Delete(int adoptionId)
        {
            return _db.Execute("usp_Adoption_Delete", SqlParams.P("@AdoptionId", adoptionId));
        }
    }
}
