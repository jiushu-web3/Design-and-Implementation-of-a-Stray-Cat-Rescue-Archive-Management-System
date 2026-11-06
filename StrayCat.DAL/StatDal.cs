using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class StatDal
    {
        private readonly DbHelper _db;

        public StatDal()
            : this(new DbHelper())
        {
        }

        public StatDal(DbHelper db)
        {
            _db = db;
        }

        public List<int> Years()
        {
            return _db.Query("usp_Stat_Years", RecordMaps.Year);
        }

        public List<MonthStat> Monthly(int year)
        {
            return _db.Query("usp_Stat_Monthly", RecordMaps.Month, SqlParams.P("@Year", year));
        }
    }
}
