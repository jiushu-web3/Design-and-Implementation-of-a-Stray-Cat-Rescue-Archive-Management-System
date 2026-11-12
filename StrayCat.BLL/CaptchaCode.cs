using System;
using System.Security.Cryptography;

namespace StrayCat.BLL
{
    public static class CaptchaCode
    {
        public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public const int Length = 4;

        public static string Create()
        {
            char[] chars = new char[Length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[1];
                int index = 0;
                int limit = (256 / Alphabet.Length) * Alphabet.Length;
                while (index < Length)
                {
                    rng.GetBytes(buffer);
                    if (buffer[0] >= limit)
                    {
                        continue;
                    }
                    chars[index] = Alphabet[buffer[0] % Alphabet.Length];
                    index++;
                }
            }
            return new string(chars);
        }

        public static bool IsAlphabet(string value)
        {
            if (value == null || value.Length != Length)
            {
                return false;
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (Alphabet.IndexOf(value[i]) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Matches(string expected, string actual)
        {
            if (!IsAlphabet(expected) || actual == null)
            {
                return false;
            }
            return string.Equals(expected, actual.Trim(), StringComparison.Ordinal);
        }
    }
}
