using System.IO;
using System.Text;
using AHI.Infrastructure.SharedKernel.Extension;
using JsonConstant = AHI.Infrastructure.SharedKernel.Extension.Constant;

namespace AHI.Broker.Function.Extension
{
    public static class StringExtension
    {
        public static string RemoveFileToken(this string fileName)
        {
            var index = fileName?.IndexOf("?token=") ?? -1;
            return index < 0 ? fileName ?? string.Empty : fileName.Remove(index);
        }

        public static string JsonSerialize(this object value)
        {
            return Encoding.UTF8.GetString(JsonExtension.Serialize(value));
        }

        public static T JsonDeserialize<T>(this string jsonString)
        {
            using (var reader = new StringReader(jsonString))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
            {
                return JsonConstant.JsonSerializer.Deserialize<T>(jsonReader);
            }
        }
        
        public static string GetCacheKey(this string cacheKey, params object[] args)
        {
            return string.Format(cacheKey, args);
        }
    }
}