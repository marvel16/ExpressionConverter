using System;

namespace ExpresionConverter
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class GameMarketsBaseAttribute : Attribute
    {
        public Type SourceType { get; set; } = typeof(GameStats);
        public string SourcePropertyName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DictionarySourceAttribute : GameMarketsBaseAttribute
    {
    }
}