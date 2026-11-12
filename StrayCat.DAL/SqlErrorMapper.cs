using System;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public static class SqlErrorMapper
    {
        public static string ToMessage(int number, string message)
        {
            if (number >= 50000)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return null;
                }
                return message.Trim();
            }

            if (number != 2601 && number != 2627)
            {
                return null;
            }

            string text = message ?? "";
            if (text.IndexOf("UQ_SysUser_UserName", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Messages.UserNameExists;
            }
            if (text.IndexOf("UX_AdoptionApplication_Pending", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Messages.ApplicationExists;
            }
            if (text.IndexOf("UQ_CatArchive_CatNo", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Messages.CatNoConflict;
            }
            if (text.IndexOf("UQ_RescueRecord_CatId", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Messages.RescueExists;
            }
            if (text.IndexOf("UX_Adoption_OneActive", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Messages.ActiveAdoptionExists;
            }
            return null;
        }
    }
}
