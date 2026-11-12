using System;
using System.Data.SqlClient;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public static class DataMaps
    {
        public static SysUser User(SqlDataReader reader)
        {
            return new SysUser
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = Text(reader, "UserName"),
                PasswordHash = Text(reader, "PasswordHash"),
                Salt = Text(reader, "Salt"),
                RealName = Text(reader, "RealName"),
                Role = reader.GetByte(reader.GetOrdinal("Role")),
                Phone = Text(reader, "Phone"),
                IsEnabled = reader.GetBoolean(reader.GetOrdinal("IsEnabled")),
                CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime"))
            };
        }

        public static HomeCounts HomeCounts(SqlDataReader reader)
        {
            return new HomeCounts
            {
                InCareCount = reader.GetInt32(reader.GetOrdinal("InCareCount")),
                WaitingCount = reader.GetInt32(reader.GetOrdinal("WaitingCount")),
                AdoptedWithoutFollowUpCount = reader.GetInt32(reader.GetOrdinal("AdoptedWithoutFollowUpCount"))
            };
        }

        public static CatListItem CatListItem(SqlDataReader reader)
        {
            return new CatListItem
            {
                CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                CatNo = Text(reader, "CatNo"),
                TempName = Text(reader, "TempName"),
                Gender = reader.GetByte(reader.GetOrdinal("Gender")),
                Status = reader.GetByte(reader.GetOrdinal("Status")),
                RescueTime = Time(reader, "RescueTime")
            };
        }

        public static string Text(SqlDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            return reader.GetString(ordinal).Trim();
        }

        public static DateTime? Time(SqlDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }
            return reader.GetDateTime(ordinal);
        }

        public static bool Has(SqlDataReader reader, string name)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static int? NullableInt(SqlDataReader reader, string name)
        {
            int index = reader.GetOrdinal(name);
            return reader.IsDBNull(index) ? (int?)null : reader.GetInt32(index);
        }
    }
}
