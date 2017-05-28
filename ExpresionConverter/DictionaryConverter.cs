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
    public class DictionaryConverter<T, TKey, TValue, TSourceItem> : JsonConverter
    {
        private readonly object[] sources;

        private static readonly Action<JsonWriter, T, object[]> Converter;

        public DictionaryConverter(params object[] sources)
        {
            this.sources = sources;
        }

        static DictionaryConverter()
        {
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var value = Expression.Parameter(typeof(T), "value");
            var sources = Expression.Parameter(typeof(object[]), "sources");

            var lambda = Expression.Lambda<Action<JsonWriter, T, object[]>>(Expression.Block(CreateBody(writer, value, sources)),
                                                                            writer,
                                                                            value,
                                                                            sources);

            Converter = lambda.Compile();
        }

        private static IEnumerable<Expression> CreateBody(ParameterExpression writer, ParameterExpression value, ParameterExpression sources)
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
                    (dictionaryAttribute = (DictionarySourceAttribute)propInfo.GetCustomAttributes().FirstOrDefault(atr => atr is DictionarySourceAttribute)) != null)
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

                    yield return Expression.Call(
                                                 writer,
                                                 nameof(JsonWriter.WriteStartObject),
                                                 Type.EmptyTypes);

                    var processStrategy = Expression.Constant(dictionaryAttribute.ProcessStrategy);

                    yield return propertyValue.ForEach((item, body) =>
                                                       {
                                                           var key = Expression.Property(item, "Key");
                                                           body.Add(
                                                                    Expression.Call(
                                                                                    writer,
                                                                                    nameof(JsonWriter.WritePropertyName),
                                                                                    Type.EmptyTypes,
                                                                                    Expression.Call(key, nameof(ToString), Type.EmptyTypes)));

                                                           var k = Expression.Convert(key, typeof(object));
                                                           var v = Expression.Convert(Expression.Property(item, "Value"), typeof(object));
                                                           var s = Expression.Convert(sourceValue, typeof(object));

                                                           body.Add(Expression.Call(
                                                                                    Expression.Invoke(processStrategy, k, v, s),
                                                                                    nameof(JToken.WriteTo),
                                                                                    Type.EmptyTypes,
                                                                                    writer,
                                                                                    Expression.Constant(new JsonConverter[0])));
                                                       });

                    yield return Expression.Call(
                                                 writer,
                                                 nameof(JsonWriter.WriteEndObject),
                                                 Type.EmptyTypes);
                }
                else if (propInfo.PropertyType.IsNumeric() && (boolAttribute = propInfo.GetCustomAttribute<BoolSourceAttribute>()) != null)
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

                    yield return listSource.ForEach((index, item, body) =>
                                                    {
                                                        var converter = Expression.Convert(Expression.New(converterType.GetConstructor(new[] { typeof(object[]) }), Expression.NewArrayInit(typeof(object), item)), typeof(JsonConverter));
                                                        var objectToSerialize = Expression.Convert(Expression.Property(propertyValue, "Item", index), typeof(object));

                                                        var serializedItem = Expression.Call(null, typeof(JsonConvert).GetMethod(nameof(JsonConvert.SerializeObject), new[] { typeof(object), typeof(JsonConverter[]) }),
                                                                                             objectToSerialize, Expression.NewArrayInit(typeof(JsonConverter), converter));

                                                        body.Add(Expression.Call(writer,
                                                                                 nameof(JsonWriter.WriteValue),
                                                                                 Type.EmptyTypes,
                                                                                 Expression.Convert(serializedItem, typeof(object))));
                                                    });

                    yield return
                        Expression.Call(
                                        writer,
                                        nameof(JsonWriter.WriteEndArray),
                                        Type.EmptyTypes);
                }
                else
                {
                    yield return Expression.Call(Expression.Call(
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
            Converter(writer, (T)value, sources);
        }

    }
}
