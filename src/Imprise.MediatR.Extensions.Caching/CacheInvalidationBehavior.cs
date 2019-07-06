using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Imprise.MediatR.Extensions.Caching
{
    /// <summary>
    /// When injected into a MediatR pipeline, this behavior will receive a list of instances of ICacheInvalidator<TRequest>
    /// instances for every class in your project that implements this interface for the given generic types.
    ///
    /// When the request pipeline runs, the behavior will make sure the curent request runs through the pipeline and
    /// after it returns will proceed to call Invalidate on any ICacheInvalidator instance in the list of invalidators. 
    /// </summary>
    /// <typeparam name="TRequest">The type of the request that needs to invalidate other cached request responses.</typeparam>
    /// <typeparam name="TResponse">The response of the request that causes invalidation of other cached request responses.</typeparam>
    public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly List<ICacheInvalidator<TRequest>> _cacheInvalidators;

        public CacheInvalidationBehavior(IEnumerable<ICacheInvalidator<TRequest>> cacheInvalidators)
        {
            _cacheInvalidators = cacheInvalidators.ToList();
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            // run through the request handler pipeline for this request
            var result = await next();

            // now loop through each cache invalidator for this request type and call the Invalidate method passing
            // an instance of this request in order to retrieve a cache key.
            foreach (var invalidator in _cacheInvalidators)
            {
                await invalidator.Invalidate(request);
            }

            return result;
        }
    }
}