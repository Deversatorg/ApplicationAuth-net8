using ApplicationAuth.Common.Utilities.Interfaces;
using System.Text;

namespace ApplicationAuth.Common.Utilities
{
    public class HashUtility : IHashUtility
    {
        public string GetHash(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return "";

            byte[] data = Encoding.ASCII.GetBytes(inputString);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            string hash = Encoding.ASCII.GetString(data);
            return hash;
        }
    }
}
