using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace StsExample.Helpers
{
    public static class TransformXsltHelper
    {
        private static readonly TypedMemoryCache<XslCompiledTransform> Cache = new TypedMemoryCache<XslCompiledTransform>("XslCompiledTransformCache");
        public static string Transform<T>(T model, string transformFilePath)
        {
            Cache.TryGetOrSet(transformFilePath, () =>
            {
                using (var reader = XmlReader.Create(new FileStream(transformFilePath, FileMode.Open, FileAccess.Read)))
                {
                    var transform = new XslCompiledTransform();
                    transform.Load(reader);
                    return transform;
                }
            }, out var cachedTransform);
            var transformedResult = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            var ms = new MemoryStream();
            serializer.Serialize(ms, model);
            ms.Seek(0, SeekOrigin.Begin);
            using (var reader = XmlReader.Create(new StreamReader(ms)))
            {
                cachedTransform.Transform(reader, null, transformedResult);
            }
            return transformedResult.ToString();
        }
    }
}