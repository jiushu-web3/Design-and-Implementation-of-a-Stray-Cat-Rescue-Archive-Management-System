using System;

namespace StrayCat.Model
{
    public class CatListItem
    {
        public int CatId { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }
        public byte Gender { get; set; }
        public byte Status { get; set; }
        public DateTime? RescueTime { get; set; }

        public string StatusName
        {
            get { return CatStatuses.Name(Status); }
        }

        public string GenderName
        {
            get { return Lookups.GenderName(Gender); }
        }

        public string RescueTimeText
        {
            get { return RescueTime.HasValue ? RescueTime.Value.ToString("yyyy-MM-dd HH:mm") : ""; }
        }
    }
}
