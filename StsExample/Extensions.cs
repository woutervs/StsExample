using System;
using System.Security.Cryptography;
using System.Text;

namespace StsExample
{
    public static class Extensions
    {
        public static string Hash(this string input)
        {
            HashAlgorithm hashAlgorithm = SHA512.Create();
            var byteValue = Encoding.UTF8.GetBytes(input);
            var byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }
    }
}