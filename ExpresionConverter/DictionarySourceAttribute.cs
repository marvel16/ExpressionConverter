using System;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace ExpresionConverter
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DictionarySourceAttribute : Attribute
    {
        public Type SourceType { get; set; } = typeof(GameStats);
        public string SourcePropertyName { get; set; }
        public virtual Func<object, object, object, JToken> ProcessStrategy { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DictionaryTotalAttribute : DictionarySourceAttribute
    {
        public override Func<object, object, object, JToken> ProcessStrategy => (key, value, source) =>
                                                                                                JToken.FromObject(new
                                                                                                {
                                                                                                    Under = (double)key < (double)source,
                                                                                                    Over = (double)key > (double)source
                                                                                                });
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DictionaryHandicapsAttribute : DictionarySourceAttribute
    {
        public override Func<object, object, object, JToken> ProcessStrategy => (key, value, source) => 
        {
            return JToken.FromObject(double.Parse((string) key) > (int)source);
        };

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DictionaryExactAttribute : DictionarySourceAttribute
    {
        public override Func<object, object, object, JToken> ProcessStrategy => (key, value, source) =>
                                                                                {
                                                                                    return JToken.FromObject(double.Parse((string)key) - (int)source < 0.001);
                                                                                };
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DictionaryExactStringAttribute : DictionarySourceAttribute
    {
        public override Func<object, object, object, JToken> ProcessStrategy => (key, value, source) =>
        {
            return JToken.FromObject((string) key == (string) source);
        };
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DictionaryRangeAttribute : DictionarySourceAttribute
    {
        public override Func<object, object, object, JToken> ProcessStrategy => (key, value, source) =>
                                                                                {
                                                                                    bool ret = false;
                                                                                    if (key.ToString().StartsWith(">"))
                                                                                    {
                                                                                        ret = double.Parse(((string) key).Substring(1)) < (int) source;
                                                                                    }
                                                                                    else if(key.ToString().StartsWith("<"))
                                                                                    {
                                                                                        ret = double.Parse(((string)key).Substring(1)) > (int)source;
                                                                                    }

                                                                                    return JToken.FromObject(ret);
                                                                                };

    }
}