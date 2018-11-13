﻿using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aq.ExpressionJsonSerializer
{
    public class ExpressionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(Expression)
               || objectType.IsSubclassOf(typeof(Expression));

        public override void WriteJson(
            JsonWriter writer, 
            object value, 
            JsonSerializer serializer) 
            => Serializer.Serialize(writer, serializer, (Expression) value);

        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object existingValue, 
            JsonSerializer serializer) 
            => Deserializer.Deserialize(JToken.ReadFrom(reader));
    }
}
