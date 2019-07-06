using System.Threading.Tasks;
using MediatR;

namespace Imprise.MediatR.Extensions.Caching
{
    /// <summary>
    /// Inherit from this class when you need to handle the case of one request type (TReques) invalidating the cached
    /// response (TCachedResponse) of a difference cached request (TCachedRequest)
    ///
    /// e.g. GetUser : IRequest<User>  has cached the response to User. Another request UpdateUser : IRequest should
    /// invalidate this request. We can create a new class UpdateUserCacheInvalidator : CacheInvalidator<UpdateUser, GetUser, User>
    /// that will cause the GetUser for a given user to be invalidated
    /// </summary>
    /// <typeparam name="TRequest">The type of the request that will run and cause a different cached request to be invalidated.</typeparam>
    /// <typeparam name="TCachedRequest">The type of the request that has been cached and should be invalidated by TRequest</typeparam>
    /// <typeparam name="TCachedResponse">The type of the response that is cached by a TCachedRequest.</typeparam>
    public abstract class
        CacheInvalidator<TRequest, TCachedRequest, TCachedResponse> : ICacheInvalidator<TRequest>
        where TCachedRequest : IRequest<TCachedResponse>
    {
        private readonly ICache<TCachedRequest, TCachedResponse> _cache;

        protected CacheInvalidator(ICache<TCachedRequest, TCachedResponse> cache)
        {
            _cache = cache;
        }

        protected abstract string GetCacheKeyIdentifier(TRequest request);

        /// <summary>
        /// Removes the cached response entry for a request identified by this requests CacheKeyIdentifier
        /// </summary>
        /// <param name="request">The type of the request that will cause another cached request to be invalidated</param>
        /// <returns></returns>
        public async Task Invalidate(TRequest request)
        {
            await _cache.Remove(GetCacheKeyIdentifier(request));
        }
    }
}