namespace StrayCat.Model
{
    public static class ReportStatuses
    {
        public const byte Pending = 1;
        public const byte Approved = 2;
        public const byte Rejected = 3;

        public static string Name(byte status)
        {
            if (status == Approved) return "已采纳";
            if (status == Rejected) return "已驳回";
            return "待审核";
        }
    }
}
