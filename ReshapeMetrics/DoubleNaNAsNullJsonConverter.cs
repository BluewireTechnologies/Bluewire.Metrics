using System;
using Newtonsoft.Json;

namespace ReshapeMetrics
{
    public class DoubleNaNAsNullJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null || double.NaN.Equals(value))
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue((double)value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return double.NaN;
            if (reader.TokenType == JsonToken.Undefined) return double.NaN;
            if (reader.TokenType == JsonToken.Float) return (double)reader.Value;
            if (reader.TokenType == JsonToken.String) return double.Parse(reader.Value.ToString());
            throw new JsonReaderException($"DoubleNaNAsNullJsonConverter expected to find a number, but found a {reader.TokenType}");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }
    }
}
