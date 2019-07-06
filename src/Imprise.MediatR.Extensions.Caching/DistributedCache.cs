using System;
using System.Threading.Tasks;
using Imprise.MediatR.Extensions.Caching.Internal;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Imprise.MediatR.Extensions.Caching
{
    /// <summary>
    /// Implement this class when you want your cached request response to be stored in a distributed cached
    /// </summary>
    /// <typeparam name="TRequest">The type of the request who's response will be cached.</typeparam>
    /// <typeparam name="TResponse">The type of the response of the request that will be cached.</typeparam>
    public abstract class DistributedCache<TRequest, TResponse> : ICache<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IDistributedCache _distributedCache;

        protected virtual DateTime? AbsoluteExpiration { get; }
        protected virtual TimeSpan? AbsoluteExpirationRelativeToNow { get; }
        protected virtual TimeSpan? SlidingExpiration { get; }

        protected DistributedCache(
            IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// Override and return a string key to uniquely identify the cached response
        /// </summary>
        /// <param name="request">The type of the request who's response will be cached.</param>
        /// <returns></returns>
        protected abstract string GetCacheKeyIdentifier(TRequest request);

        private static string GetCacheKey(string id)
        {
            return $"{typeof(TRequest).FullName}:{id}";
        }

        private string GetCacheKey(TRequest request)
        {
            return GetCacheKey(GetCacheKeyIdentifier(request));
        }

        public virtual async Task<TResponse> Get(TRequest request)
        {
            var response = await _distributedCache.GetAsync<TResponse>(GetCacheKey(request));
            return response == null ? default(TResponse) : response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async Task Set(TRequest request, TResponse value)
        {
            await _distributedCache.SetAsync(
                GetCacheKey(request),
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = AbsoluteExpiration,
                    AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = SlidingExpiration
                });
        }

        /// <summary>
        /// Removes the response from cache using the CacheKeyIdentifier from the Request
        /// </summary>
        /// <param name="cacheKeyIdentifier">A string identifier that uniquely identifies the response to be removed</param>
        /// <returns></returns>
        public async Task Remove(string cacheKeyIdentifier)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(cacheKeyIdentifier));
        }
    }
}