using System;

namespace Converter
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TakeFromSourceAttribute : Attribute
    {
        public Type SourceType { get; set; }
        public string SourcePropertyName { get; set; }
    }
}