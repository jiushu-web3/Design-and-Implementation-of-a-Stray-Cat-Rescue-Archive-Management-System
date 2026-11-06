namespace StrayCat.Model
{
    public class CurrentUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public byte Role { get; set; }

        public bool IsAdmin
        {
            get { return Role == RoleCodes.Admin; }
        }
    }
}
