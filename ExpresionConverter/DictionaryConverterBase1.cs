using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExpresionConverter
{
    public abstract class DictionaryConverterBase1<T, TKey, TValue, TSourceItem> : JsonConverter
    {
        private readonly object[] sources;

        private static readonly Action<JsonWriter, T, object[], Func<KeyValuePair<TKey, TValue>, TSourceItem, JToken>> Converter;

        public DictionaryConverterBase1(params object[] sources)
        {
            this.sources = sources;
        }

        static DictionaryConverterBase1()
        {
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var value = Expression.Parameter(typeof(T), "value");
            var sources = Expression.Parameter(typeof(object[]), "sources");
            var strategy = Expression.Parameter(typeof(Func<KeyValuePair<TKey, TValue>, TSourceItem, JToken>), "strategy");

            var lambda = Expression.Lambda<Action<JsonWriter, T, object[], Func<KeyValuePair<TKey, TValue>, TSourceItem, JToken>>>(
                                                                                                                    Expression.Block(CreateBody(writer, value, sources, strategy)),
                                                                                                                    writer,
                                                                                                                    value,
                                                                                                                    sources,
                                                                                                                    strategy);

            Converter = lambda.Compile();
        }

        private static IEnumerable<Expression> CreateBody(ParameterExpression writer, ParameterExpression value, ParameterExpression sources, ParameterExpression strategy)
        {
            yield return
                Expression.Call(
                                writer,
                                nameof(JsonWriter.WriteStartObject),
                                Type.EmptyTypes);

            foreach (var propInfo in typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
            {
                var propertyValue = Expression.Property(value, propInfo);

                yield return
                    Expression.Call(
                                    writer,
                                    nameof(JsonWriter.WritePropertyName),
                                    Type.EmptyTypes,
                                    Expression.Constant(propInfo.Name));

                DictionarySourceAttribute dictionaryAttribute;
                BoolSourceAttribute boolAttribute;
                TakeFromSourceAttribute takeFromSourceAttribute;
                ListConvertableAttribute listConvertableAttribute;


                if (propInfo.PropertyType.IsConstructedGenericType &&
                         propInfo.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                         propInfo.PropertyType.GenericTypeArguments[0] == typeof(TKey) &&
                         propInfo.PropertyType.GenericTypeArguments[1] == typeof(TValue) &&
                         (dictionaryAttribute = propInfo.GetCustomAttribute<DictionarySourceAttribute>()) != null)
                {
                    var source = Expression.Convert(Expression.Call(
                                                                    typeof(Enumerable),
                                                                    nameof(Enumerable.Single),
                                                                    new[] { typeof(object) },
                                                                    sources,
                                                                    (Expression<Func<object, bool>>)(src => src.GetType() == dictionaryAttribute.SourceType)),
                                                    dictionaryAttribute.SourceType);

                    var sourceValue = Expression.Convert(Expression.Property(source, dictionaryAttribute.SourcePropertyName),
                                                        typeof(TSourceItem));

                    yield return
                        Expression.Call(
                                        writer,
                                        nameof(JsonWriter.WriteStartObject),
                                        Type.EmptyTypes);

                    yield return propertyValue.ForEach((item, body) =>
                                                       {
                                                           var key = Expression.Property(item, "Key");
                                                           body.Add(
                                                                    Expression.Call(
                                                                                    writer,
                                                                                    nameof(JsonWriter.WritePropertyName),
                                                                                    Type.EmptyTypes,
                                                                                    Expression.Call(key, nameof(ToString), Type.EmptyTypes)));

                                                           var kvp = Expression.New(
                                                                                    typeof(KeyValuePair<TKey, TValue>).GetConstructor(new[] { typeof(TKey), typeof(TValue) }),
                                                                                    key,
                                                                                    Expression.Property(item, "Value"));

                                                           body.Add(
                                                                    Expression.Call(
                                                                                    Expression.Invoke(strategy, kvp, sourceValue),
                                                                                    nameof(JToken.WriteTo),
                                                                                    Type.EmptyTypes,
                                                                                    writer,
                                                                                    Expression.Constant(new JsonConverter[0])));
                                                       });

                    yield return
                        Expression.Call(
                                        writer,
                                        nameof(JsonWriter.WriteEndObject),
                                        Type.EmptyTypes);
                }
                else if (propInfo.PropertyType.IsNumeric() &&
                         (boolAttribute = propInfo.GetCustomAttribute<BoolSourceAttribute>()) != null)
                {
                    var source = Expression.Convert(
                                                    Expression.Call(
                                                                    typeof(Enumerable),
                                                                    nameof(Enumerable.Single),
                                                                    new[] { typeof(object) },
                                                                    sources,
                                                                    (Expression<Func<object, bool>>)(src => src.GetType() == boolAttribute.SourceType)),
                                                    boolAttribute.SourceType);

                    var sourceValue = Expression.Property(source, boolAttribute.SourcePropertyName);

                    yield return
                        Expression.Call(
                                        writer,
                                        nameof(JsonWriter.WriteValue),
                                        Type.EmptyTypes,
                                        sourceValue);

                }
                else if ((takeFromSourceAttribute = propInfo.GetCustomAttribute<TakeFromSourceAttribute>()) != null)
                {
                    var source = Expression.Convert(
                                                    Expression.Call(
                                                                    typeof(Enumerable),
                                                                    nameof(Enumerable.Single),
                                                                    new[] { typeof(object) },
                                                                    sources,
                                                                    (Expression<Func<object, bool>>)(src => src.GetType() == takeFromSourceAttribute.SourceType)),
                                                    takeFromSourceAttribute.SourceType);

                    var sourceValue = Expression.Property(source, takeFromSourceAttribute.SourcePropertyName);

                    yield return
                        Expression.Call(
                                        writer,
                                        nameof(JsonWriter.WriteValue),
                                        Type.EmptyTypes,
                                        sourceValue);
                }
                else if ((listConvertableAttribute = propInfo.GetCustomAttribute<ListConvertableAttribute>()) != null)
                {
                    yield return
                        Expression.Call(writer,
                                        nameof(JsonWriter.WriteStartArray),
                                        Type.EmptyTypes);

                    var listSource = Expression.Convert(Expression.Call(
                                                                    typeof(Enumerable),
                                                                    nameof(Enumerable.Single),
                                                                    new[] { typeof(object) },
                                                                    sources,
                                                                    (Expression<Func<object, bool>>)(src => src.GetType() == listConvertableAttribute.PropertyType)),
                                                    listConvertableAttribute.PropertyType);

                    var converterType = listConvertableAttribute.ConverterType;

                    var itemIndex = Expression.Variable(typeof(int), "indexExpression");
                    var index = Expression.Assign(itemIndex, Expression.Constant(0, typeof(int)));

                    yield return listSource.ForEach((item, body) =>
                                                       {
                                                           var converter = Expression.New(converterType.GetConstructor(new[] { typeof(object[]) }), Expression.NewArrayInit(typeof(object), item));
                                                           var target = Expression.ArrayAccess(propertyValue, index);
                                                           var serializedItem = Expression.Call(Expression.Constant(null), typeof(JsonConvert).GetMethod("SerializeObject", BindingFlags.Public | BindingFlags.Static), target, converter);

                                                           body.Add(Expression.Call(
                                                                                    writer,
                                                                                    nameof(JsonWriter.WriteValue),
                                                                                    Type.EmptyTypes,
                                                                                    serializedItem));

                                                           body.Add(Expression.Increment(index));
                                                       });

                    yield return
                        Expression.Call(
                                        writer,
                                        nameof(JsonWriter.WriteEndArray),
                                        Type.EmptyTypes);
                }
                else
                {
                    yield return
                        Expression.Call(
                                        Expression.Call(
                                                        typeof(JToken),
                                                        nameof(JToken.FromObject),
                                                        Type.EmptyTypes,
                                                        Expression.Convert(propertyValue, typeof(object))),
                                        nameof(JToken.WriteTo),
                                        Type.EmptyTypes,
                                        writer,
                                        Expression.Constant(new JsonConverter[0]));
                }
            }

            yield return
                Expression.Call(
                                writer,
                                nameof(JsonWriter.WriteEndObject),
                                Type.EmptyTypes);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Converter(writer, (T)value, sources, ProcessItem);
        }

        protected abstract JToken ProcessItem(KeyValuePair<TKey, TValue> item, TSourceItem sourceValue);
    }
}
