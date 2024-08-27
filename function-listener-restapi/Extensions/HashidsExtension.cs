using System;
using System.Collections.Generic;
using HashidsNet;

namespace AHI.Broker.Function.Extension
{
    public static class HashidsExtension
    {
        public static string[] DecodeGuid(this Hashids hasher, string hash)
        {
            var decoded = hasher.DecodeHex(hash);
            if (string.IsNullOrEmpty(decoded) || decoded.Length % 32 != 0)
                return Array.Empty<string>();

            var result = new List<string>();
            for (int startIndex = 0; startIndex < decoded.Length; startIndex += 32)
            {
                var value = Guid.Parse(decoded.Substring(startIndex, 32)).ToString();
                result.Add(value);
            }

            return result.ToArray();
        }
    }
}