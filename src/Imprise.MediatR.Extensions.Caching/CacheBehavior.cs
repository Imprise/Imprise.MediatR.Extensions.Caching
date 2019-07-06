using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Imprise.MediatR.Extensions.Caching
{
    /// <summary>
    /// When injected into a MediatR pipeline, this behavior will receive a list of instances of ICache<TRequest, TResponse>
    /// instances for every class in your project that implements this interface for the given generic types.
    ///
    /// When the request pipeline runs, the behavior will first check to see if the response for this request type is
    /// already in the cache and if so will short-circuit the request pipeline and return the cached response.
    ///
    /// If the request response is not in the cache, the next request handler in the pipeline will be called and the
    /// response cached using the logic and settings in the derived implementation of ICache<TRequest, TResponse>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request that will have it's result cached.</typeparam>
    /// <typeparam name="TResponse">The response of the request to be cached.</typeparam>
    public class CacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly List<ICache<TRequest, TResponse>> _caches;

        public CacheBehavior(IEnumerable<ICache<TRequest, TResponse>> cachedRequests)
        {
            _caches = cachedRequests.ToList();
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            // It's possible an ICache for the same request could be added more than once but just try and get the first one
            var cacheRequest = _caches.FirstOrDefault();
            if (cacheRequest == null)
            {
                // A cache request handler implementation for this request was not found, do nothing and continue
                return await next();
            }

            // try and get the response out of cache for this request
            var cachedResult = await cacheRequest.Get(request);

            if (cachedResult != null)
            {
                // cached response found, return and short-circuit the pipeline
                return cachedResult;
            }

            // No cached response was found so continue the handler pipeline and cache the result
            var result = await next();
            await cacheRequest.Set(request, result);
            return result;
        }
    }
}