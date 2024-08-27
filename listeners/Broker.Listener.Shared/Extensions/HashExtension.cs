using System.Security.Cryptography;

namespace Broker.Listener.Shared.Extensions;

public static class HashExtension
{
    public static string CalculateMd5Hash(this byte[] input)
    {
        using var md5 = MD5.Create();
        byte[] array = md5.ComputeHash(input);
        return BitConverter.ToString(array).Replace("-", string.Empty);
    }
}
