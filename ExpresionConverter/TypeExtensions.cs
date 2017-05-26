using System;

namespace ExpresionConverter
{
    public static class TypeExtensions
    {
        public static bool IsNumeric(this Type @this)
        {
            var typeCode = (int)Type.GetTypeCode(@this);
            return typeCode > 5 && typeCode < 15;
        }
    }
}
