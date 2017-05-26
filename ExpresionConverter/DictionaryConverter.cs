using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExpresionConverter
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

    public class DictionaryConverterDoubleTotal<T> : DictionaryConverterBase1<T, double, Total, double>
    {
        public DictionaryConverterDoubleTotal(params object[] sources) : base(sources)
        {
        }

        protected override JToken ProcessItem(KeyValuePair<double, Total> item, double sourceValue)
        {
            var flag = item.Key < sourceValue;
            return JToken.FromObject(new
            {
                Under = flag,
                Over = !flag
            });
        }
    }

    public class DictionaryConverterStringDouble<T> : DictionaryConverterBase1<T, string, double, double>
    {
        public DictionaryConverterStringDouble(params object[] sources) : base(sources)
        {
        }

        protected override JToken ProcessItem(KeyValuePair<string, double> item, double sourceValue)
        {
            return JToken.FromObject(int.Parse(item.Key) == (int)sourceValue);
        }
    }
}
