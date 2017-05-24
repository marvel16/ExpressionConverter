using System.Collections.Generic;
using Common;
using Newtonsoft.Json.Linq;

namespace Converter
{
    public class DictionaryConverter<T> : DictionaryConverterBase<T, Total>
    {
        public DictionaryConverter(params object[] sources) : base(sources)
        {
        }

        protected override JToken ProcessKvpWithDoubleKey(KeyValuePair<double, Total> item, double sourceValue)
        {
            var flag = item.Key < sourceValue;
            return JToken.FromObject(new
            {
                Under = flag,
                Over = !flag
            });
        }

        protected override JToken ProcessKvpWithStringKey(KeyValuePair<string, double> item, int sourceValue)
        {
            return JToken.FromObject(int.Parse(item.Key) == sourceValue);
        }
    }
}
