using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StsExample.Helpers
{
    public class StreamChangingMiddleware : OwinMiddleware
    {
        public StreamChangingMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var stream = context.Response.Body;
            context.Response.Body = new MemoryStream();
            await Next.Invoke(context);
            context.Response.Body.CopyTo(stream);
            context.Response.Body = stream;
        }
    }
    public class LoggingMiddleware : OwinMiddleware
    {
        private readonly LoggingMiddlewareOptions options;
        private readonly JsonSerializerSettings jsonSettings;

        public LoggingMiddleware(OwinMiddleware next, LoggingMiddlewareOptions options) : base(next)
        {
            this.options = options;
            jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new JsonStreamResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                Trace.WriteLine($"========= Request before: {options?.Stage ?? "unknown"} =========");
                Trace.WriteLine(JsonConvert.SerializeObject(context.Request, jsonSettings));
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            await Next.Invoke(context);
            try
            {
                Trace.WriteLine($"============ Response after: {options?.Stage} ============");
                Trace.WriteLine(JsonConvert.SerializeObject(context.Response, jsonSettings));
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }
    }

    public class LoggingMiddlewareOptions
    {
        public string Stage { get; set; }
    }

    public class JsonStreamResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (new[] { "Environment", "Context" }.ToList().Contains(property.PropertyName))
            {
                property.Ignored = true;
            }
            if (property.PropertyType.IsAssignableFrom(typeof(Stream)))
            {
                property.Converter = new JsonStreamConverter();
            }
            return property;
        }
    }

    public class JsonStreamConverter : JsonConverter<Stream>
    {
        public override void WriteJson(JsonWriter writer, Stream stream, JsonSerializer serializer)
        {
            if (stream.CanRead)
            {
                using (var sr = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
                {
                    var result = sr.ReadToEnd();
                    stream.Seek(0, SeekOrigin.Begin);
                    writer.WriteValue(result);
                }
            }
        }

        public override Stream ReadJson(JsonReader reader, Type objectType, Stream existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}