using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Converter
{
    public abstract class DictionaryConverterBase<T, TValue> : JsonConverter
    {
        private readonly object[] sources;

        private static readonly Action<JsonWriter, T, object[], Func<KeyValuePair<double, TValue>, double, JToken>> Converter;

        public DictionaryConverterBase(params object[] sources)
        {
            this.sources = sources;
        }

        static DictionaryConverterBase()
        {
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var value = Expression.Parameter(typeof(T), "value");
            var sources = Expression.Parameter(typeof(object[]), "sources");
            var dictStrategy = Expression.Parameter(typeof(Func<KeyValuePair<double, TValue>, double, JToken>), "dictStrategy");

            var lambda = 
                Expression
                    .Lambda<Action<JsonWriter, T, object[], Func<KeyValuePair<double, TValue>, double, JToken>>> (
                        Expression.Block(CreateBody(writer, value, sources, dictStrategy)),
                        writer,
                        value,
                        sources,
                        dictStrategy);

            Converter = lambda.Compile();
        }

        private static IEnumerable<Expression> CreateBody(ParameterExpression writer, ParameterExpression value, ParameterExpression sources, ParameterExpression dictStrategy)
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

                if (propInfo.PropertyType.IsConstructedGenericType &&
                    propInfo.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                    propInfo.PropertyType.GenericTypeArguments[0].IsNumeric() &&
                    propInfo.PropertyType.GenericTypeArguments[1] == typeof(TValue) &&
                    (dictionaryAttribute = propInfo.GetCustomAttribute<DictionarySourceAttribute>()) != null)
                {
                    var source = Expression.Convert(
                        Expression.Call(
                            typeof(Enumerable),
                            nameof(Enumerable.Single),
                            new[] { typeof(object) },
                            sources,
                            (Expression<Func<object, bool>>)(src => src.GetType() == dictionaryAttribute.SourceType)),
                        dictionaryAttribute.SourceType);

                    var sourceValue = Expression.Convert(
                        Expression.Property(
                            source,
                            dictionaryAttribute.SourcePropertyName),
                        typeof(double));

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
                            typeof(KeyValuePair<double, TValue>).GetConstructor(new[] { typeof(double), typeof(TValue) }),
                            Expression.Convert(key, typeof(double)),
                            Expression.Property(item, "Value"));

                        body.Add(
                            Expression.Call(
                                Expression.Invoke(dictStrategy, kvp, sourceValue),
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
            Converter(writer, (T) value, sources, ProcessDictionaryItem);
        }

        protected abstract JToken ProcessDictionaryItem(KeyValuePair<double, TValue> item, double sourceValue);
    }
}