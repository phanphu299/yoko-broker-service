using System.Collections.Generic;
using System.Text;
using HashidsNet;

namespace AHI.Broker.Function.Extension
{
    public static class HashidsExtension
    {
        public static string EncodeGuid(this Hashids hasher, params string[] ids)
        {
            return EncodeGuid(hasher, (IEnumerable<string>)ids);
        }

        public static string EncodeGuid(this Hashids hasher, IEnumerable<string> ids)
        {
            var builder = new StringBuilder();
            foreach (var id in ids)
                builder.Append(id);

            return hasher.EncodeHex(builder.ToString());
        }
    }
}