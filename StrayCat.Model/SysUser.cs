using System;

namespace StrayCat.Model
{
    public class SysUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string RealName { get; set; }
        public byte Role { get; set; }
        public string Phone { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreateTime { get; set; }

        public string RoleName
        {
            get { return Lookups.RoleName(Role); }
        }

        public string EnabledText
        {
            get { return IsEnabled ? "启用" : "停用"; }
        }
    }
}
