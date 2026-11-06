using System.Data.SqlClient;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public static class RecordMaps
    {
        public static CatDetail Detail(SqlDataReader reader)
        {
            return new CatDetail
            {
                MedicalCount = reader.GetInt32(reader.GetOrdinal("MedicalCount")),
                AdoptionCount = reader.GetInt32(reader.GetOrdinal("AdoptionCount")),
                Cat = new CatArchive
                {
                    CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                    CatNo = DataMaps.Text(reader, "CatNo"),
                    TempName = DataMaps.Text(reader, "TempName"),
                    Gender = reader.GetByte(reader.GetOrdinal("Gender")),
                    EstimatedAge = DataMaps.Text(reader, "EstimatedAge"),
                    Color = DataMaps.Text(reader, "Color"),
                    IsNeutered = reader.GetByte(reader.GetOrdinal("IsNeutered")),
                    Status = reader.GetByte(reader.GetOrdinal("Status")),
                    TerminateReason = DataMaps.Text(reader, "TerminateReason"),
                    PhotoPath = DataMaps.Text(reader, "PhotoPath"),
                    Remark = DataMaps.Text(reader, "Remark"),
                    CreateUserId = reader.GetInt32(reader.GetOrdinal("CreateUserId")),
                    CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime")),
                    UpdateTime = DataMaps.Time(reader, "UpdateTime")
                },
                Rescue = new RescueRecord
                {
                    RescueId = reader.GetInt32(reader.GetOrdinal("RescueId")),
                    CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                    RescueTime = reader.GetDateTime(reader.GetOrdinal("RescueTime")),
                    Location = DataMaps.Text(reader, "Location"),
                    FoundWay = DataMaps.Text(reader, "FoundWay"),
                    InitialHealth = DataMaps.Text(reader, "InitialHealth"),
                    ReceiverUserId = reader.GetInt32(reader.GetOrdinal("ReceiverUserId")),
                    Remark = DataMaps.Text(reader, "RescueRemark")
                }
            };
        }

        public static RescueRecord Rescue(SqlDataReader reader)
        {
            return new RescueRecord
            {
                RescueId = reader.GetInt32(reader.GetOrdinal("RescueId")),
                CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                RescueTime = reader.GetDateTime(reader.GetOrdinal("RescueTime")),
                Location = DataMaps.Text(reader, "Location"),
                FoundWay = DataMaps.Text(reader, "FoundWay"),
                InitialHealth = DataMaps.Text(reader, "InitialHealth"),
                ReceiverUserId = reader.GetInt32(reader.GetOrdinal("ReceiverUserId")),
                Remark = DataMaps.Text(reader, "Remark")
            };
        }

        public static MedicalRecord Medical(SqlDataReader reader)
        {
            return new MedicalRecord
            {
                MedicalId = reader.GetInt32(reader.GetOrdinal("MedicalId")),
                CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                MedicalDate = reader.GetDateTime(reader.GetOrdinal("MedicalDate")),
                Diagnosis = DataMaps.Text(reader, "Diagnosis"),
                Treatment = DataMaps.Text(reader, "Treatment"),
                Medicine = DataMaps.Text(reader, "Medicine"),
                RecordUserId = reader.GetInt32(reader.GetOrdinal("RecordUserId")),
                Remark = DataMaps.Text(reader, "Remark"),
                CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime"))
            };
        }

        public static AdoptionRecord Adoption(SqlDataReader reader)
        {
            return new AdoptionRecord
            {
                AdoptionId = reader.GetInt32(reader.GetOrdinal("AdoptionId")),
                CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                AdopterName = DataMaps.Text(reader, "AdopterName"),
                AdopterPhone = DataMaps.Text(reader, "AdopterPhone"),
                AdopterAddress = DataMaps.Text(reader, "AdopterAddress"),
                AdoptDate = reader.GetDateTime(reader.GetOrdinal("AdoptDate")),
                Status = reader.GetByte(reader.GetOrdinal("Status")),
                ReturnDate = DataMaps.Time(reader, "ReturnDate"),
                ReturnReason = DataMaps.Text(reader, "ReturnReason"),
                HandlerUserId = reader.GetInt32(reader.GetOrdinal("HandlerUserId")),
                Remark = DataMaps.Text(reader, "Remark"),
                CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime")),
                FollowUpCount = reader.GetInt32(reader.GetOrdinal("FollowUpCount"))
            };
        }

        public static FollowUpRecord FollowUp(SqlDataReader reader)
        {
            return new FollowUpRecord
            {
                FollowUpId = reader.GetInt32(reader.GetOrdinal("FollowUpId")),
                AdoptionId = reader.GetInt32(reader.GetOrdinal("AdoptionId")),
                FollowDate = reader.GetDateTime(reader.GetOrdinal("FollowDate")),
                FollowWay = DataMaps.Text(reader, "FollowWay"),
                CatCondition = DataMaps.Text(reader, "CatCondition"),
                RecordUserId = reader.GetInt32(reader.GetOrdinal("RecordUserId")),
                Remark = DataMaps.Text(reader, "Remark"),
                CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime"))
            };
        }

        public static MonthStat Month(SqlDataReader reader)
        {
            return new MonthStat
            {
                Month = reader.GetInt32(reader.GetOrdinal("MonthNum")),
                RescueCount = reader.GetInt32(reader.GetOrdinal("RescueCount")),
                AdoptionCount = reader.GetInt32(reader.GetOrdinal("AdoptionCount"))
            };
        }

        public static int Year(SqlDataReader reader)
        {
            return reader.GetInt32(reader.GetOrdinal("YearNum"));
        }
    }
}
