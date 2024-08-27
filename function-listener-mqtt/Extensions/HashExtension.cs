using System;
using System.Security.Cryptography;

namespace AHI.Broker.Function.Extension
{
    public static class HashExtension
    {
        public static string CalculateMd5Hash(this byte[] input)
        {
            // file deepcode ignore InsecureHash: Should discuss the business impact when change to another hash algorithm
            using MD5 mD = MD5.Create();
            byte[] array = mD.ComputeHash(input);
            return BitConverter.ToString(array).Replace("-", string.Empty);
        }
    }
}