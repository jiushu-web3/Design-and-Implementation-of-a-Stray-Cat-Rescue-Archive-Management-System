using System;
using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class CatDal
    {
        private readonly DbHelper _db;

        public CatDal()
            : this(new DbHelper())
        {
        }

        public CatDal(string connectionString)
            : this(new DbHelper(connectionString))
        {
        }

        public CatDal(DbHelper db)
        {
            _db = db;
        }

        public HomeCounts GetHomeCounts()
        {
            List<HomeCounts> rows = _db.Query("usp_Cat_HomeCounts", DataMaps.HomeCounts);
            if (rows.Count == 0)
            {
                return new HomeCounts();
            }
            return rows[0];
        }

        public List<CatListItem> GetRecent()
        {
            return _db.Query("usp_Cat_Recent", DataMaps.CatListItem);
        }

        public CatDetail GetDetail(int catId)
        {
            List<CatDetail> rows = _db.Query("usp_Cat_GetDetail", RecordMaps.Detail, SqlParams.P("@CatId", catId));
            if (rows.Count == 0)
            {
                return null;
            }
            return rows[0];
        }

        public RescueRecord GetRescue(int catId)
        {
            List<RescueRecord> rows = _db.Query("usp_Rescue_GetByCatId", RecordMaps.Rescue, SqlParams.P("@CatId", catId));
            if (rows.Count == 0)
            {
                return null;
            }
            return rows[0];
        }

        public PagedResult<CatListItem> Search(string keyword, byte? status, byte? gender, DateTime? rescueFrom, DateTime? rescueTo, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Cat_Search",
                DataMaps.CatListItem,
                SqlParams.P("@Keyword", keyword),
                SqlParams.Tiny("@Status", status),
                SqlParams.Tiny("@Gender", gender),
                SqlParams.P("@RescueFrom", rescueFrom),
                SqlParams.P("@RescueTo", rescueTo),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public OperationResult<int> Create(CatArchive cat, RescueRecord rescue)
        {
            return _db.ExecuteReturnId(
                "usp_Cat_CreateWithRescue",
                "@CatId",
                SqlParams.P("@TempName", cat.TempName),
                SqlParams.Tiny("@Gender", cat.Gender),
                SqlParams.P("@EstimatedAge", cat.EstimatedAge),
                SqlParams.P("@Color", cat.Color),
                SqlParams.Tiny("@IsNeutered", cat.IsNeutered),
                SqlParams.P("@PhotoPath", cat.PhotoPath),
                SqlParams.P("@Remark", cat.Remark),
                SqlParams.P("@CreateUserId", cat.CreateUserId),
                SqlParams.P("@RescueTime", rescue.RescueTime),
                SqlParams.P("@Location", rescue.Location),
                SqlParams.P("@FoundWay", rescue.FoundWay),
                SqlParams.P("@InitialHealth", rescue.InitialHealth),
                SqlParams.P("@RescueRemark", rescue.Remark));
        }

        public OperationResult<int> Update(CatArchive cat)
        {
            return _db.Execute(
                "usp_Cat_Update",
                SqlParams.P("@CatId", cat.CatId),
                SqlParams.P("@TempName", cat.TempName),
                SqlParams.Tiny("@Gender", cat.Gender),
                SqlParams.P("@EstimatedAge", cat.EstimatedAge),
                SqlParams.P("@Color", cat.Color),
                SqlParams.Tiny("@IsNeutered", cat.IsNeutered),
                SqlParams.P("@PhotoPath", cat.PhotoPath),
                SqlParams.P("@Remark", cat.Remark));
        }

        public OperationResult<int> UpdateRescue(RescueRecord rescue)
        {
            return _db.Execute(
                "usp_Rescue_Update",
                SqlParams.P("@CatId", rescue.CatId),
                SqlParams.P("@RescueTime", rescue.RescueTime),
                SqlParams.P("@Location", rescue.Location),
                SqlParams.P("@FoundWay", rescue.FoundWay),
                SqlParams.P("@InitialHealth", rescue.InitialHealth),
                SqlParams.P("@Remark", rescue.Remark));
        }

        public OperationResult<int> Delete(int catId)
        {
            return _db.Execute("usp_Cat_Delete", SqlParams.P("@CatId", catId));
        }

        public OperationResult<int> ChangeStatus(int catId, byte target, string reason)
        {
            return _db.Execute(
                "usp_Cat_ChangeStatus",
                SqlParams.P("@CatId", catId),
                SqlParams.Tiny("@TargetStatus", target),
                SqlParams.P("@TerminateReason", reason));
        }
    }
}
