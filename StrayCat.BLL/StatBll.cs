using System;
using System.Collections.Generic;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class StatBll
    {
        private readonly StatDal _dal;

        public StatBll()
            : this(new DbHelper())
        {
        }

        public StatBll(string connectionString)
            : this(new DbHelper(connectionString))
        {
        }

        private StatBll(DbHelper db)
        {
            _dal = new StatDal(db);
        }

        public List<int> Years()
        {
            List<int> stored = _dal.Years();
            int now = DateTime.Today.Year;
            if (stored.Count == 0)
            {
                return new List<int> { now };
            }
            int min = stored[0];
            List<int> years = new List<int>();
            for (int year = min; year <= now; year++)
            {
                years.Add(year);
            }
            if (years.Count == 0)
            {
                years.Add(now);
            }
            return years;
        }

        public List<MonthStat> Monthly(int year)
        {
            return _dal.Monthly(year);
        }
    }
}
