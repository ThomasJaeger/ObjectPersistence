using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UtilityFunctions
{
    /// <summary>
    /// Common utility functions that can be used system wide
    /// </summary>
    public class Utility
    {
        private const string _matchEmailPattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@" +
                                                  @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\." +
                                                  @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|" +
                                                  @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return Regex.IsMatch(email, _matchEmailPattern);
        }

        /// <summary>
        /// Calulcate an SH1 hash value as string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CalculateSHA1Hash(string input)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = sha1.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
