using System;

namespace ExpresionConverter
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ListConvertableAttribute : Attribute
    {
        public Type ConverterType { get; set; }
        public Type PropertyType { get; set; }

        public ListConvertableAttribute(Type converterType, Type propertyType)
        {
            ConverterType = converterType;
            PropertyType = propertyType;
        }
    }
}
