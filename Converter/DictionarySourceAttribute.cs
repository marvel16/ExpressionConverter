using System;

namespace Converter
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DictionarySourceAttribute : Attribute
    {
        public Type SourceType { get; set; }

        public string SourcePropertyName { get; set; }
    }
}