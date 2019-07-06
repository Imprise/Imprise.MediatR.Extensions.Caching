using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Imprise.MediatR.Extensions.Caching.Internal
{

    /// <summary>
    /// The IDistributedCache interface is designed to work solely with byte arrays, unlike IMemoryCache which can
    /// take any arbitrary object.
    ///
    /// Microsoft have indicated that they do not intend adding these extension methods to support automatically
    /// serialising objects in several github issue e.g. https://github.com/aspnet/Caching/pull/94
    ///
    /// This implementation was gratefully taken from a Stack Overflow solution posted by dzed at
    /// https://stackoverflow.com/a/50222288/2316834
    ///
    /// Thank you https://www.goodreads.com/book/show/29437996-copying-and-pasting-from-stack-overflow
    /// </summary>
    internal static class DistributedCacheExtensions
    {
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            return SetAsync(cache, key, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(
            this IDistributedCache cache,
            string key,
            T value,
            DistributedCacheEntryOptions options)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, value);
                bytes = memoryStream.ToArray();
            }

            return cache.SetAsync(key, bytes, options);
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var val = await cache.GetAsync(key);
            var result = default(T);

            if (val == null) return result;

            using (var memoryStream = new MemoryStream(val))
            {
                var binaryFormatter = new BinaryFormatter();
                result = (T) binaryFormatter.Deserialize(memoryStream);
            }

            return result;
        }

        public static async Task<(bool Found, T Value)> TryGetAsync<T>(this IDistributedCache cache, string key)
        {
            var cachedValue = await cache.GetAsync(key);
            T value;

            if (cachedValue == null)
            {
                return (false, default(T));
            }

            using (var memoryStream = new MemoryStream(cachedValue))
            {
                var binaryFormatter = new BinaryFormatter();
                value = (T) binaryFormatter.Deserialize(memoryStream);
            }

            return (true, value);
        }
    }
}