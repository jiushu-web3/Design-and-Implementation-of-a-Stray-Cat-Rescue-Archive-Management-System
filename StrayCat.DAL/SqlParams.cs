using System;
using System.Data;
using System.Data.SqlClient;

namespace StrayCat.DAL
{
    public static class SqlParams
    {
        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static SqlParameter Tiny(string name, byte? value)
        {
            SqlParameter parameter = new SqlParameter(name, SqlDbType.TinyInt);
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
            return parameter;
        }

        public static SqlParameter Bit(string name, bool value)
        {
            SqlParameter parameter = new SqlParameter(name, SqlDbType.Bit);
            parameter.Value = value;
            return parameter;
        }
    }
}
