namespace StrayCat.Model
{
    public static class CatStatuses
    {
        public const byte InCare = 1;
        public const byte WaitingAdoption = 2;
        public const byte Adopted = 3;
        public const byte Closed = 4;
        public const byte Terminated = 5;

        public static string Name(byte status)
        {
            switch (status)
            {
                case InCare:
                    return "在站照料";
                case WaitingAdoption:
                    return "待领养";
                case Adopted:
                    return "已领养";
                case Closed:
                    return "已结案";
                case Terminated:
                    return "已终止";
                default:
                    return "";
            }
        }

        public static bool IsClosed(byte status)
        {
            return status == Closed || status == Terminated;
        }
    }
}
