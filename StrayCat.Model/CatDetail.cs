namespace StrayCat.Model
{
    public class CatDetail
    {
        public CatArchive Cat { get; set; }
        public RescueRecord Rescue { get; set; }
        public int MedicalCount { get; set; }
        public int AdoptionCount { get; set; }
    }

    public class CatSearch
    {
        public string Keyword { get; set; }
        public byte? Status { get; set; }
        public byte? Gender { get; set; }
        public System.DateTime? RescueFrom { get; set; }
        public System.DateTime? RescueTo { get; set; }
        public int PageIndex { get; set; }
    }

    public class MonthStat
    {
        public int Month { get; set; }
        public int RescueCount { get; set; }
        public int AdoptionCount { get; set; }
    }
}
