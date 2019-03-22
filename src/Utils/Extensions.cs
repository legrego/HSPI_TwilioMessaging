using System;
namespace Hspi.Utils
{
    public static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }
    }
}
