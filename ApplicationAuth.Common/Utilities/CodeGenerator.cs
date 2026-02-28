using System;

namespace ApplicationAuth.Common.Utilities
{
    public static class CodeGenerator
    {
        public static string GenerateCode() => new Random().Next(1000, 9999).ToString();
    }
}
