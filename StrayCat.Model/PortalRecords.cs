namespace StrayCat.Model
{
    public class PublicCat
    {
        public int CatId { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }
        public byte Gender { get; set; }
        public byte Status { get; set; }
        public string EstimatedAge { get; set; }
        public string Color { get; set; }
        public byte IsNeutered { get; set; }
        public string PhotoPath { get; set; }
        public string Remark { get; set; }
        public System.DateTime? RescueTime { get; set; }
        public int MedicalCount { get; set; }
        public int AdoptionCount { get; set; }

        public string GenderName { get { return Lookups.GenderName(Gender); } }
        public string NeuteredName { get { return Lookups.NeuteredName(IsNeutered); } }
    }

    public class StrayReport
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string FoundPlace { get; set; }
        public System.DateTime FoundDate { get; set; }
        public string Description { get; set; }
        public string PhotoPath { get; set; }
        public string ContactPhone { get; set; }
        public byte Status { get; set; }
        public string RejectReason { get; set; }
        public int? CatId { get; set; }
        public System.DateTime CreateTime { get; set; }
        public int? AuditUserId { get; set; }
        public System.DateTime? AuditTime { get; set; }
    }

    public class AdoptionApplication
    {
        public int ApplicationId { get; set; }
        public int CatId { get; set; }
        public int UserId { get; set; }
        public string ApplicantName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Reason { get; set; }
        public byte Status { get; set; }
        public string RejectReason { get; set; }
        public int? AdoptionId { get; set; }
        public System.DateTime CreateTime { get; set; }
        public int? AuditUserId { get; set; }
        public System.DateTime? AuditTime { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }
    }
}
