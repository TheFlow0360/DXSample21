using System;
using System.Linq;

namespace DXSample21
{
    public static class EnumExtensions
    {
        public static Boolean In<T>(this T val, params T[] values) where T : struct
        {
            return values.Contains(val);
        }
    }
}