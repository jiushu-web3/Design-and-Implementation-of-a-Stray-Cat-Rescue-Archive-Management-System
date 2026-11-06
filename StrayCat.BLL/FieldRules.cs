using System;
using System.Text.RegularExpressions;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public static class FieldRules
    {
        private static readonly Regex UserNamePattern = new Regex("^[A-Za-z0-9]{4,20}$");
        private static readonly Regex PhonePattern = new Regex("^1[3-9][0-9]{9}$");

        public static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static string Required(string value, int maxLength)
        {
            string text = (value ?? "").Trim();
            if (text.Length == 0)
            {
                return Messages.RequiredField;
            }
            if (text.Length > maxLength)
            {
                return Messages.RequiredField;
            }
            return null;
        }

        public static string Optional(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (value.Trim().Length > maxLength)
            {
                return Messages.RequiredField;
            }
            return null;
        }

        public static string UserName(string value)
        {
            if (!UserNamePattern.IsMatch((value ?? "").Trim()))
            {
                return Messages.UserNameRule;
            }
            return null;
        }

        public static string Password(string value)
        {
            if (value == null || value.Length < 6 || value.Length > 20)
            {
                return Messages.PasswordRule;
            }
            return null;
        }

        public static string Phone(string value, bool required)
        {
            string text = (value ?? "").Trim();
            if (text.Length == 0)
            {
                return required ? Messages.PhoneRule : null;
            }
            if (!PhonePattern.IsMatch(text))
            {
                return Messages.PhoneRule;
            }
            return null;
        }

        public static string Choice(string value, string[] options, string message)
        {
            string text = (value ?? "").Trim();
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == text)
                {
                    return null;
                }
            }
            return message;
        }

        public static string NotAfterToday(DateTime value)
        {
            if (value.Date > DateTime.Today)
            {
                return Messages.DateAfterToday;
            }
            return null;
        }

        public static string NotBefore(DateTime value, DateTime minimum, string message)
        {
            if (value.Date < minimum.Date)
            {
                return message;
            }
            return null;
        }

        public static string RealDate(DateTime value)
        {
            if (value.Year < 1990 || value.Year > 9999)
            {
                return Messages.BadDate;
            }
            return null;
        }

        public static string First(params string[] errors)
        {
            if (errors == null)
            {
                return null;
            }
            for (int i = 0; i < errors.Length; i++)
            {
                if (errors[i] != null)
                {
                    return errors[i];
                }
            }
            return null;
        }
    }
}
