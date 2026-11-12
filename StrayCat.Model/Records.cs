using System;

namespace StrayCat.Model
{
    public class CatArchive
    {
        public int CatId { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }
        public byte Gender { get; set; }
        public string EstimatedAge { get; set; }
        public string Color { get; set; }
        public byte IsNeutered { get; set; }
        public byte Status { get; set; }
        public string TerminateReason { get; set; }
        public string PhotoPath { get; set; }
        public string Remark { get; set; }
        public int CreateUserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }

    public class RescueRecord
    {
        public int RescueId { get; set; }
        public int CatId { get; set; }
        public DateTime RescueTime { get; set; }
        public string Location { get; set; }
        public string FoundWay { get; set; }
        public string InitialHealth { get; set; }
        public int ReceiverUserId { get; set; }
        public string Remark { get; set; }

        public string RescueTimeText
        {
            get { return RescueTime.ToString("yyyy-MM-dd HH:mm"); }
        }
    }

    public class MedicalRecord
    {
        public int MedicalId { get; set; }
        public int CatId { get; set; }
        public DateTime MedicalDate { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Medicine { get; set; }
        public int RecordUserId { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }

        public string MedicalDateText
        {
            get { return MedicalDate.ToString("yyyy-MM-dd"); }
        }
    }

    public class AdoptionRecord
    {
        public int AdoptionId { get; set; }
        public int CatId { get; set; }
        public string AdopterName { get; set; }
        public string AdopterPhone { get; set; }
        public string AdopterAddress { get; set; }
        public DateTime AdoptDate { get; set; }
        public byte Status { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnReason { get; set; }
        public int HandlerUserId { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public int FollowUpCount { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }

        public string StatusName
        {
            get { return Lookups.AdoptionName(Status); }
        }

        public string AdoptDateText
        {
            get { return AdoptDate.ToString("yyyy-MM-dd"); }
        }

        public string ReturnDateText
        {
            get { return ReturnDate.HasValue ? ReturnDate.Value.ToString("yyyy-MM-dd") : ""; }
        }
    }

    public class FollowUpRecord
    {
        public int FollowUpId { get; set; }
        public int AdoptionId { get; set; }
        public DateTime FollowDate { get; set; }
        public string FollowWay { get; set; }
        public string CatCondition { get; set; }
        public int RecordUserId { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }

        public string FollowDateText
        {
            get { return FollowDate.ToString("yyyy-MM-dd"); }
        }
    }
}
