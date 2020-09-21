using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Confluent.Kafka;

namespace MyKafkaConsumer
{
    public class MyDeserializer<T>: IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            try
            {
                var array = data.ToArray();
                using var stream = new MemoryStream(array);
                using var sr = new StreamReader(stream, Encoding.UTF8);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
