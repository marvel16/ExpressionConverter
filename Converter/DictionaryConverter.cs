using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Converter
{
    public class DictionaryConverter<T> : DictionaryConverterBase<T, Total>
    {
        public DictionaryConverter(params object[] sources) : base(sources)
        {
        }

        protected override JToken ProcessDictionaryItem(KeyValuePair<double, Total> item, double sourceValue)
        {
            var flag = item.Key < sourceValue;
            return JToken.FromObject(new
            {
                Under = flag,
                Over = !flag
            });
        }
    }
}
