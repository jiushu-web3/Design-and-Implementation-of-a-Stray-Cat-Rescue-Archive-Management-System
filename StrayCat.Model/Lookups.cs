namespace StrayCat.Model
{
    public static class Lookups
    {
        public static readonly string[] FoundWays = { "市民送交", "工作人员发现", "其他机构转入" };
        public static readonly string[] FollowWays = { "电话", "上门", "微信" };

        public static string GenderName(byte gender)
        {
            switch (gender)
            {
                case 1: return "公";
                case 2: return "母";
                default: return "未知";
            }
        }

        public static string NeuteredName(byte value)
        {
            switch (value)
            {
                case 1: return "已绝育";
                case 2: return "未绝育";
                default: return "未知";
            }
        }

        public static string RoleName(byte role)
        {
            return role == RoleCodes.Admin ? "管理员" : "普通用户";
        }

        public static string AdoptionName(byte status)
        {
            return status == AdoptionStatuses.Active ? "有效" : "已退养";
        }
    }
}
