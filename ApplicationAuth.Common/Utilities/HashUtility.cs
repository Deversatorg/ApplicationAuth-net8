using ApplicationAuth.Common.Utilities.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationAuth.Common.Utilities
{
    public class HashUtility : IHashUtility
    {
        public string GetHash(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(inputString);
            var hash = SHA256.HashData(bytes);      // Modern, allocation-free SHA256
            return Convert.ToBase64String(hash);    // Lossless binary-to-string
        }
    }
}
